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

        public MovieService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MovieListDto>> GetAllMoviesAsync(string? search = null, string? genre = null, int page = 1, int pageSize = 20)
        {
            var searchTerm = search?.Trim();

            // ÖNEMLİ: GetPagedAsync içinde sıralamayı SQL tarafında yapacak bir yapı kurduk.
            // Bu sayede 10 milyon veri içinden sadece ilgili 20 kayıt çekilirken indeks kullanılır.
            var movies = await _unitOfWork.Movies.GetPagedAsync(
                page,
                pageSize,
                predicate: m => (string.IsNullOrEmpty(searchTerm) || m.Title.StartsWith(searchTerm)) &&
                                (string.IsNullOrEmpty(genre) || m.Genre == genre)
            );

            // Veriler zaten SQL'de ID'ye göre sıralı çekilmeli (Repo içinde düzelteceğiz), 
            // ama burada mapper ile DTO'ya çevirip dönüyoruz.
            return _mapper.Map<IEnumerable<MovieListDto>>(movies);
        }

        public async Task<MovieDetailDto?> GetMovieByIdAsync(int id)
        {
            // Detay sayfasında tracking maliyeti düşüktür ama performans için GetMovieWithDetailsAsync 
            // içinde AsNoTracking() olması her zaman iyidir.
            var movie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(id);
            if (movie == null) throw new KeyNotFoundException($"Film bulunamadı (ID: {id})");
            return _mapper.Map<MovieDetailDto>(movie);
        }

        public async Task AddMovieAsync(CreateMovieDto createMovieDto)
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
            // Kiralama fiyatı ekleme operasyonu
            var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
            if (movie == null) throw new KeyNotFoundException("Film bulunamadı.");

            movie.AddRentalPricing(durationType, 1, price);
            await _unitOfWork.CompleteAsync();
        }
    }
}