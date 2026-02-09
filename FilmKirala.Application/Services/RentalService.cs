using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums; // DurationType için ekledim

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

        // Dönüş tipini void'den RentResponseDto'ya çevirdim
        public async Task<RentResponseDto> RentMovieAsync(RentRequestDto request, int userId)
        {
            if (request.Quantity <= 0) throw new Exception("En az 1 birim süre seçmelisiniz.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // Fiyatları da çekiyoruz ki içinden seçim yapalım
            var movie = await _unitOfWork.Movies.GetMovieWithDetailsAsync(request.MovieId);

            if (movie == null) throw new Exception("Film bulunamadı.");
            if (!movie.IsActive) throw new Exception("Bu film şu an kiralamaya kapalı.");
            if (movie.Stock <= 0) throw new Exception("Üzgünüz, film stokta kalmadı.");

            // PricingId yerine kullanıcının seçtiği tipe (DurationType) göre fiyatı buluyoruz
            var pricing = movie.RentalPricings.FirstOrDefault(p => p.DurationType == request.DurationType);

            if (pricing == null)
            {
                throw new Exception($"Bu film için '{request.DurationType}' kiralama seçeneği (Fiyat Paketi) tanımlanmamış.");
            }

            // Hesaplama: Fiyat * Adet
            int totalCost = pricing.Price * request.Quantity;

            // Bakiye Düşme (User entity içindeki kontrol çalışacak)
            user.DecreaseBalance(totalCost);

            // Stok Düş
            movie.DecreaseStock();

            // Süre Hesaplama: (Pricing Süresi * İstenen Adet)
            DateTime rentalEndDate = CalculateEndDate(pricing.DurationType, pricing.DurationValue, request.Quantity);

            var rental = new Rental();
            rental.RentalsCreate(
                DateTime.UtcNow,
                rentalEndDate,
                totalCost,
                true,
                user,
                movie,
                pricing
            );
            await _unitOfWork.Rentals.AddAsync(rental);

            await _unitOfWork.CompleteAsync();

            // Fiş Kesiyoruz
            return new RentResponseDto
            {
                Success = true,
                Message = "Kiralama başarılı! İyi seyirler.",
                TotalCost = totalCost,
                RemainingBalance = user.WalletBalance,
                RentalEndDate = rentalEndDate,
                RentalType = pricing.DurationType.ToString(),
                RentalDuration = request.Quantity,
                Summary = $"{request.Quantity} {pricing.DurationType} Kiralama"
            };
        }

        // Yardımcı Metot: Tarih Hesaplama
        private DateTime CalculateEndDate(DurationType type, int durationValue, int quantity)
        {
            int totalUnits = durationValue * quantity;

            return type switch
            {
                DurationType.Saatlik => DateTime.UtcNow.AddHours(totalUnits),
                DurationType.Günlük => DateTime.UtcNow.AddDays(totalUnits),
                DurationType.Haftalık => DateTime.UtcNow.AddDays(totalUnits * 7),
                DurationType.Aylık => DateTime.UtcNow.AddMonths(totalUnits),
                DurationType.Yıllık => DateTime.UtcNow.AddYears(totalUnits),
                _ => DateTime.UtcNow.AddDays(totalUnits)
            };
        }
        public async Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId)
        {
            await CheckExpiredRentalsAsync();

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
        public async Task CheckExpiredRentalsAsync()
        {
            var activeRentals = await _unitOfWork.Rentals.FindAsync(r => r.Status == true);
            bool isChanged = false;

            foreach (var rental in activeRentals)
            {
                if (rental.EndRentalDate <= DateTime.UtcNow)
                {
                    rental.ExpireRental();

                    var movie = await _unitOfWork.Movies.GetByIdAsync(rental.MovieId);
                    if (movie != null)
                    {
                        movie.IncreaseStock();
                    }
                    isChanged = true;
                }
            }

            if (isChanged)
            {
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}