using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Shared.Events;
using Microsoft.Extensions.Logging;

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
                        ?? throw new KeyNotFoundException("User is not founded.");

            var movie = await unitOfWork.Movies.GetMovieWithDetailsAsync(request.MovieId)
                         ?? throw new KeyNotFoundException("Movie is not founded.");

            // Senior Dokunuşu: Hata aldığında sadece "Geçersiz" demek yerine mevcut ID'leri de fırlatıyoruz ki sorunu anlayalım.
            var pricing = movie.RentalPricings.FirstOrDefault(p => p.Id == request.RentalPricingId);

            if (pricing == null)
            {
                var existingPricingIds = string.Join(", ", movie.RentalPricings.Select(p => p.Id));
                throw new InvalidOperationException(
                    $"Geçersiz fiyatlandırma seçeneği (ID: {request.RentalPricingId}). " +
                    $"Bu film için mevcut fiyatlandırma ID'leri: [{existingPricingIds}]");
            }

            if (movie.Stock < request.Quantity)
                throw new InvalidOperationException(
                    $"'{movie.Title}' stok yetersiz! Mevcut: {movie.Stock}, İstenen: {request.Quantity}");

            int totalCost = pricing.Price * request.Quantity;

            if (user.WalletBalance < totalCost)
                throw new InvalidOperationException(
                    $"Bakiye yetersiz! Gereken: {totalCost} TL, Mevcut: {user.WalletBalance} TL");

            user.DecreaseBalance(totalCost);
            movie.DecreaseStock(request.Quantity);

            DateTime endDate = CalculateEndDate(pricing.DurationType, pricing.DurationValue, request.Quantity);

            var rental = new Rental();
            rental.RentalsCreate(DateTime.UtcNow, endDate, totalCost, true, user, movie, pricing);

            await unitOfWork.Rentals.AddAsync(rental);

            await unitOfWork.CompleteAsync();

            try
            {
                await cacheService.RemoveAsync($"user_profile_{userId}");
            }
            catch (Exception)
            {
                // Loglama gerekirse buraya: _logger.LogWarning("Cache temizlenemedi");
            }

            try
            {
                await busService.PublishAsync(new FilmRentedEvent
                {
                    Email = user.Email,
                    Subject = "Movie Rented is successfully!",
                    Message = $"'{movie.Title}' Rented. Tutar: {totalCost} TL"
                });
            }
            catch (Exception)
            {

                
            }

            return new RentResponseDto(true, "Movie Rented is successfully!", totalCost,
                user.WalletBalance, endDate, pricing.DurationType.ToString(), request.Quantity,
                $"{request.Quantity} {pricing.DurationType} Rentals");
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