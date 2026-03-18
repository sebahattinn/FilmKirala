using System.Linq.Expressions;
using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FilmKirala.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public MovieService(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<PagedResult<MovieListDto>> GetAllMoviesAsync(string? search = null, string? genre = null, int page = 1, int pageSize = 20)
        {
            var searchTerm = search?.Trim();

            Expression<Func<Movie, bool>> predicate = m =>
                (string.IsNullOrEmpty(searchTerm) || m.Title.Contains(searchTerm)) &&
                (string.IsNullOrEmpty(genre) || m.Genre == genre);

            var movies = await _unitOfWork.Movies.GetPagedAsync(page, pageSize, predicate: predicate);
            var totalCount = await _unitOfWork.Movies.CountAsync(predicate);

            return new PagedResult<MovieListDto>
            {
                Items = _mapper.Map<IEnumerable<MovieListDto>>(movies),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MovieDetailDto?> GetMovieByIdAsync(int id)
        {
            return await _cacheService.GetOrSetAsync(
                key: $"movie_{id}",
                factory: async () =>
                {
                    var dto = await _unitOfWork.Movies.GetMovieWithDetailsAsync(id);
                    if (dto == null) throw new KeyNotFoundException($"Film not found (ID: {id})");
                    return dto;
                },
                expiration: TimeSpan.FromMinutes(30)
            );
        }

        public async Task CreateMovieAsync(CreateMovieDto createMovieDto)
        {
            var movie = new Movie(createMovieDto.Title, createMovieDto.Description, createMovieDto.Genre, createMovieDto.Stock, true);

            if (createMovieDto.Pricings != null)
            {
                foreach (var price in createMovieDto.Pricings)
                    movie.AddRentalPricing(price.DurationType, price.DurationValue, price.Price);
            }

            await _unitOfWork.Movies.AddAsync(movie);
            await _unitOfWork.CompleteAsync();
        }

        public async Task AddRentalPricingAsync(int movieId, DurationType durationType, int price)
        {
            var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
            if (movie == null) throw new KeyNotFoundException($"Movie not found (ID: {movieId})");

            movie.AddRentalPricing(durationType, 1, price);
            await _unitOfWork.CompleteAsync();

            _ = _cacheService.RemoveAsync($"movie_{movieId}");
        }
    }
}