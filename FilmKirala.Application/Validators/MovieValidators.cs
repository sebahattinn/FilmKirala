using FluentValidation;
using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Validators
{
    public class PricingDtoValidator : AbstractValidator<PricingDto>
    {
        public PricingDtoValidator()
        {
            RuleFor(x => x.DurationValue)
                .GreaterThan(0).WithMessage("Süre değeri 0'dan büyük olmalıdır.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

            RuleFor(x => x.DurationType)
                .IsInEnum().WithMessage("Geçersiz süre tipi.");
        }
    }
    public class CreateMovieDtoValidator : AbstractValidator<CreateMovieDto>
    {
        public CreateMovieDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Film adı zorunludur.")
                .MaximumLength(300).WithMessage("Film adı 300 karakteri geçemez.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama zorunludur.")
                .MinimumLength(5).WithMessage("Açıklama çok kısa, en az 5 karakter giriniz.");

            RuleFor(x => x.Genre)
                .NotEmpty().WithMessage("Film türü boş olamaz.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stok adedi eksi olamaz.");

            RuleForEach(x => x.Pricings).SetValidator(new PricingDtoValidator());
        }
    }
    public class UpdateMovieDtoValidator : AbstractValidator<UpdateMovieDto>
    {
        public UpdateMovieDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçersiz Film ID.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Film adı boş olamaz.")
                .MaximumLength(300);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stok eksi olamaz.");
        }
    }
}