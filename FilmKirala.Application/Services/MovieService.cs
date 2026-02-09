using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;

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

            // if (movie == null) throw new Exception("Film bulunamadı!");
            if (movie == null)
            {
                throw new KeyNotFoundException($"HATA: {id} ID'li film sistemde bulunamadı! Lütfen geçerli bir ID giriniz.");
            }

            return _mapper.Map<MovieDetailDto>(movie);
        }

        public async Task AddRentalPricingAsync(int movieId, DurationType durationType, int price)
        {
            var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
            if (movie == null) throw new Exception("Film bulunamadı.");

            var existingPricings = await _unitOfWork.RentalPricings.FindAsync(x => x.MovieId == movieId && x.DurationType == durationType);

            if (existingPricings.Any())
            {
                throw new Exception($"Bu film için '{durationType}' fiyatlandırması zaten yapılmış! Aynı türden iki fiyat olamaz.");
            }

            var newPricing = new RentalPricing(durationType, 1, price, movie);

            await _unitOfWork.RentalPricings.AddAsync(newPricing);
            await _unitOfWork.CompleteAsync();
        }
    }
}