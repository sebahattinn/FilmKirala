namespace FilmKirala.Application.DTOs
{
    // Kayıt Olurken
    public record RegisterRequestDto(string Username, string Email, string Password);

    // Giriş Yaparken
    public record LoginRequestDto(string Email, string Password);

    // 👇 AuthResponseDto GÜNCELLENDİ (RefreshToken eklendi) 👇
    public record AuthResponseDto(
        int Id,
        string Username,
        string Email,
        string Token,       // Access Token
        string RefreshToken, // Yeni Eklenen
        string Role,
        int WalletBalance
    );

    // 👇 YENİ DTO (Token süresi bitince bunu gönderecekler) 👇
    public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);
}