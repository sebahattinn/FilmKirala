using FluentValidation;
using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Validators
{
    public class RentRequestDtoValidator : AbstractValidator<RentRequestDto>
    {
        public RentRequestDtoValidator()
        {
            RuleFor(x => x.MovieId)
                .GreaterThan(0).WithMessage("Geçersiz Film ID'si.");

            // PricingId kalktı, yerine DurationType (Enum) kontrolü geldi
            RuleFor(x => x.DurationType)
                .IsInEnum().WithMessage("Geçersiz kiralama türü (Saatlik, Günlük vb. seçiniz).");

            // Yeni eklenen Quantity (Adet/Süre) kontrolü
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("En az 1 adet/birim süre seçmelisiniz.");
        }
    }
}