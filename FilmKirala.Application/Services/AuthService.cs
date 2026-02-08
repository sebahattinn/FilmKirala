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
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _unitOfWork.Users.GetByEmailAsync(request.Email) != null)
                throw new Exception("Bu email zaten kayıtlı.");

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User(
                request.Username,
                request.Email,
                Convert.ToBase64String(passwordHash),
                Convert.ToBase64String(passwordSalt),
                0,
                Roles.User
            );

            // 👇 Refresh Token Oluştur ve Kaydet 👇
            var refreshToken = GenerateRefreshToken();
            user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7)); // 7 gün geçerli

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            string accessToken = CreateToken(user);

            // Response'a RefreshToken'ı da ekledik
            return new AuthResponseDto(user.Id, user.Username, user.Email, accessToken, user.RefreshToken, user.Roles.ToString(), user.WalletBalance);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (!VerifyPasswordHash(request.Password, Convert.FromBase64String(user.PasswordHash), Convert.FromBase64String(user.PasswordSalt)))
                throw new Exception("Şifre yanlış.");

            // 👇 Login olunca Refresh Token yenile (Rotation) 👇
            var refreshToken = GenerateRefreshToken();
            user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _unitOfWork.CompleteAsync();

            string accessToken = CreateToken(user);

            return new AuthResponseDto(user.Id, user.Username, user.Email, accessToken, user.RefreshToken, user.Roles.ToString(), user.WalletBalance);
        }

        // 👇 YENİ METOT: TOKEN YENİLEME İŞLEMİ 👇
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // 1. Süresi bitmiş Access Token'dan User Id'yi çıkart
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) throw new Exception("Geçersiz Token");

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) throw new Exception("Token içinde ID bulunamadı");

            // 2. Kullanıcıyı DB'den bul
            var user = await _unitOfWork.Users.GetByIdAsync(int.Parse(userIdStr));

            // 3. Refresh Token Kontrolü (DB'deki ile uyuşuyor mu? Süresi dolmuş mu?)
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Geçersiz veya süresi dolmuş Refresh Token. Lütfen tekrar giriş yapın.");
            }

            // 4. Yeni Çift Oluştur (Hem Access Hem Refresh değişir)
            var newAccessToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _unitOfWork.CompleteAsync();

            return new AuthResponseDto(user.Id, user.Username, user.Email, newAccessToken, newRefreshToken, user.Roles.ToString(), user.WalletBalance);
        }

        // --- PRIVATE METHODS ---

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }

        // Access Token Üretici (Senin düzelttiğin hali)
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Roles.ToString())
            };

            var keyString = _configuration.GetSection("JwtSettings:Key").Value;

            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JwtSettings:Key değeri appsettings.json dosyasından okunamadı!");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // Access Token ömrü kısa olur (15 dk)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 👇 Rastgele Refresh Token Üreten Metot 👇
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // 👇 Süresi bitmiş token'ı okuyan kritik metot 👇
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var keyString = _configuration.GetSection("JwtSettings:Key").Value;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString!)),
                ValidateLifetime = false // ÖNEMLİ: Süre kontrolünü kapatıyoruz ki okuyabilelim
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}