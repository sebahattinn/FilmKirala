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

            //  FIX: 4 parametreye güncellendi
            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 100, Roles.User);
            var movie = new Movie("Batman", "Desc", "Action", 5, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 20);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Günlük, 6);

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

            //  FIX: 4 parametreye güncellendi
            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 1000, Roles.User);
            var movie = new Movie("Inception", "Desc", "Sci-Fi", 0, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Günlük, 1);

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

            //  FIX: 4 parametreye güncellendi
            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 500, Roles.User);
            var movie = new Movie("The Whale", "Desc", "Drama", 10, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 30);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Haftalık, 1);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RentMovieAsync(request, user.Id));
            Assert.Contains("mevcut değil", ex.Message.ToLower());
        }

        [Fact]
        public async Task Rental_TimeCalculation_ShouldSetCorrectEndDate()
        {
            using var context = GetDbContext();
            var uow = new UnitOfWork(context, new MovieRepository(context), new UserRepository(context));
            var busMock = new Mock<IBusService>();
            var cacheMock = new Mock<ICacheService>(); 

            //  FIX: 4 parametreye güncellendi
            var service = new RentalService(uow, null!, busMock.Object, cacheMock.Object);

            var user = new User("Seba", "test@test.com", "h", "s", 200, Roles.User);
            var movie = new Movie("Flash", "Desc", "Action", 5, true);
            movie.AddRentalPricing(DurationType.Saatlik, 1, 10);

            await context.Users.AddAsync(user);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            var request = new RentRequestDto(movie.Id, DurationType.Saatlik, 3);

            var result = await service.RentMovieAsync(request, user.Id);

            var expectedDate = DateTime.UtcNow.AddHours(3);
            Assert.True((result.RentalEndDate - expectedDate).TotalMinutes < 1);
        }
    }
}