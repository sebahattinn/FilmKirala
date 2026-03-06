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
using System.Linq;

namespace FilmKirala.Test.IntegrationTests
{
    public class RentalSystemDeepTests
    {
        private static AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task Rental_EdgeCase_InsufficientBalance_ShouldRollbackAndThrow()
        {
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 100, Roles.User);
            var movie = new Movie("Batman", "Desc", "Action", 5, true);

            // Fiyatlandırma ekle ve kaydet ki ID oluşsun
            movie.AddRentalPricing(DurationType.Günlük, 1, 150); // Bakiye 100, Fiyat 150

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            // FIX: En son eklenen pricing ID'sini alıyoruz
            var pricingId = movie.RentalPricings.First().Id;
            var request = new RentRequestDto(movie.Id, pricingId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RentMovieAsync(request, user.Id));

            var dbUser = await context.Users.FindAsync(user.Id);
            Assert.Equal(100, dbUser!.WalletBalance);
        }

        [Fact]
        public async Task Rental_EdgeCase_OutOfStock_ShouldThrow()
        {
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Inception", "Desc", "Sci-Fi", 0, true); // Stok 0
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var pricingId = movie.RentalPricings.First().Id;
            var request = new RentRequestDto(movie.Id, pricingId);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RentMovieAsync(request, user.Id));
            Assert.Contains("stok", ex.Message.ToLower());
        }

        [Fact]
        public async Task Rental_EdgeCase_UndefinedPricing_ShouldThrow()
        {
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 500, Roles.User);
            var movie = new Movie("The Whale", "Desc", "Drama", 10, true);
            // Pricing eklemiyoruz

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            // Olmayan bir Pricing ID (999) gönderiyoruz
            var request = new RentRequestDto(movie.Id, 999);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RentMovieAsync(request, user.Id));
            Assert.Contains("fiyat", ex.Message.ToLower());
        }

        [Fact]
        public async Task Rental_TimeCalculation_ShouldSetCorrectEndDate()
        {
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 200, Roles.User);
            var movie = new Movie("Flash", "Desc", "Action", 5, true);
            movie.AddRentalPricing(DurationType.Saatlik, 3, 10); // 3 Saatlik paket

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var pricingId = movie.RentalPricings.First().Id;
            var request = new RentRequestDto(movie.Id, pricingId);

            var result = await service.RentMovieAsync(request, user.Id);

            var expectedDate = DateTime.UtcNow.AddHours(3);
            Assert.True(Math.Abs((result.RentalEndDate - expectedDate).TotalMinutes) < 1);
        }
    }
}