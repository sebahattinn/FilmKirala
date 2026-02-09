using FluentValidation;
using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Validators
{
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.MovieId)
                .GreaterThan(0).WithMessage("Film seçimi zorunludur.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Yorum metni boş olamaz.")
                .MinimumLength(3).WithMessage("Yorum en az 3 karakter olmalıdır.")
                .MaximumLength(500).WithMessage("Yorum 500 karakterden uzun olamaz.");

            // Enum'ı (int) yaparak sayısal değerini kontrol ediyoruz (1 ile 5 arası mı?)
            RuleFor(x => (int)x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Geçersiz puanlama. Lütfen 1 ile 5 arasında bir değer giriniz.");
        }
    }
}