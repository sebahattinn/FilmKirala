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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions; 

namespace FilmKirala.Test.IntegrationTests
{
    public class MasterProcessTests
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
        public async Task FullFlow_RentalAndReview_ShouldWorkPerfectly()
        {
            using var context = GetDbContext();
            var uow = CreateUnitOfWork(context); 

            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var rentalService = new RentalService(uow, null!, busMock.Object, cacheMock.Object);
            var reviewService = new ReviewService(uow);

            var user = new User("MasterSeba", "master@bursa.com", "hash", "salt", 500, Roles.User);
            await context.Users.AddAsync(user);

            var movie = new Movie("Inception Master", "Sci-Fi Epic", "Action", 10, true);
            movie.AddRentalPricing(DurationType.Günlük, 2, 100);

            await uow.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            var pricing = movie.RentalPricings.First();
            var rentRequest = new RentRequestDto(movie.Id, pricing.Id);

            var rentalResult = await rentalService.RentMovieAsync(rentRequest, user.Id);

            var reviewDto = new CreateReviewDto { MovieId = movie.Id, Comment = "Harika!", Rating = 5 };
            await reviewService.AddReviewAsync(reviewDto, user.Id);

            var dbUser = await context.Users.FindAsync(user.Id);
            var dbMovie = await context.Movies.FindAsync(movie.Id);

            Assert.Equal(400, dbUser!.WalletBalance);
            Assert.Equal(9, dbMovie!.Stock);
            Assert.NotNull(rentalResult);
        }
    }
}