using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmKirala.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Test sürecinde hızlı aksiyon için açık bıraktım
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        public RentalsController(IRentalService rentalService) => _rentalService = rentalService;

        [HttpPost("rent")]
        public async Task<IActionResult> RentMovie([FromBody] RentRequestDto request)
        {
            // Parse hatası durumunda 401 döner, test için 1'e düşer.
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                userId = 1; // k6 stres testi için fallback
            }

            var result = await _rentalService.RentMovieAsync(request, userId);


            return Ok(result);
        }

        [HttpGet("my-rentals")]
        public async Task<IActionResult> GetMyRentals()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                userId = 1; // k6 stres testi için fallback
            }

            var rentals = await _rentalService.GetUserRentalsAsync(userId);
            return Ok(rentals);
        }
    }
}