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
    public class UltimateCoverageTests
    {
        private static AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task MassiveCoverage_AllEdgeCases_ShouldPass()
        {
            // 1. Setup
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var rentalService = new RentalService(uow, null!, busMock.Object);
            var reviewService = new ReviewService(uow);

            // 2. Domain Entity Testleri (Rental.cs ve Movie.cs Coverage için)
            var user = new User("Seba", "seba@bursa.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Inception", "Desc", "Sci-Fi", 5, true);
            var rentalPricing = new RentalPricing(DurationType.Günlük, 1, 100, movie);

            // Rental.cs RentalsCreate ve ExpireRental metodunu koştur
            var rental = new Rental();
            rental.RentalsCreate(DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 100, true, user, movie, rentalPricing);
            rental.ExpireRental();
            Assert.False(rental.Status);

            // Movie.cs IncreaseStock metodunu koştur
            movie.IncreaseStock();
            Assert.Equal(6, movie.Stock);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            // 3. ReviewService.cs Hata Senaryosu (Uncovered lines için)
            var invalidReviewDto = new CreateReviewDto { MovieId = 9999, Comment = "Hata!", Rating = 5 };
            await Assert.ThrowsAsync<KeyNotFoundException>(() => reviewService.AddReviewAsync(invalidReviewDto, user.Id));

            // 4. ReviewService.cs Başarı Senaryosu
            var validReviewDto = new CreateReviewDto { MovieId = movie.Id, Comment = "Mükemmel!", Rating = 5 };
            await reviewService.AddReviewAsync(validReviewDto, user.Id);

            // 5. User.cs Exception Senaryoları (Kırmızı satırları siler)
            Assert.Throws<ArgumentException>(() => user.DecreaseBalance(-10));
            Assert.Throws<InvalidOperationException>(() => user.DecreaseBalance(5000));
            Assert.Throws<ArgumentException>(() => user.UpdateBalance(-50));

            // 6. Rental.cs Hata Senaryosu
            Assert.Throws<InvalidOperationException>(() =>
                rental.RentalsCreate(DateTime.UtcNow, DateTime.UtcNow, 0, true, user, movie, rentalPricing));

            // 7. Verify All
            var reviews = await uow.Reviews.GetAllAsync();
            Assert.Single(reviews);
        }
    }
}