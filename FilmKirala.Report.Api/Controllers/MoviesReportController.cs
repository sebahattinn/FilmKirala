using FilmKirala.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Report.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MoviesReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMovieSummary([FromQuery] int lastId = 0, [FromQuery] int pageSize = 10)
        {

            var report = await _context.Movies
                .AsNoTracking()
                .OrderBy(m => m.Id) // Sıralama şart
                .Where(m => m.Id > lastId) // Kaldığın yerden devam et
                .Take(pageSize) // Sadece sayfa boyutu kadar getir
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Genre,
                    m.Stock,
                    // Index (IX_Rentals_MovieId) sayesinde bu sayma işlemi artık çok daha hızlı!
                    RentalCount = _context.Rentals.Count(r => r.MovieId == m.Id)
                })
                .ToListAsync();

         
            if (!report.Any())
            {
                return NotFound("Gösterilebilecek başka veri kalmadı.");
            }

            return Ok(new
            {
                GeneratedAt = DateTime.Now,
                Count = report.Count,
                LastId = report.Last().Id, 
                Data = report
            });
        }
    }
}