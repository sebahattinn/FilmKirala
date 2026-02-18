using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FilmKirala.Application.Services
{
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

            user.AddRefreshToken(GenerateRefreshToken(), DateTime.UtcNow.AddDays(7));

            await unitOfWork.Users.AddAsync(user);
            await unitOfWork.CompleteAsync();

            return new AuthResponseDto(user.Id, user.Username, user.Email, CreateToken(user),
                user.RefreshTokens.Last().Token, user.Roles.ToString(), user.WalletBalance);
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

          //  string cacheKey = $"user_profile_{user.Id}";                          
          //  await cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));      direkt user atmak yerine DTO gönderimi sağlıyoruz.
            string cacheKey = $"user_profile_{user.Id}";
            var cacheData = new UserCacheDtos(user.Id, user.Username, user.Email, user.WalletBalance, user.Roles.ToString());
            await cacheService.SetAsync(cacheKey, cacheData, TimeSpan.FromHours(1));

            return new AuthResponseDto(user.Id, user.Username, user.Email, CreateToken(user),
                refreshToken, user.Roles.ToString(), user.WalletBalance);
        }

        public async Task UpdateUserBalanceAsync(string email, int newBalance)
        {
            var user = await unitOfWork.Users.GetByEmailAsync(email);
            if (user == null) throw new KeyNotFoundException($"Kullanıcı bulunamadı: {email}");

            user.UpdateBalance(newBalance);
            await unitOfWork.CompleteAsync();
            await cacheService.RemoveAsync($"user_profile_{user.Id}");
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

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}