using Moq;
using Xunit;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services; 
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using Microsoft.Extensions.Configuration;
using FilmKirala.Infrastructure.Repositories;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Test.UnitTests
{
    public class DeepLogicCoverageTests
    {
        private static AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GenericRepository_AllMethods_ShouldBeCovered()
        {
            using var context = GetDbContext();
            var repo = new GenericRepository<User>(context);
            var user = new User("RepoTest", "repo@test.com", "h", "s", 100, Roles.User);

            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            _ = await repo.GetAllAsync();
            _ = await repo.GetByIdAsync(user.Id);

            repo.Update(user);
            repo.Remove(user);
            await context.SaveChangesAsync();

            Assert.Empty(await repo.GetAllAsync());
        }

        [Fact]
        public async Task AuthService_EdgeCases_ShouldBeCovered()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var configMock = new Mock<IConfiguration>();
            var cacheMock = new Mock<ICacheService>();
            var authService = new AuthService(uowMock.Object, configMock.Object, cacheMock.Object);

            uowMock.Setup(x => x.Users.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                authService.UpdateUserBalanceAsync("olmayan@mail.com", 100));

            var loginRequest = new LoginRequestDto("test@test.com", "wrongpass");
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(loginRequest));
        }

        [Fact]
        public async Task RentalService_ExtraEdgeCases_ShouldBeCovered()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>(); 

            //  FIX: cacheMock.Object dördüncü parametre olarak eklendi
            var rentalService = new RentalService(uowMock.Object, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "seba@test.com", "h", "s", 100, Roles.User);
            uowMock.Setup(x => x.Users.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);

            // Filmi bilerek null bırakıyoruz ki KeyNotFoundException fırlasın
            uowMock.Setup(x => x.Movies.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Movie)null!);

            var request = new RentRequestDto(999, DurationType.Günlük, 1);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => rentalService.RentMovieAsync(request, user.Id));
        }
    }
}