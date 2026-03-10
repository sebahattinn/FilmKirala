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
    public class RentalSystemDeepTests
    {
        private static AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private UnitOfWork CreateUnitOfWork(AppDbContext context)
        {
            var movieRepo = new MovieRepository(context, NullLogger<MovieRepository>.Instance);
            var userRepo = new UserRepository(context, NullLogger<UserRepository>.Instance);
            var pricingRepo = new RentalPricingRepository(context, NullLogger<RentalPricingRepository>.Instance);
            var loggerFactory = new NullLoggerFactory();

            return new UnitOfWork(context, movieRepo, userRepo, pricingRepo, loggerFactory);
        }

        [Fact]
        public async Task Rental_EdgeCase_InsufficientBalance_ShouldRollbackAndThrow()
        {
            using var context = GetDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 100, Roles.User);
            var movie = new Movie("Batman", "Desc", "Action", 5, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 150);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Günlük);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RentMovieAsync(request, user.Id));

            var dbUser = await context.Users.FindAsync(user.Id);
            Assert.Equal(100, dbUser!.WalletBalance);
        }

        [Fact]
        public async Task Rental_EdgeCase_OutOfStock_ShouldThrow()
        {
            using var context = GetDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Inception", "Desc", "Sci-Fi", 0, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Günlük);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RentMovieAsync(request, user.Id));
            Assert.Contains("stok", ex.Message.ToLower());
        }

        [Fact]
        public async Task Rental_EdgeCase_UndefinedPricing_ShouldThrow()
        {
            using var context = GetDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 500, Roles.User);
            var movie = new Movie("The Whale", "Desc", "Drama", 10, true);
            // Bu filme hiç pricing eklenmedi; Haftalık isteyince KeyNotFoundException fırlatmalı

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Haftalık);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RentMovieAsync(request, user.Id));
            Assert.Contains("haftalık", ex.Message.ToLower());
        }

        [Fact]
        public async Task Rental_TimeCalculation_ShouldSetCorrectEndDate()
        {
            using var context = GetDbContext();
            var uow = CreateUnitOfWork(context);
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>();

            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 200, Roles.User);
            var movie = new Movie("Flash", "Desc", "Action", 5, true);
            movie.AddRentalPricing(DurationType.Saatlik, 3, 10);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Saatlik);

            var result = await service.RentMovieAsync(request, user.Id);

            var expectedDate = DateTime.UtcNow.AddHours(3);
            Assert.True(Math.Abs((result.RentalEndDate - expectedDate).TotalMinutes) < 1);
        }
    }
}
