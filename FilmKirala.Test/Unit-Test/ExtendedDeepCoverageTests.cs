using Moq;
using Xunit;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Repositories;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions; 

namespace FilmKirala.Test.UnitTests
{
    public class ExtendedDeepCoverageTests
    {
        private static AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task MovieService_FullLogic_Coverage()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var cacheMock = new Mock<ICacheService>();
            var movieService = new MovieService(uowMock.Object, null!, cacheMock.Object);

            uowMock.Setup(x => x.Movies.GetMovieWithDetailsAsync(It.IsAny<int>())).ReturnsAsync((MovieDetailDto)null!);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.GetMovieByIdAsync(999));

            uowMock.Verify(x => x.Movies.GetMovieWithDetailsAsync(999), Times.Once);
        }

        [Fact]
        public async Task ReviewService_AllPaths_Coverage()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var reviewService = new ReviewService(uowMock.Object);

            uowMock.Setup(x => x.Movies.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Movie)null!);

            var dto = new CreateReviewDto { MovieId = 1, Comment = "Harika", Rating = 5 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => reviewService.AddReviewAsync(dto, 1));
        }

        [Fact]
        public async Task Repository_ComplexQueries_Coverage()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context, NullLogger<UserRepository>.Instance);
            var user = new User("SebaRepo", "repo@test.com", "h", "s", 500, Roles.User);

            await userRepo.AddAsync(user);
            await context.SaveChangesAsync();

            var found = await userRepo.GetByEmailAsync("repo@test.com");
            Assert.NotNull(found);

            user.UpdateBalance(600);

            userRepo.Update(user);
            userRepo.Remove(user);
            await context.SaveChangesAsync();

            Assert.Null(await userRepo.GetByEmailAsync("repo@test.com"));
        }
    }
}