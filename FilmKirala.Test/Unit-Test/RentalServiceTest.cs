using Moq;
using Xunit;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
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
        public async Task CreateRentalAsync_ShouldThrowInvalidOperationException_WhenUserBalanceIsInsufficient()
        {
            // Arrange
            var userId = 1;
            var user = new User("Seba", "seba@test.com", "h", "s", 10, Roles.User);
            var movie = new Movie("Batman", "Desc", "Action", 5, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            var pricing = movie.RentalPricings.First(); // pricing.Movie = movie (constructor'da set edildi)

            var pricingRepoMock = new Mock<IRentalPricingRepository>();
            pricingRepoMock
                .Setup(x => x.GetPricingByMovieAndTypeAsync(movie.Id, DurationType.Günlük))
                .ReturnsAsync(pricing);
            _uowMock.Setup(x => x.RentalPricings).Returns(pricingRepoMock.Object);
            _uowMock.Setup(x => x.Users.GetByIdAsync(userId)).ReturnsAsync(user);

            var request = new RentRequestDto(movie.Id, DurationType.Günlük);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _rentalService.CreateRentalAsync(request, userId));

            Assert.Contains("balance", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
