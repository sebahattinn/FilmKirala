using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FilmKirala.Application.Services;
public class AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ICacheService cacheService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await unitOfWork.Users.GetByEmailAsync(request.Email) != null)
            throw new InvalidOperationException("This email is already registered.");

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
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email);       // find user with gmail and pull all data from Users table 
        if (user == null) throw new UnauthorizedAccessException("The email or password is incorrect.");

        if (!VerifyPasswordHash(request.Password, Convert.FromBase64String(user.PasswordHash), Convert.FromBase64String(user.PasswordSalt)))
            throw new UnauthorizedAccessException("The email or password is incorrect.");

     //Because of this section—that is, because we've thrown an exception—a JWT cannot be generated, and this prevents the login from proceeding. 
        if (user.IsLoginBlocked)
            throw new PasswordChangeRequiredException(); 

        var refreshToken = GenerateRefreshToken();
        user.AddRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await unitOfWork.CompleteAsync();

        return new AuthResponseDto(user.Id, user.Username, user.Email, CreateToken(user),
            refreshToken, user.Roles.ToString(), user.WalletBalance);
    }

    public async Task<AuthResponseDto> GetCurrentUserAsync(int userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

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
        if (user == null) throw new KeyNotFoundException($"User not found: {email}");

        user.UpdateBalance(newBalance);
        await unitOfWork.CompleteAsync();

        await cacheService.RemoveAsync($"user_profile_{user.Id}");
    }

    public async Task<TopUpResponseDto> TopUpBalanceAsync(int userId, int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("The amount to be loaded must be greater than 0.");

        var user = await unitOfWork.Users.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("User not found.");

        user.AddBalance(amount);
        await unitOfWork.CompleteAsync();

        await cacheService.RemoveAsync($"user_profile_{userId}");

        return new TopUpResponseDto(user.WalletBalance, $"{amount} Added to your TL balance.");
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        
        var user = await unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null)
            throw new UnauthorizedAccessException("Refresh token is invalid.");

        var tokenRecord = user.RefreshTokens.FirstOrDefault(x => x.Token == request.RefreshToken);
        if (tokenRecord == null || !tokenRecord.IsActive)
            throw new UnauthorizedAccessException("The refresh token has expired or is invalid.");

        
        var newToken = CreateToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Cancel the current token and add a new one
        tokenRecord.Revoke();
        user.AddRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        await unitOfWork.CompleteAsync();

        return new AuthResponseDto(user.Id, user.Username, user.Email, newToken,
            newRefreshToken, user.Roles.ToString(), user.WalletBalance);
    }
    public async Task ChangePasswordAsync(ChangePasswordRequestDto request)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email)
                   ?? throw new KeyNotFoundException("User not found.");

        if (!VerifyPasswordHash(request.CurrentPassword, Convert.FromBase64String(user.PasswordHash), Convert.FromBase64String(user.PasswordSalt)))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        CreatePasswordHash(request.NewPassword, out byte[] newHash, out byte[] newSalt);

        user.ChangePassword(Convert.ToBase64String(newHash), Convert.ToBase64String(newSalt));

        await unitOfWork.CompleteAsync();
        await cacheService.RemoveAsync($"user_profile_{user.Id}");
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