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
    public class UltimateCoverageTests
    {
        private static AppDbContext GetDbContext()
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
            var loggerFactory = new NullLoggerFactory();

            return new UnitOfWork(context, movieRepo, userRepo, loggerFactory);
        }

        [Fact]
        public async Task MassiveCoverage_AllEdgeCases_ShouldPass()
        {
       
            using var context = GetDbContext();
            var uow = CreateUnitOfWork(context); 

            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var rentalService = new RentalService(uow, null!, busMock.Object, cacheMock.Object);
            var reviewService = new ReviewService(uow);

            var user = new User("Seba", "seba@bursa.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Inception", "Desc", "Sci-Fi", 5, true);
            var rentalPricing = new RentalPricing(DurationType.Günlük, 1, 100, movie);

            var rental = new Rental();
            rental.RentalsCreate(DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 100, true, user, movie, rentalPricing);
            rental.ExpireRental();
            Assert.False(rental.Status);

            movie.IncreaseStock();
            Assert.Equal(6, movie.Stock);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var invalidReviewDto = new CreateReviewDto { MovieId = 9999, Comment = "Hata!", Rating = 5 };
            await Assert.ThrowsAsync<KeyNotFoundException>(() => reviewService.AddReviewAsync(invalidReviewDto, user.Id));

            var validReviewDto = new CreateReviewDto { MovieId = movie.Id, Comment = "Mükemmel!", Rating = 5 };
            await reviewService.AddReviewAsync(validReviewDto, user.Id);

            Assert.Throws<ArgumentException>(() => user.DecreaseBalance(-10));
            Assert.Throws<InvalidOperationException>(() => user.DecreaseBalance(5000));
            Assert.Throws<ArgumentException>(() => user.UpdateBalance(-50));

            Assert.Throws<InvalidOperationException>(() =>
                rental.RentalsCreate(DateTime.UtcNow, DateTime.UtcNow, 0, true, user, movie, rentalPricing));

            var reviews = await uow.Reviews.GetAllAsync();
            Assert.Single(reviews);
        }
    }
}