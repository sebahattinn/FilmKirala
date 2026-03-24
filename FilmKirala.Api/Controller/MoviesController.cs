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

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMovie([FromBody] CreateMovieDto request)
        {
            await _movieService.CreateMovieAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { message = "Movie created successfully." });
        }
    }
}