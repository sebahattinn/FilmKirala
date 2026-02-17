using Moq;
using Xunit;
using AutoMapper;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Test.UnitTests
{
    public class MovieServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly MovieService _movieService;

        public MovieServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _movieRepoMock = new Mock<IMovieRepository>();
            _mapperMock = new Mock<IMapper>();

            // UnitOfWork içindeki Movies çağrıldığında sahte repository'yi dönmesini sağladık
            _uowMock.Setup(x => x.Movies).Returns(_movieRepoMock.Object);

            _movieService = new MovieService(_uowMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task AddMovieAsync_ShouldCallRepositoryAdd_WhenRequestIsValid()
        {
            // Arrange (Senin DTO yapına göre nesne oluşturma kısmını düzelttik)
            var dto = new CreateMovieDto
            {
                Title = "Inception",
                Description = "Nolan'ın başyapıtı",
                Genre = "Sci-Fi",
                Stock = 10,
                Pricings = new List<PricingDto>()
            };

            // Act
            await _movieService.AddMovieAsync(dto);

            // Assert
            // Movie nesnesinin repository'ye bir kez eklendiğini doğrula
            _movieRepoMock.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Once);

            // Veritabanı commit işleminin (SaveChangesAsync/CompleteAsync) çağrıldığını doğrula
            _uowMock.Verify(x => x.CompleteAsync(), Times.Once);
        }
    }
}