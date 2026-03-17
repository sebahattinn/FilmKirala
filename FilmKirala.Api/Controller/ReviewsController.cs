using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmKirala.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _reviewService.AddReviewAsync(request, userId);
            return Ok(new { message = "Your review has been submitted successfully." });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var reviews = await _reviewService.GetMyReviewsAsync(userId);
            return Ok(reviews);
        }

        [HttpGet("by-rating")]
        public async Task<IActionResult> GetReviewsByRating([FromQuery] int rating = 3, [FromQuery] int? movieId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _reviewService.GetReviewsByRatingAsync(rating, movieId, page, pageSize);

            if (result.TotalCount == 0)
            {
                var message = movieId.HasValue
                    ? $"No {rating}-star reviews found for movie ID {movieId}."
                    : $"No {rating}-star reviews found.";
                return NotFound(new { message });
            }

            return Ok(result);
        }
    }
}
