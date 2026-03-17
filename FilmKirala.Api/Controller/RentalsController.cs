using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmKirala.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        public RentalsController(IRentalService rentalService) => _rentalService = rentalService;

        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] RentRequestDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("You must be logged in to rent a movie.");

            var result = await _rentalService.CreateRentalAsync(request, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRentals()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("You must be logged in to view your rentals.");

            var rentals = await _rentalService.GetUserRentalsAsync(userId);
            return Ok(rentals);
        }
    }
}
