using FluentValidation;
using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Validators
{
    public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
                .Length(3, 50).WithMessage("Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email adresi boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalı.")
             //   .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermeli.") test ederken ömrümü çürüttü bu satır aq
                .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermeli.");
        }
    }

    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email adresi gereklidir.")
                .EmailAddress().WithMessage("Email formatı hatalı.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre gereklidir.");
        }
    }
    public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestDtoValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access Token gereklidir.");
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh Token gereklidir.");
        }
    }
}