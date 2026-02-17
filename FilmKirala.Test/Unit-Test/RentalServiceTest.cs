using Moq;
using Xunit;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Test.UnitTests
{
    public class RentalServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IBusService> _busMock;
       

        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _busMock = new Mock<IBusService>();

           
            _rentalService = new RentalService(
                _uowMock.Object,
                null!, // IMapper şimdilik test logic'inde null olabilir
                _busMock.Object);
        }

        [Fact]
        public async Task RentMovieAsync_ShouldThrowInvalidOperationException_WhenUserBalanceIsInsufficient()
        {
            // Arrange
            var userId = 1;
            // Bakiyesi 10 TL olan bir kullanıcı [cite: 2026-02-10]
            var user = new User("Seba", "seba@test.com", "h", "s", 10, Roles.User);
            var movie = new Movie("Batman", "Desc", "Action", 5, true);
            // 50 TL'lik bir fiyatlandırma ekliyoruz [cite: 2026-02-10]
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            _uowMock.Setup(x => x.Users.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(x => x.Movies.GetMovieWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(movie);

            var request = new RentRequestDto(movie.Id, DurationType.Günlük, 1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _rentalService.RentMovieAsync(request, userId));

            Assert.Contains("yetersiz", exception.Message.ToLower());
        }
    }
}