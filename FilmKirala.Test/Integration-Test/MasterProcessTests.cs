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

          
            var rentalService = new RentalService(uow, null!, busMock.Object);
            var reviewService = new ReviewService(uow);
            var movieService = new MovieService(uow, null!);

            // 2. Kullanıcı Hazırla
            var user = new User("MasterSeba", "master@bursa.com", "hash", "salt", 500, Roles.User);
            await context.Users.AddAsync(user);

           
            var createMovieDto = new CreateMovieDto
            {
                Title = "Inception Master",
                Description = "Sci-Fi Epic",
                Genre = "Action",
                Stock = 10,
                Pricings = new List<PricingDto> { new() { DurationType = DurationType.Günlük, DurationValue = 1, Price = 50 } }
            };

            // 3. Film Ekleme (Domain üzerinden direkt ekleyip coverage alıyoruz)
            var movie = new Movie(createMovieDto.Title, createMovieDto.Description, createMovieDto.Genre, createMovieDto.Stock, true);
            movie.AddRentalPricing(DurationType.Günlük, 1, 50);
            await uow.Movies.AddAsync(movie);
            await uow.CompleteAsync();

            // 4. Kiralama Testi
            var rentRequest = new RentRequestDto(movie.Id, DurationType.Günlük, 2);
            var rentalResult = await rentalService.RentMovieAsync(rentRequest, user.Id);

            // 5. Yorum Yapma Testi (🚀 FIX: CreateReviewDto initializer ile güncellendi)
            var reviewDto = new CreateReviewDto { MovieId = movie.Id, Comment = "Harika!", Rating = 5 };
            await reviewService.AddReviewAsync(reviewDto, user.Id);

            // 6. Sonuçları Doğrula (Assert)
            var dbUser = await context.Users.FindAsync(user.Id);
            var dbMovie = await context.Movies.FindAsync(movie.Id);

            Assert.Equal(400, dbUser!.WalletBalance); // 500 - (50*2)
            Assert.Equal(9, dbMovie!.Stock);
            
            Assert.NotNull(rentalResult);
        }
    }
}