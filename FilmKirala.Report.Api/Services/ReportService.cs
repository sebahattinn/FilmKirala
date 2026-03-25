using Dapper;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using StackExchange.Profiling;
using System.Data;
using MSConfig = Microsoft.Extensions.Configuration.IConfiguration;

namespace FilmKirala.Report.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _exportPath;
        private readonly MSConfig _configuration;
        private readonly ILogger<ReportService> _logger;
        
        public ReportService(AppDbContext context, IServiceScopeFactory scopeFactory, MSConfig configuration, ILogger<ReportService> logger)
        {
            _context = context;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
            var parentDir = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();   //CSV writer. sorguyu csv'ye çıkaran 
            _exportPath = Path.Combine(parentDir, "FilmKiralaExports");
        }
        // Inserts a pending record into the database, returns the JobId 
        public async Task<string> QueueReportAsync(bool isCsv)
        {
            var jobId = Guid.NewGuid().ToString("N")[..8];
            var job = ReportJob.Create(jobId, isCsv);
            _context.ReportJobs.Add(job);
            await _context.SaveChangesAsync();
            return jobId;
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

        // We are converting the job status in the database to a string.
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
            // Single connection: no extra EF Core scope needed since only Dapper queries run here.
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

            var movies = (await SqlMapper.QueryAsync(conn,
                "SELECT Id, Title, Genre, Stock FROM Movies WHERE Id > @lastId ORDER BY Id OFFSET 0 ROWS FETCH NEXT @pageSize ROWS ONLY",
                new { lastId, pageSize })).ToList();

            if (!movies.Any()) return new { Count = 0, Data = new List<object>() };

            var movieIds = movies.Select(m => (int)m.Id).ToList();

            var rentalCounts = await SqlMapper.QueryAsync<(int MovieId, int Count)>(conn,
                "SELECT MovieId, COUNT(*) as Count FROM Rentals WHERE MovieId IN @movieIds GROUP BY MovieId",
                new { movieIds });

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

            // LEFT JOIN with GROUP BY — single SQL query, no correlated subquery per row.
            return query
                .GroupJoin(
                    _context.Rentals.AsNoTracking(),
                    m => m.Id,
                    r => r.MovieId,
                    (m, rentals) => new { m, RentalCount = rentals.Count() })
                .Select(x => (object)new
                {    
                    FilmId = x.m.Id,
                    FilmAdi = x.m.Title,
                    Tur = x.m.Genre,
                    KiralamaSayisi = x.RentalCount
                });
        }
    }
}
