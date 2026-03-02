using Dapper;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text.Json;
using System.Diagnostics;
using StackExchange.Profiling;
using MSConfig = Microsoft.Extensions.Configuration.IConfiguration;

namespace FilmKirala.Report.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _exportPath;
        private readonly MSConfig _configuration;

        private static readonly ConcurrentQueue<(string JobId, bool IsCsv)> _reportQueue = new();

        public ReportService(AppDbContext context, IServiceScopeFactory scopeFactory, MSConfig configuration)
        {
            _context = context;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            var parentDir = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();
            _exportPath = Path.Combine(parentDir, "FilmKiralaExports");
        }

        public string EnqueueReport(bool isCsv)
        {
            var jobId = Guid.NewGuid().ToString().Substring(0, 8);
            _reportQueue.Enqueue((jobId, isCsv));
            return jobId;
        }

        public static bool TryDequeue(out (string JobId, bool IsCsv) job) => _reportQueue.TryDequeue(out job);

        public async Task CreateLargeReportInBackgroundAsync(string jobId, bool isCsv)
        {
            // MiniProfiler'ın arka plan işlerinde çalışabilmesi için scope oluşturuyoruz
            using (MiniProfiler.Current.Step($"Arka Plan Raporu: {jobId}"))
            {
                Stopwatch sw = Stopwatch.StartNew();
                if (!Directory.Exists(_exportPath)) Directory.CreateDirectory(_exportPath);
                var fullPath = Path.Combine(_exportPath, $"Report_{jobId}.{(isCsv ? "csv" : "xlsx")}");

                if (File.Exists(fullPath)) File.Delete(fullPath);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    using var conn = new Microsoft.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

              
                    Dictionary<int, int> rentalDict;
                    using (MiniProfiler.Current.Step("DB: Kiralama Sayılarını Gruplayarak Çekme"))
                    {
                        var rentals = await SqlMapper.QueryAsync<(int MovieId, int Count)>(conn,
                            "SELECT MovieId, COUNT(*) as Count FROM Rentals WITH (NOLOCK) GROUP BY MovieId",
                            commandTimeout: 300);
                        rentalDict = rentals.ToDictionary(x => x.MovieId, x => x.Count);
                    }

                    int batchSize = 40000;
                    int lastId = 0;
                    int processedCount = 0;
                    bool isFirstBatch = true;
                    int totalMovies = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Movies WITH (NOLOCK)");

                    while (true)
                    {
                        List<dynamic> movies;
                        using (MiniProfiler.Current.Step($"Batch Çekiliyor (LastId: {lastId})"))
                        {
                            movies = (await SqlMapper.QueryAsync(conn,
                                "SELECT Id, Title, Genre FROM Movies WHERE Id > @lastId ORDER BY Id OFFSET 0 ROWS FETCH NEXT @batchSize ROWS ONLY",
                                new { lastId, batchSize },
                                commandTimeout: 120)).ToList();
                        }

                        if (!movies.Any()) break;
               
                        List<object> currentBatchRows;
                        using (MiniProfiler.Current.Step("PLINQ: Mapping İşlemi"))
                        {
                            var activeThreads = new ConcurrentDictionary<int, byte>();
                            currentBatchRows = movies.AsParallel()
                                .WithDegreeOfParallelism(Environment.ProcessorCount)
                                .Select(m => {
                                    activeThreads.TryAdd(Thread.CurrentThread.ManagedThreadId, 0);
                                    return new
                                    {
                                        FilmId = (int)m.Id,
                                        FilmAdi = (string)m.Title,
                                        Tur = (string)m.Genre,
                                        KiralamaSayisi = rentalDict.GetValueOrDefault((int)m.Id, 0)
                                    };
                                }).Cast<object>().ToList();

                           
                            var threadList = string.Join(",", activeThreads.Keys.OrderBy(x => x));
                            double progress = (double)(processedCount + movies.Count) / totalMovies * 100;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($"\r[BATCH] %{progress:F1} | İşçiler: [{threadList}] | Job: {jobId}");
                            Console.ResetColor();
                        }
                    
                        using (MiniProfiler.Current.Step("IO: MiniExcel ile Diske Yazma"))
                        {
                            if (isFirstBatch)
                            {
                                await MiniExcel.SaveAsAsync(fullPath, currentBatchRows, excelType: isCsv ? ExcelType.CSV : ExcelType.XLSX);
                                isFirstBatch = false;
                            }
                            else
                            {
                                await MiniExcel.InsertAsync(fullPath, currentBatchRows, excelType: isCsv ? ExcelType.CSV : ExcelType.XLSX);
                            }
                        }

                        processedCount += movies.Count;
                        lastId = (int)movies.Last().Id;
                    }

                    using (MiniProfiler.Current.Step("DB: Bildirim Kaydı Atılıyor"))
                    {
                        var notification = new NotificationLog(
                            userEmail: "admin@filmkirala.com",
                            subject: "Rapor Hazır",
                            message: $"Rapor bitti. JobId: {jobId} | Süre: {sw.Elapsed.TotalSeconds:F1}s",
                            isSent: false,
                            createdAt: DateTime.Now,
                            sentAt: DateTime.Now,
                            errorMessage: string.Empty,
                            type: isCsv ? "CSV" : "XLSX"
                        );
                        db.NotificationLogs.Add(notification);
                        await db.SaveChangesAsync();
                    }
                }
                sw.Stop();
                Console.WriteLine($"\n Rapor Bitti! JobId: {jobId} | Süre: {sw.Elapsed.TotalSeconds:F1} sn.\n");
            }
        }
        public async Task<(bool IsReady, byte[]? FileBytes, string? FileName)> GetReportFileAsync(string jobId)
        {
            if (!Directory.Exists(_exportPath)) return (false, null, null);

            var files = Directory.GetFiles(_exportPath, $"Report_{jobId}.*");
            if (files.Length == 0) return (false, null, null);

            var filePath = files[0];

            try
            {
                // FileShare.ReadWrite kullanarak dosya o an yazılsa bile okuyabiliyoruz
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        return (true, ms.ToArray(), Path.GetFileName(filePath));
                    }
                }
            }
            catch (IOException)
            {
                return (false, null, null);
            }
        }
        public async Task<object> GetMovieSummaryAsync(int lastId, int pageSize)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

            var movies = (await SqlMapper.QueryAsync(conn,
                "SELECT Id, Title, Genre, Stock FROM Movies WHERE Id > @lastId ORDER BY Id OFFSET 0 ROWS FETCH NEXT @pageSize ROWS ONLY",
                new { lastId, pageSize })).ToList();

            if (!movies.Any()) return new { Count = 0, Data = new List<object>() };

            var movieIds = movies.Select(m => (int)m.Id).ToList();
            string jsonIds = JsonSerializer.Serialize(movieIds);

            var rentalCounts = await SqlMapper.QueryAsync<(int MovieId, int Count)>(conn,
                @"SELECT r.MovieId, COUNT(*) as Count FROM Rentals r WHERE r.MovieId IN (SELECT value FROM OPENJSON(@jsonIds) WITH (value int '$')) GROUP BY r.MovieId",
                new { jsonIds });

            var rentalDict = rentalCounts.ToDictionary(x => x.MovieId, x => x.Count);

            var resultData = movies.Select(m => new {
                Id = (int)m.Id,
                Title = (string)m.Title,
                Genre = (string)m.Genre,
                Stock = (int)m.Stock,
                RentalCount = rentalDict.GetValueOrDefault((int)m.Id, 0)
            }).ToList();

            return new { GeneratedAt = DateTime.UtcNow, Count = resultData.Count, LastId = resultData.Last().Id, Data = resultData };
        }

        public async Task<MemoryStream> ExportMoviesAsync(int? lastId = null, int? pageSize = null, bool isCsv = false)
        {
            var query = PrepareExportQuery(lastId, pageSize);
            var stream = new MemoryStream();
            await stream.SaveAsAsync(query, excelType: isCsv ? ExcelType.CSV : ExcelType.XLSX);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private IQueryable<object> PrepareExportQuery(int? lastId, int? pageSize)
        {
            var query = _context.Movies.AsNoTracking().OrderBy(m => m.Id).AsQueryable();
            if (lastId.HasValue) query = query.Where(m => m.Id > lastId.Value);
            if (pageSize.HasValue) query = query.Take(pageSize.Value);

            return query.Select(m => new
            {
                FilmId = m.Id,
                FilmAdi = m.Title,
                Tur = m.Genre,
                KiralamaSayisi = _context.Rentals.Count(r => r.MovieId == m.Id)
            });
        }
    }
}