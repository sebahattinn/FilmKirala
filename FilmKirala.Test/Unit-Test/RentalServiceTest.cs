using Moq;
using Xunit;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using System.Linq;

namespace FilmKirala.Test.UnitTests
{
    public class RentalServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IBusService> _busMock;
        private readonly Mock<ICacheService> _cacheMock;

        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _busMock = new Mock<IBusService>();
            _cacheMock = new Mock<ICacheService>();

            _rentalService = new RentalService(
                _uowMock.Object,
                null!,
                _busMock.Object,
                _cacheMock.Object);
        }

        [Fact]
        public async Task RentMovieAsync_ShouldThrowInvalidOperationException_WhenUserBalanceIsInsufficient()
        {
            // Arrange
            var userId = 1;
            var user = new User("Seba", "seba@test.com", "h", "s", 10, Roles.User);
            var movie = new Movie("Batman", "Desc", "Action", 5, true);

            // Fiyatlandırmayı ekliyoruz
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            _uowMock.Setup(x => x.Users.GetByIdAsync(userId)).ReturnsAsync(user);
            _uowMock.Setup(x => x.Movies.GetMovieWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(movie);

            // FIX: Pricing listesindeki ilk elemanın ID'sini alıyoruz. 
            // Unit testte ID normalde 0 gelir ama DTO'nun int beklediği hatasını çözer.
            var pricing = movie.RentalPricings.First();
            var request = new RentRequestDto(movie.Id, pricing.Id);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _rentalService.RentMovieAsync(request, userId));

            Assert.Contains("yetersiz", exception.Message.ToLower());
        }
    }
}