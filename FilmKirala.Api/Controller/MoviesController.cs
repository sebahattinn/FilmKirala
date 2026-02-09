using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAll()
        {
            var result = await _movieService.GetAllMoviesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _movieService.GetMovieByIdAsync(id);
            return Ok(result);
        }

        // Rol bazlı film ekleme yapıyoruz ab
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateMovieDto request)
        {
            await _movieService.AddMovieAsync(request);
            return Ok(new { message = "Film başarıyla eklendi." });
        }
    }
}