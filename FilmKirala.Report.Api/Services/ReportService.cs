using FilmKirala.Domain.Entity;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using Hangfire;

namespace FilmKirala.Report.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly string _exportPath;

        public ReportService(AppDbContext context)
        {
            _context = context;
           
            _exportPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "FilmKiralaExports");
        }

        public string EnqueueReport(bool isCsv)
        {
            var jobId = Guid.NewGuid().ToString().Substring(0, 8);
            BackgroundJob.Enqueue<IReportService>(service => service.CreateLargeReportInBackgroundAsync(jobId, isCsv));
            return jobId;
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

        public async Task CreateLargeReportInBackgroundAsync(string jobId, bool isCsv)
        {
            var query = PrepareExportQuery(null, null);

            if (!Directory.Exists(_exportPath)) Directory.CreateDirectory(_exportPath);

            var fileName = $"Report_{jobId}.{(isCsv ? "csv" : "xlsx")}";
            var fullPath = Path.Combine(_exportPath, fileName);

            await MiniExcel.SaveAsAsync(fullPath, query, excelType: isCsv ? ExcelType.CSV : ExcelType.XLSX);

            var notification = new NotificationLog(
                userEmail: "admin@filmkirala.com",
                subject: "Rapor Hazır",
                message: $"Raporunuz hazır! JobId: {jobId}",
                isSent: false,
                createdAt: DateTime.Now,
                sentAt: DateTime.Now,
                errorMessage: string.Empty,
                type: "ReportReady"
            );

            _context.NotificationLogs.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<object> GetMovieSummaryAsync(int lastId, int pageSize)
        {
            var report = await _context.Movies.AsNoTracking().OrderBy(m => m.Id).Where(m => m.Id > lastId).Take(pageSize)
                .Select(m => new { m.Id, m.Title, m.Genre, m.Stock, RentalCount = _context.Rentals.Count(r => r.MovieId == m.Id) }).ToListAsync();
            return new { GeneratedAt = DateTime.Now, Count = report.Count, LastId = report.Any() ? report.Last().Id : 0, Data = report };
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