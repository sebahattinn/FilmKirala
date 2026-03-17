using Moq;
using Xunit;
using AutoMapper;
using FilmKirala.Application.Services;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Test.UnitTests
{
    public class MovieServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly MovieService _movieService;

        public MovieServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _movieRepoMock = new Mock<IMovieRepository>();
            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<ICacheService>();

            // UnitOfWork içindeki Movies çağrıldığında sahte repository'yi dönmesini sağladım
            _uowMock.Setup(x => x.Movies).Returns(_movieRepoMock.Object);

            _movieService = new MovieService(_uowMock.Object, _mapperMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task CreateMovieAsync_ShouldCallRepositoryAdd_WhenRequestIsValid()
        {
            var dto = new CreateMovieDto
            {
                Title = "Inception",
                Description = "Nolan'ın başyapıtı",
                Genre = "Sci-Fi",
                Stock = 10,
                Pricings = new List<PricingDto>()
            };

           
            await _movieService.CreateMovieAsync(dto);

         
            _movieRepoMock.Verify(x => x.AddAsync(It.IsAny<Movie>()), Times.Once);

            _uowMock.Verify(x => x.CompleteAsync(), Times.Once);
        }
    }
}