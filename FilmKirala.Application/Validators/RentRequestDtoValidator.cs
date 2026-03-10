using FluentValidation;
using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Application.Validators
{
    public class RentRequestDtoValidator : AbstractValidator<RentRequestDto>
    {
        public RentRequestDtoValidator()
        {
            RuleFor(x => x.MovieId)
                .GreaterThan(0).WithMessage("Geçersiz Film ID'si.");

            RuleFor(x => x.DurationType)
                .IsInEnum().WithMessage("Geçersiz kiralama türü.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("En az 1 adet/birim süre seçmelisiniz.");
        }
    }
}
