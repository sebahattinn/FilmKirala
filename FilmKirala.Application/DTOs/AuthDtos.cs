namespace FilmKirala.Application.DTOs
{
    public record RegisterRequestDto(string Username, string Email, string Password);
    public record LoginRequestDto(string Email, string Password);
    public record AuthResponseDto(
        int Id,
        string Username,
        string Email,
        string Token,       
        string RefreshToken, 
        string Role,
        int WalletBalance
    );
    public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);
}