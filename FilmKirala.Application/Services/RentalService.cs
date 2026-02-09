using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Services
{
    public class RentalService : IRentalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RentalService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task RentMovieAsync(RentRequestDto request, int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
            if (movie == null) throw new Exception("Film bulunamadı.");

            if (!movie.IsActive) throw new Exception("Bu film şu an kiralamaya kapalı.");
            if (movie.Stock <= 0) throw new Exception("Üzgünüz, film stokta kalmadı.");

            var pricing = await _unitOfWork.RentalPricings.GetByIdAsync(request.PricingId);
            if (pricing == null) throw new Exception("Geçersiz fiyat seçimi.");
            if (pricing.MovieId != movie.Id) throw new Exception("Bu fiyat bu filme ait değil!");

            user.DecreaseBalance(pricing.Price);


            movie.DecreaseStock();

            var rental = new Rental();
            rental.RentalsCreate(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(pricing.DurationValue),
                pricing.Price,
                true, 
                user,
                movie,
                pricing
            );

            await _unitOfWork.Rentals.AddAsync(rental);

            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId)
        {
            var rentals = await _unitOfWork.Rentals.FindAsync(r => r.UserId == userId);

            var rentalDtos = _mapper.Map<IEnumerable<RentalListDto>>(rentals); //Mapping

            foreach (var dto in rentalDtos)
            {

                var originalRental = rentals.FirstOrDefault(r => r.Id == dto.Id);
                if (originalRental != null)
                {
                    var movie = await _unitOfWork.Movies.GetByIdAsync(originalRental.MovieId);
                    if (movie != null)
                    {
                        dto.MovieTitle = movie.Title;
                    }
                }
            }

            return rentalDtos;
        }

        public Task CheckExpiredRentalsAsync()
        {
            throw new NotImplementedException();
        }
    }
}