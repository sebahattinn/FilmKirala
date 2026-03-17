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
        public async Task<RentResponseDto> CreateRentalAsync(RentRequestDto request, int userId)
        {
          
            var pricing = await unitOfWork.RentalPricings.GetPricingByMovieAndTypeAsync(request.MovieId, request.DurationType)
                          ?? throw new KeyNotFoundException(
                              $"'{request.DurationType}' Rental options of this type are not defined for this film.");

            var movie = pricing.Movie;

            var user = await unitOfWork.Users.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            if (movie.Stock < request.Quantity)
                throw new InvalidOperationException(
                    $"'{movie.Title}' Out of stock! Available: {movie.Stock}, Requested: {request.Quantity}");

            int totalCost = pricing.Price * request.Quantity;

            if (user.WalletBalance < totalCost)
                throw new InvalidOperationException(
                    $"Insufficient balance! Required: {totalCost} TL, Current: {user.WalletBalance} TL");

            user.DecreaseBalance(totalCost);
            movie.DecreaseStock(request.Quantity);


            DateTime endDate = CalculateEndDate(pricing.DurationType, pricing.DurationValue, request.Quantity);

            var rental = new Rental();
            rental.RentalsCreate(DateTime.UtcNow, endDate, totalCost, true, user, movie, pricing, request.Quantity);

            await unitOfWork.Rentals.AddAsync(rental);
            await unitOfWork.CompleteAsync();

            //  Fire and Forget async Calls
            _ = cacheService.RemoveAsync($"user_profile_{userId}");

            _ = busService.PublishAsync(new FilmRentedEvent
            {
                Email = user.Email,
                Subject = "Movie Rented is successfully!",
                Message = $"'{movie.Title}' Rented. Amount: {totalCost} TL"
            });

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
                        movie.IncreaseStock(rental.Quantity);
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