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
        IBusService busService) : IRentalService
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
            await unitOfWork.CompleteAsync();

            await busService.PublishAsync(new FilmRentedEvent
            {
                Email = user.Email,
                Subject = "Film Kiralama Başarılı! ",
                Message = $"'{movie.Title}' kiralandı. Tutar: {totalCost} TL"
            });

            return new RentResponseDto(true, "Kiralama başarılı!", totalCost,
                user.WalletBalance, endDate, pricing.DurationType.ToString(), request.Quantity,
                $"{request.Quantity} {pricing.DurationType} Kiralama");
        }

        public async Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId)
        {
            await CheckExpiredRentalsAsync();
            var rentals = await unitOfWork.Rentals.FindAsync(r => r.UserId == userId);
            return mapper.Map<IEnumerable<RentalListDto>>(rentals);
        }

        public async Task CheckExpiredRentalsAsync()
        {
            var expired = await unitOfWork.Rentals.FindAsync(r => r.Status && r.EndRentalDate <= DateTime.UtcNow);
            foreach (var rental in expired)
            {
                rental.ExpireRental();
                var movie = await unitOfWork.Movies.GetByIdAsync(rental.MovieId);
                movie?.IncreaseStock();
            }

            if (expired.Any()) await unitOfWork.CompleteAsync();
        }

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