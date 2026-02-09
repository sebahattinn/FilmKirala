using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmKirala.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bearer şartı koyduk
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost("rent")]
        public async Task<IActionResult> RentMovie([FromBody] RentRequestDto request)
        {
            // Token'dan User ID'yi çekme (Claims)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            try
            {
                // Service artık bize detaylı fiş dönüyor
                var result = await _rentalService.RentMovieAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Yetersiz bakiye vb. hatalar burada yakalanıp dönülecek
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("my-rentals")]
        public async Task<IActionResult> GetMyRentals()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var rentals = await _rentalService.GetUserRentalsAsync(userId);
            return Ok(rentals);
        }
    }
}