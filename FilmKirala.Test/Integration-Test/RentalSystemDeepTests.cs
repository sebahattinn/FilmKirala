using Microsoft.EntityFrameworkCore;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Application.Services;
using FilmKirala.Infrastructure.Repositories;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using Moq;
using Xunit;

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

        [Fact]
        public async Task Rental_InsufficientBalance_EdgeCase()
        {
            using var context = GetUniqueDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var rentalService = new RentalService(uow, null!, new Mock<IBusService>().Object);

            var user = new User("PoorUser", "poor@test.com", "h", "s", 100, Roles.User);
            var movie = new Movie("Expensive Film", "Desc", "Genre", 10, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 500);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.RentMovieAsync(rentRequest, user.Id));
        }

        [Fact]
        public async Task Rental_StockOut_EdgeCase()
        {
            using var context = GetUniqueDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var rentalService = new RentalService(uow, null!, new Mock<IBusService>().Object);

            var user = new User("StockTest", "stock@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("NoStock", "Desc", "Genre", 0, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük, 1);

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.RentMovieAsync(rentRequest, user.Id));
        }

        [Fact]
        public async Task Rental_Duration_Calculation_Verification()
        {
            using var context = GetUniqueDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var rentalService = new RentalService(uow, null!, new Mock<IBusService>().Object);

            var user = new User("Tester", "test@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Matrix", "Desc", "Sci-Fi", 10, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük, 2);
            var result = await rentalService.RentMovieAsync(rentRequest, user.Id);

            Assert.True(result.RentalEndDate > DateTime.UtcNow.AddDays(1));
        }
    }
}