using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace FilmKirala.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [EnableRateLimiting("auth")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            //  201 Created dönerken Location Header verildi
            return CreatedAtAction(nameof(Login), new { email = request.Email }, result);
        }

        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
            => Ok(await _authService.LoginAsync(request));

        [EnableRateLimiting("auth")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
            => Ok(await _authService.RefreshTokenAsync(request));

        [Authorize(Roles = "Admin")]
        [HttpPost("update-balance")]
        public async Task<IActionResult> AdminUpdateBalance([FromBody] UpdateBalanceDto request)
        {
            await _authService.UpdateUserBalanceAsync(request.Email, request.NewBalance);

            return Ok(new { Message = "User balance updated successfully." });
        }

        [Authorize]
        [HttpPost("top-up")]
        public async Task<IActionResult> TopUpBalance([FromBody] TopUpRequestDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Token geçersiz.");

            var result = await _authService.TopUpBalanceAsync(userId, request.Amount);
            return Ok(result);
        }

        [EnableRateLimiting("auth")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            await _authService.ChangePasswordAsync(request);
            return Ok(new { Message = "Password changed successfully." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            // Token içindeki NameIdentifier (Id) claim'ini çekiyoruz
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Token geçersiz.");

            var result = await _authService.GetCurrentUserAsync(int.Parse(userIdClaim));
            return Ok(result);
        }
    }
}