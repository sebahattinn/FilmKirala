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

        [Fact]
        public async Task FullFlow_RentalAndReview_ShouldWorkPerfectly()
        {
            // 1. Setup (Arrange)
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var rentalService = new RentalService(uow, null!, busMock.Object, cacheMock.Object);
            var reviewService = new ReviewService(uow);

            // 2. Kullanıcı Hazırla
            var user = new User("MasterSeba", "master@bursa.com", "hash", "salt", 500, Roles.User);
            await context.Users.AddAsync(user);

            // 3. Film ve Fiyatlandırma Ekleme
            var movie = new Movie("Inception Master", "Sci-Fi Epic", "Action", 10, true);
            // 2 günlük kiralama için 100 TL fiyat tanımlıyoruz (50*2 gibi düşün)
            movie.AddRentalPricing(DurationType.Günlük, 2, 100);

            await uow.Movies.AddAsync(movie);
            await uow.CompleteAsync(); // ID'lerin oluşması için şart

            // 4. Kiralama Testi
            // FIX: Enum ve miktar yerine, veritabanında oluşan pricing ID'sini veriyoruz
            var pricing = movie.RentalPricings.First();
            var rentRequest = new RentRequestDto(movie.Id, pricing.Id);

            var rentalResult = await rentalService.RentMovieAsync(rentRequest, user.Id);

            // 5. Yorum Yapma Testi
            var reviewDto = new CreateReviewDto { MovieId = movie.Id, Comment = "Harika!", Rating = 5 };
            await reviewService.AddReviewAsync(reviewDto, user.Id);

            // 6. Sonuçları Doğrula
            var dbUser = await context.Users.FindAsync(user.Id);
            var dbMovie = await context.Movies.FindAsync(movie.Id);

            // Bakiye kontrolü: 500 - 100 = 400
            Assert.Equal(400, dbUser!.WalletBalance);
            // Stok kontrolü: 10 - 1 = 9
            Assert.Equal(9, dbMovie!.Stock);
            Assert.NotNull(rentalResult);
        }
    }
}