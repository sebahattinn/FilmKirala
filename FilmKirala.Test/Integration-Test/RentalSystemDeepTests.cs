using Microsoft.EntityFrameworkCore;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Application.Services;
using FilmKirala.Infrastructure.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace FilmKirala.Test.IntegrationTests
{
    public class AdvancedRentalIntegrationTests
    {
        private static AppDbContext GetUniqueDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private IUnitOfWork CreateUnitOfWork(AppDbContext context)
        {
            var movieRepo = new MovieRepository(context, NullLogger<MovieRepository>.Instance);
            var userRepo = new UserRepository(context, NullLogger<UserRepository>.Instance);
            var pricingRepo = new RentalPricingRepository(context, NullLogger<RentalPricingRepository>.Instance);
            var loggerFactory = new NullLoggerFactory();

            return new UnitOfWork(context, movieRepo, userRepo, pricingRepo, loggerFactory);
        }

        [Fact]
        public async Task Rental_InsufficientBalance_EdgeCase()
        {
            using var context = GetUniqueDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var rentalService = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("PoorUser", "poor@test.com", "h", "s", 100, Roles.User);
            var movie = new Movie("Expensive Film", "Desc", "Genre", 10, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 500);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük);

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(rentRequest, user.Id));
        }

        [Fact]
        public async Task Rental_StockOut_EdgeCase()
        {
            using var context = GetUniqueDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var rentalService = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("StockTest", "stock@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("NoStock", "Desc", "Genre", 0, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük);

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(rentRequest, user.Id));
        }

        [Fact]
        public async Task Rental_Duration_Calculation_Verification()
        {
            using var context = GetUniqueDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var rentalService = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Tester", "test@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Matrix", "Desc", "Sci-Fi", 10, true);
            movie.AddRentalPricing(DurationType.Günlük, 2, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük);

            var result = await rentalService.CreateRentalAsync(rentRequest, user.Id);

            Assert.True(result.RentalEndDate > DateTime.UtcNow.AddDays(1));
        }
    }
}
