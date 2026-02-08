using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IAuthService
    {
        // Kullanıcıyı kaydeder, şifreyi hashler
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

        // Kullanıcıyı doğrular, JWT Token üretip döner
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

        // 👇 YENİ METOT 👇
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}