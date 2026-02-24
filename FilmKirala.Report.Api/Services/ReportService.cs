using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace FilmKirala.Report.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetMovieSummaryAsync(int lastId, int pageSize)
        {
            var report = await _context.Movies
                .AsNoTracking()
                .OrderBy(m => m.Id)
                .Where(m => m.Id > lastId)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Genre,
                    m.Stock,
                    RentalCount = _context.Rentals.Count(r => r.MovieId == m.Id)
                })
                .ToListAsync();

            return new
            {
                GeneratedAt = DateTime.Now,
                Count = report.Count,
                LastId = report.Any() ? report.Last().Id : 0,
                Data = report
            };
        }

        public async Task<MemoryStream> ExportMoviesToExcelAsync(int? lastId = null, int? pageSize = null)
        {
         
            var query = _context.Movies.AsNoTracking().OrderBy(m => m.Id).AsQueryable();

            if (lastId.HasValue)
            {
                query = query.Where(m => m.Id > lastId.Value);
            }

            if (pageSize.HasValue)
            {
                query = query.Take(pageSize.Value);
            }

            var finalQuery = query.Select(m => new
            {
                FilmId = m.Id,
                FilmAdi = m.Title,
                Tur = m.Genre,
                Stok = m.Stock,
                KiralamaSayisi = _context.Rentals.Count(r => r.MovieId == m.Id)
            });

            var stream = new MemoryStream();
            await stream.SaveAsAsync(finalQuery);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}