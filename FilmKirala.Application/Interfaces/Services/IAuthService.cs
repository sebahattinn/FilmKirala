using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IAuthService
    {
        
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request); // Kullanıcıyı kaydedip şifreyi hasleteceğim metot

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request); //JWT üretimi de burada yer alsın istiom

        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        Task UpdateUserBalanceAsync(string email, int newBalance);

        Task<TopUpResponseDto> TopUpBalanceAsync(int userId, int amount);

        Task<AuthResponseDto> GetCurrentUserAsync(int userId);

        Task ChangePasswordAsync(ChangePasswordRequestDto request);
    }
}