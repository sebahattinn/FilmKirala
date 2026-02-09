using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MovieService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddMovieAsync(CreateMovieDto dto)
        {
            var movie = new Movie(      
                dto.Title,
                dto.Description,
                dto.Genre,
                dto.Stock,
                true // Yeni eklenen film default olarak aktiftir
            );

            if (dto.Pricings != null && dto.Pricings.Any())
            {
                foreach (var priceDto in dto.Pricings)
                {
                    movie.AddRentalPricing(
                        priceDto.DurationType,
                        priceDto.DurationValue,
                        priceDto.Price
                    );
                }
            }

            await _unitOfWork.Movies.AddAsync(movie);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<MovieListDto>> GetAllMoviesAsync()
        {
            var movies = await _unitOfWork.Movies.GetAllAsync();
            return _mapper.Map<IEnumerable<MovieListDto>>(movies);
        }

        public async Task<MovieDetailDto> GetMovieByIdAsync(int id)
        {
            var movie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(id);

            if (movie == null) throw new Exception("Film bulunamadı!");

            return _mapper.Map<MovieDetailDto>(movie);
        }
    }
}