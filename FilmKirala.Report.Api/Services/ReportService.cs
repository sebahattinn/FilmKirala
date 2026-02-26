using Dapper;
using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using Hangfire;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text.Json;

namespace FilmKirala.Report.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _exportPath;

        public ReportService(AppDbContext context, IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _scopeFactory = scopeFactory;
            var parentDir = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();
            _exportPath = Path.Combine(parentDir, "FilmKiralaExports");
        }

        public string EnqueueReport(bool isCsv)
        {
            var jobId = Guid.NewGuid().ToString().Substring(0, 8);
            BackgroundJob.Enqueue<IReportService>(service => service.CreateLargeReportInBackgroundAsync(jobId, isCsv));
            return jobId;
        }

        public async Task CreateLargeReportInBackgroundAsync(string jobId, bool isCsv)
        {
            if (!Directory.Exists(_exportPath)) Directory.CreateDirectory(_exportPath);
            var fullPath = Path.Combine(_exportPath, $"Report_{jobId}.{(isCsv ? "csv" : "xlsx")}");
            if (File.Exists(fullPath)) File.Delete(fullPath);

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Bağlantıyı IDbConnection olarak al ve aç
                using var conn = db.Database.GetDbConnection();
                if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

               
                // Eğer SqlMapper'a ulaşamazsan 'Dapper.SqlMapper.QueryAsync' dene.
                var rentals = await SqlMapper.QueryAsync<(int MovieId, int Count)>(conn,
                    "SELECT MovieId, COUNT(*) as Count FROM Rentals GROUP BY MovieId");

                var rentalDict = rentals.ToDictionary(x => x.MovieId, x => x.Count);

                int batchSize = 100000;
                int lastId = 0;
                var finalData = new List<object>();

                while (true)
                {
                    // Filmleri de aynı şekilde statik metodla çekiyoruz.
                    var movies = (await SqlMapper.QueryAsync(conn,
                        "SELECT Id, Title, Genre FROM Movies WHERE Id > @lastId ORDER BY Id OFFSET 0 ROWS FETCH NEXT @batchSize ROWS ONLY",
                        new { lastId, batchSize })).ToList();

                    if (!movies.Any()) break;

                    var currentBatchRows = new ConcurrentBag<object>();
                    Parallel.ForEach(movies, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, m =>
                    {
                        currentBatchRows.Add(new
                        {
                            FilmId = (int)m.Id,
                            FilmAdi = (string)m.Title,
                            Tur = (string)m.Genre,
                            KiralamaSayisi = rentalDict.GetValueOrDefault((int)m.Id, 0)
                        });
                    });

                    finalData.AddRange(currentBatchRows);
                    lastId = (int)movies.Last().Id;
                    Console.WriteLine($"[CPU BATCH DONE] Last ID: {lastId}");
                }

                await MiniExcel.SaveAsAsync(fullPath, finalData, excelType: isCsv ? ExcelType.CSV : ExcelType.XLSX);
                finalData.Clear();
            }

            try
            {
                var notification = new NotificationLog(
                    userEmail: "admin@filmkirala.com",
                    subject: "Rapor Hazır",
                    message: $"İşlemci odaklı rapor bitti. JobId: {jobId}",
                    isSent: false,
                    createdAt: DateTime.Now,
                    sentAt: DateTime.Now,
                    errorMessage: string.Empty,
                    type: isCsv ? "CSV" : "XLSX"
                );
                _context.NotificationLogs.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { Console.WriteLine($"Bildirim hatası: {ex.Message}"); }
            Console.WriteLine($"✅ İşlemci Gücüyle Bitti: {fullPath}");
        }

        public async Task<(bool IsReady, byte[]? FileBytes, string? FileName)> GetReportFileAsync(string jobId)
        {
            if (!Directory.Exists(_exportPath)) return (false, null, null);
            var files = Directory.GetFiles(_exportPath, $"Report_{jobId}.*");
            if (files.Length == 0) return (false, null, null);
            var filePath = files[0];
            var fileName = Path.GetFileName(filePath);
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            return (true, fileBytes, fileName);
        }

        public async Task<object> GetMovieSummaryAsync(int lastId, int pageSize)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var conn = db.Database.GetDbConnection();
                if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

              
                var movies = (await SqlMapper.QueryAsync(conn,
                    "SELECT Id, Title, Genre, Stock FROM Movies WHERE Id > @lastId ORDER BY Id OFFSET 0 ROWS FETCH NEXT @pageSize ROWS ONLY",
                    new { lastId, pageSize })).ToList();

                if (!movies.Any()) return new { Count = 0, Data = new List<object>() };

                var movieIds = movies.Select(m => (int)m.Id).ToList();
                string jsonIds = JsonSerializer.Serialize(movieIds);

              
               
                var rentalCounts = await SqlMapper.QueryAsync<(int MovieId, int Count)>(conn,
                    @"SELECT r.MovieId, COUNT(*) as Count 
              FROM Rentals r 
              WHERE r.MovieId IN (SELECT value FROM OPENJSON(@jsonIds) WITH (value int '$'))
              GROUP BY r.MovieId",
                    new { jsonIds });

                var rentalDict = rentalCounts.ToDictionary(x => x.MovieId, x => x.Count);

                
                var resultData = movies.Select(m => new
                {
                    Id = (int)m.Id,
                    Title = (string)m.Title,
                    Genre = (string)m.Genre,
                    Stock = (int)m.Stock,
                    RentalCount = rentalDict.GetValueOrDefault((int)m.Id, 0)
                }).ToList();

                return new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Count = resultData.Count,
                    LastId = resultData.Last().Id,
                    Data = resultData
                };
            }
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
            return query.Select(m => new { FilmId = m.Id, FilmAdi = m.Title, Tur = m.Genre, KiralamaSayisi = _context.Rentals.Count(r => r.MovieId == m.Id) });
        }
    }
}