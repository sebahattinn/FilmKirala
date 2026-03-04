using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FilmKirala.Application.Services;

public class AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ICacheService cacheService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await unitOfWork.Users.GetByEmailAsync(request.Email) != null)
            throw new InvalidOperationException("Bu email zaten kayıtlı.");

        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User(
            request.Username,
            request.Email,
            Convert.ToBase64String(passwordHash),
            Convert.ToBase64String(passwordSalt),
            0,
            Roles.User
        );

        var refreshToken = GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.CompleteAsync();

        return new AuthResponseDto(user.Id, user.Username, user.Email, CreateToken(user),
            refreshToken, user.Roles.ToString(), user.WalletBalance);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null) throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");

        if (!VerifyPasswordHash(request.Password, Convert.FromBase64String(user.PasswordHash), Convert.FromBase64String(user.PasswordSalt)))
            throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");

        var refreshToken = GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await unitOfWork.CompleteAsync();

        return new AuthResponseDto(user.Id, user.Username, user.Email, CreateToken(user),
            refreshToken, user.Roles.ToString(), user.WalletBalance);
    }

    public async Task<AuthResponseDto> GetCurrentUserAsync(int userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            string.Empty,
            string.Empty,
            user.Roles.ToString(),
            user.WalletBalance
        );
    }

    public async Task UpdateUserBalanceAsync(string email, int newBalance)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(email);
        if (user == null) throw new KeyNotFoundException($"Kullanıcı bulunamadı: {email}");

        user.UpdateBalance(newBalance);
        await unitOfWork.CompleteAsync();

        await cacheService.RemoveAsync($"user_profile_{user.Id}");
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        // 1. Kullanıcıyı bul (Refresh token ile eşleşen kullanıcıyı DB'den çekiyoruz)
        var user = await unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null)
            throw new UnauthorizedAccessException("Refresh token geçersiz.");

        var tokenRecord = user.RefreshTokens.FirstOrDefault(x => x.Token == request.RefreshToken);
        if (tokenRecord == null || !tokenRecord.IsActive)
            throw new UnauthorizedAccessException("Refresh token süresi dolmuş veya geçersiz.");

        // 2. Yeni tokenları üret
        var newToken = CreateToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // 3. Mevcut token'ı iptal et ve yenisini ekle (Domain logic)
        tokenRecord.Revoke();
        user.AddRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        await unitOfWork.CompleteAsync();

        return new AuthResponseDto(user.Id, user.Username, user.Email, newToken,
            newRefreshToken, user.Roles.ToString(), user.WalletBalance);
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Roles.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            configuration["JwtSettings:Issuer"],
            configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(password)).SequenceEqual(storedHash);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}