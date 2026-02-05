namespace FilmKirala.Application.DTOs
{
    // Kayıt Olurken
    public record RegisterRequestDto(string Username, string Email, string Password);

    // Giriş Yaparken
    public record LoginRequestDto(string Email, string Password);

    // Giriş Başarılı Olunca Döneceğimiz Cevap
    public record AuthResponseDto(
        int Id,
        string Username,
        string Email,
        string Token,
        string Role,
        int WalletBalance
    );
}