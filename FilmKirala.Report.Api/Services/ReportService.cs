using Dapper;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;
using StackExchange.Profiling;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using MSConfig = Microsoft.Extensions.Configuration.IConfiguration;

namespace FilmKirala.Report.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _exportPath;
        private readonly MSConfig _configuration;

        public ReportService(AppDbContext context, IServiceScopeFactory scopeFactory, MSConfig configuration)
        {
            _context = context;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            var parentDir = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();
            _exportPath = Path.Combine(parentDir, "FilmKiralaExports");
        }

        // DB'ye Pending kaydı atar, JobId döner 
        public async Task<string> EnqueueReportAsync(bool isCsv)
        {
            var jobId = Guid.NewGuid().ToString("N")[..8];
            var job = ReportJob.Create(jobId, isCsv);
            _context.ReportJobs.Add(job);
            await _context.SaveChangesAsync();
            return jobId;
        }

        // BackgroundWorker tarafından çağrılır
        public async Task CreateLargeReportInBackgroundAsync(string jobId, bool isCsv)
        {
            using (MiniProfiler.Current?.Step($"Arka Plan Raporu: {jobId}"))
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
                    using (MiniProfiler.Current?.Step("DB: Kiralama Sayılarını Gruplayarak Çekme"))
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
                        using (MiniProfiler.Current?.Step($"Batch Çekiliyor (LastId: {lastId})"))
                        {
                            movies = (await SqlMapper.QueryAsync(conn,
                                "SELECT Id, Title, Genre FROM Movies WHERE Id > @lastId ORDER BY Id OFFSET 0 ROWS FETCH NEXT @batchSize ROWS ONLY",
                                new { lastId, batchSize },
                                commandTimeout: 120)).ToList();
                        }

                        if (!movies.Any()) break;

                        List<object> currentBatchRows;
                        using (MiniProfiler.Current?.Step("PLINQ: Mapping İşlemi"))
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

                        using (MiniProfiler.Current?.Step("IO: MiniExcel ile Diske Yazma"))
                        {
                            var excelType = isCsv ? ExcelType.CSV : ExcelType.XLSX;

                            if (isFirstBatch)
                            {
                                // İlk partide dosyayı sıfırdan oluşturur. MiniExcel Dosyasını 
                                await MiniExcel.SaveAsAsync(fullPath, currentBatchRows, excelType: excelType);
                                isFirstBatch = false;
                            }
                            else
                            {
                                // Sonraki partilerde mevcut sayfanın (Sheet1) altına ekleme yapar
                                await MiniExcel.InsertAsync(fullPath, currentBatchRows, sheetName: "Sheet1", excelType: excelType);
                            }
                        } 

                        processedCount += movies.Count;
                        lastId = (int)movies.Last().Id;
                    }

                    using (MiniProfiler.Current?.Step("DB: Job Durumu Güncelleniyor"))
                    {
                        var reportJob = await db.ReportJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
                        if (reportJob != null)
                        {
                            reportJob.MarkAsCompleted(fullPath);
                            await db.SaveChangesAsync();
                        }
                    }

                    using (MiniProfiler.Current?.Step("DB: Bildirim Kaydı Atılıyor"))
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
            var job = await _context.ReportJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
            if (job == null || job.Status != ReportJobStatus.Completed || job.FilePath == null)
                return (false, null, null);

            if (!File.Exists(job.FilePath)) return (false, null, null);

            try
            {
                using var stream = new FileStream(job.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return (true, ms.ToArray(), Path.GetFileName(job.FilePath));
            }
            catch (IOException)
            {
                return (false, null, null);
            }
        }

        // DB'deki job durumunu string olarak dönüyoruz.
        public async Task<string> GetJobStatusAsync(string jobId)
        {
            var job = await _context.ReportJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
            if (job == null) return "NotFound";

            return job.Status switch
            {
                ReportJobStatus.Pending => "Pending",
                ReportJobStatus.Processing => "Processing",
                ReportJobStatus.Completed => "Completed",
                ReportJobStatus.Failed => "Failed",
                _ => "Unknown"
            };
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
