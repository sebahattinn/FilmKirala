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
            // 1. Kullanıcı Kontrolü
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 2. Film Kontrolü
            var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
            if (movie == null) throw new Exception("Film bulunamadı.");

            if (!movie.IsActive) throw new Exception("Bu film şu an kiralamaya kapalı.");
            if (movie.Stock <= 0) throw new Exception("Üzgünüz, film stokta kalmadı.");

            // 3. Fiyat Kontrolü
            var pricing = await _unitOfWork.RentalPricings.GetByIdAsync(request.PricingId);
            if (pricing == null) throw new Exception("Geçersiz fiyat seçimi.");
            if (pricing.MovieId != movie.Id) throw new Exception("Bu fiyat bu filme ait değil!");

            // 4. Bakiye Yeterli mi?
            // (Property'ler private set olduğu için metod kullanıyoruz)
            // Eğer User.cs'de bu metot yoksa hata verir, eklediğinden emin ol.
            user.DecreaseBalance(pricing.Price);

            // 5. Stok Düş
            movie.DecreaseStock();

            // 6. Rental Kaydı Oluştur
            // Constructor private olduğu için Entity içindeki metodu kullanıyoruz.
            var rental = new Rental();
            rental.RentalsCreate(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(pricing.DurationValue),
                pricing.Price,
                true, // Status: Aktif
                user,
                movie,
                pricing
            );

            await _unitOfWork.Rentals.AddAsync(rental);

            // 7. Değişiklikleri Kaydet (User bakiye düştü, Movie stok düştü, Rental eklendi)
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId)
        {
            // 1. Kullanıcının kiralamalarını çek
            var rentals = await _unitOfWork.Rentals.FindAsync(r => r.UserId == userId);

            // 2. Mapping
            var rentalDtos = _mapper.Map<IEnumerable<RentalListDto>>(rentals);

            // 3. Film İsimlerini Doldur (Include olmadığı için manuel yapıyoruz)
            // Bu kısım biraz maliyetli ama GenericRepo ile yapabileceğimiz en hızlı çözüm bu.
            foreach (var dto in rentalDtos)
            {
                // DTO'daki ID ile orijinal Rental'ı bulup MovieId'sini alıyoruz
                // (Mapper bazen source nesneye erişimi kaybedebilir, o yüzden ID'den gidiyoruz)

                var originalRental = rentals.FirstOrDefault(r => r.Id == dto.Id);
                if (originalRental != null)
                {
                    // Filmi tekrar çekiyoruz (Memory Cache olsa süper olurdu ama şimdilik veritabanı)
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