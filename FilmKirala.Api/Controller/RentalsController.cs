using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmKirala.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Test sürecinde açık kalabilir ama UserId kontrolü artık sıkı.
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        public RentalsController(IRentalService rentalService) => _rentalService = rentalService;

        [HttpPost("rent")]
        public async Task<IActionResult> RentMovie([FromBody] RentRequestDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                // Artık fallback (ID = 1) yok! Kimlik yoksa işlem de yok.
                return Unauthorized("Kiralama yapmak için giriş yapmalısınız.");
            }

            var result = await _rentalService.RentMovieAsync(request, userId);
            return Ok(result);
        }

        [HttpGet("my-rentals")]
        public async Task<IActionResult> GetMyRentals()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Kiralamalarınızı görmek için giriş yapmalısınız.");
            }

            var rentals = await _rentalService.GetUserRentalsAsync(userId);
            return Ok(rentals);
        }
    }
}