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

            var pricing = movie.RentalPricings.FirstOrDefault(p => p.DurationType == request.DurationType)
                           ?? throw new InvalidOperationException("Bu kiralama seçeneği mevcut değil.");

            int totalCost = pricing.Price * request.Quantity;

           
            user.DecreaseBalance(totalCost);
            movie.DecreaseStock();

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
                DurationType.Saatlik => DateTime.UtcNow.AddHours(total),
                DurationType.Günlük => DateTime.UtcNow.AddDays(total),
                _ => DateTime.UtcNow.AddDays(total)
            };
        }
    }
}