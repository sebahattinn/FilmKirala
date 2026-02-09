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

            var refreshToken = GenerateRefreshToken();
            user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(2)); // 2 gün geçerli

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            string accessToken = CreateToken(user);

            return new AuthResponseDto(user.Id, user.Username, user.Email, accessToken, user.RefreshToken, user.Roles.ToString(), user.WalletBalance);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (!VerifyPasswordHash(request.Password, Convert.FromBase64String(user.PasswordHash), Convert.FromBase64String(user.PasswordSalt)))
                throw new Exception("Şifre yanlış.");

            var refreshToken = GenerateRefreshToken();
            user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
            await _unitOfWork.CompleteAsync();

            string accessToken = CreateToken(user);

            return new AuthResponseDto(user.Id, user.Username, user.Email, accessToken, user.RefreshToken, user.Roles.ToString(), user.WalletBalance);
        }
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {

            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) throw new Exception("Geçersiz Token (Principal oluşturulamadı)"); //Süresi bitmiş Access Token'dan User Id'yi çıkart

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) //parse ile token kontroplü
            {
                throw new Exception("Token içinde geçerli bir Kullanıcı ID'si bulunamadı!");
            }


            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Oturum süreniz dolmuş veya kullanıcı bulunamadı. Lütfen tekrar giriş yapın.");
            }

            if (user.RefreshToken != request.RefreshToken)
            {
                user.UpdateRefreshToken(null, DateTime.UtcNow); // Token'ı öldür yani Revoke
                await _unitOfWork.CompleteAsync();
                throw new Exception("Güvenlik Uyarısı: Eski bir token kullanıldı! Hesabınız güvenliği için oturum kapatıldı.");
            }

            var newAccessToken = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
            await _unitOfWork.CompleteAsync();

            return new AuthResponseDto(user.Id, user.Username, user.Email, newAccessToken, newRefreshToken, user.Roles.ToString(), user.WalletBalance);
        }

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
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Roles.ToString())
            };

            var keyString = _configuration.GetSection("JwtSettings:Key").Value;

            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JwtSettings:Key değeri appsettings.json dosyasından okunamadı");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // Access Token ömrü 15dk
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var keyString = _configuration.GetSection("JwtSettings:Key").Value;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}