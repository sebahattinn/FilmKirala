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
            // --- DDD & Senior Yaklaşım ---
            // Write (Yazma) işlemlerinde AutoMapper kullanmıyoruz.
            // Çünkü Entity'nin Constructor'ındaki iş kurallarının (Validation) çalışmasını istiyoruz.

            var movie = new Movie(
                dto.Title,
                dto.Description,
                dto.Genre,
                dto.Stock,
                true // Yeni eklenen film varsayılan olarak aktiftir
            );

            // Fiyatlandırmaları ekle
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
            // Read (Okuma) işlemlerinde AutoMapper kullanıyoruz (Hız ve Kolaylık)
            var movies = await _unitOfWork.Movies.GetAllAsync();
            return _mapper.Map<IEnumerable<MovieListDto>>(movies);
        }

        public async Task<MovieDetailDto> GetMovieByIdAsync(int id)
        {
            var movie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(id);

            if (movie == null) throw new Exception("Film bulunamadı!");

            // Artık MovieDetailDto boş constructor'a sahip olduğu için burası hata vermeyecek
            return _mapper.Map<MovieDetailDto>(movie);
        }
    }
}