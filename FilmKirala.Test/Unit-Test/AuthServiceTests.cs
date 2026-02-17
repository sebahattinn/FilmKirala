using Moq;
using Xunit;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace FilmKirala.Test.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _configMock = new Mock<IConfiguration>();
            _cacheMock = new Mock<ICacheService>();

            _authService = new AuthService(_uowMock.Object, _configMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
        {
            var request = new RegisterRequestDto("Seba", "existing@test.com", "123456");
            _uowMock.Setup(x => x.Users.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User("Old", "existing@test.com", "h", "s", 0, Roles.User));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task UpdateUserBalanceAsync_ShouldHandleNegativeBalance_ByDomainRules()
        {
            // Arrange
            var email = "seba@bursa.com";
            var user = new User("Seba", email, "h", "s", 100, Roles.User);
            _uowMock.Setup(x => x.Users.GetByEmailAsync(email)).ReturnsAsync(user);

            // 🚀 FIX: S112 uyumu için artık Exception değil ArgumentException bekliyoruz
            await Assert.ThrowsAsync<ArgumentException>(() => _authService.UpdateUserBalanceAsync(email, -50));
        }
    }
}