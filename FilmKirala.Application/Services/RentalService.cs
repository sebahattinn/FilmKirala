using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Shared.Events;

namespace FilmKirala.Application.Services
{
    public class RentalService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBusService busService,
        ICacheService cacheService) : IRentalService 
    {
        public async Task<RentResponseDto> RentMovieAsync(RentRequestDto request, int userId)
        {
            var user = await unitOfWork.Users.GetByIdAsync(userId)
                        ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

            var movie = await unitOfWork.Movies.GetMovieWithDetailsAsync(request.MovieId)
                         ?? throw new KeyNotFoundException("Film bulunamadı.");

            // Pricing ID'ye göre buluyoruz — kullanıcı fiyatı manipüle edemez, DB'den doğruluyoruz
            // Aynı zamanda bu pricing'in gerçekten o filme ait olup olmadığını garanti etmiş oluyoruz
            var pricing = movie.RentalPricings.FirstOrDefault(p => p.Id == request.RentalPricingId)
                           ?? throw new InvalidOperationException("Geçersiz fiyatlandırma seçeneği.");

            // Stok yeterliliği (quantity kadar kopya var mı?)
            if (movie.Stock < request.Quantity)
                throw new InvalidOperationException(
                    $"'{movie.Title}' stok yetersiz! Mevcut: {movie.Stock}, İstenen: {request.Quantity}");

            int totalCost = pricing.Price * request.Quantity;

            // Bakiye kontrolü User.DecreaseBalance içinde de var ama önce explicit kontrol
            if (user.WalletBalance < totalCost)
                throw new InvalidOperationException(
                    $"Bakiye yetersiz! Gereken: {totalCost} TL, Mevcut: {user.WalletBalance} TL");

            user.DecreaseBalance(totalCost);
            movie.DecreaseStock(request.Quantity);

            DateTime endDate = CalculateEndDate(pricing.DurationType, pricing.DurationValue, request.Quantity);

            var rental = new Rental();
            rental.RentalsCreate(DateTime.UtcNow, endDate, totalCost, true, user, movie, pricing);

            await unitOfWork.Rentals.AddAsync(rental);

         
            await unitOfWork.CompleteAsync();   // save the db

           
            await cacheService.RemoveAsync($"user_profile_{userId}");        //Remove the Cache 

            await busService.PublishAsync(new FilmRentedEvent               
            {
                Email = user.Email,
                Subject = "Film Kiralama Başarılı!", 
                Message = $"'{movie.Title}' kiralandı. Tutar: {totalCost} TL"          
            });

            return new RentResponseDto(true, "Kiralama başarılı!", totalCost,
                user.WalletBalance, endDate, pricing.DurationType.ToString(), request.Quantity,
                $"{request.Quantity} {pricing.DurationType} Kiralama");
        }

        public async Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId)
        {
            var rentals = await unitOfWork.Rentals.FindAsync(r => r.UserId == userId);
            var orderedRentals = rentals.OrderByDescending(r => r.Id).ToList();

            return mapper.Map<IEnumerable<RentalListDto>>(orderedRentals);
        }

        public async Task CheckExpiredRentalsAsync(int? userId = null)
        {
            var query = userId.HasValue
                ? await unitOfWork.Rentals.FindAsync(r => r.UserId == userId.Value && r.Status && r.EndRentalDate <= DateTime.UtcNow)
                : await unitOfWork.Rentals.FindAsync(r => r.Status && r.EndRentalDate <= DateTime.UtcNow);

            var expired = query.ToList();

            if (expired.Any())
            {
                var movieIds = expired.Select(r => r.MovieId).Distinct().ToList();
                var movies = (await unitOfWork.Movies.FindAsync(m => movieIds.Contains(m.Id))).ToDictionary(m => m.Id);

                foreach (var rental in expired)
                {
                    rental.ExpireRental();
                    if (movies.TryGetValue(rental.MovieId, out var movie))
                    {
                        movie.IncreaseStock();
                    }
                }
                await unitOfWork.CompleteAsync();
            }
        }

        public async Task CheckExpiredRentalsAsync() => await CheckExpiredRentalsAsync(null);

        private static DateTime CalculateEndDate(DurationType type, int val, int qty)
        {
            int total = val * qty;
            return type switch
            {
                DurationType.Saatlik  => DateTime.UtcNow.AddHours(total),
                DurationType.Günlük   => DateTime.UtcNow.AddDays(total),
                DurationType.Haftalık => DateTime.UtcNow.AddDays(total * 7),
                DurationType.Aylık    => DateTime.UtcNow.AddMonths(total),
                DurationType.Yıllık   => DateTime.UtcNow.AddYears(total),
                _ => DateTime.UtcNow.AddDays(total)
            };
        }
    }
}