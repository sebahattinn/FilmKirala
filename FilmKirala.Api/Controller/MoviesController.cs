using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace FilmKirala.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        [OutputCache(Duration = 30)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? genre,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _movieService.GetAllMoviesAsync(search, genre, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _movieService.GetMovieByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Yeni bir film oluşturur. (Sadece Admin erişebilir)
        /// </summary>
        /// <remarks>
        /// Film oluşturulurken isteğe bağlı olarak kiralama seçenekleri (pricings) de eklenebilir.
        ///
        /// **DurationType değerleri:**
        /// | Değer | Anlam      |
        /// |-------|------------|
        /// | 1     | Saatlik    |
        /// | 2     | Günlük     |
        /// | 3     | Haftalık   |
        /// | 4     | Aylık      |
        /// | 5     | Yıllık     |
        ///
        /// **Örnek istek:**
        /// ```json
        /// {
        ///   "title": "Inception",
        ///   "description": "Rüya içinde rüya filmi",
        ///   "genre": "Bilim Kurgu",
        ///   "stock": 10,
        ///   "pricings": [
        ///     { "durationType": 2, "durationValue": 1, "price": 20 },
        ///     { "durationType": 3, "durationValue": 1, "price": 50 }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <response code="201">Film başarıyla oluşturuldu.</response>
        /// <response code="400">Validasyon hatası.</response>
        /// <response code="401">Giriş yapılmamış.</response>
        /// <response code="403">Admin yetkisi yok.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMovie([FromBody] CreateMovieDto request)
        {
            await _movieService.CreateMovieAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { message = "Movie created successfully." });
        }
    }
}