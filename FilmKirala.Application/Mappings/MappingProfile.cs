using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- MOVIE MAPPINGS ---

            // CreateMovieDto -> Movie (Film eklerken)
            CreateMap<CreateMovieDto, Movie>();

            // Movie -> MovieListDto (Vitrin listesi)
            // IsStockAvailable ve MinPrice gibi alanları manuel hesaplatmak gerekebilir 
            // ama şimdilik düz mapleyelim, service içinde dolduruyoruz.
            CreateMap<Movie, MovieListDto>();

            // Movie -> MovieDetailDto (Detay sayfası)
            CreateMap<Movie, MovieDetailDto>()
                .ForMember(dest => dest.RentalOptions, opt => opt.MapFrom(src => src.RentalPricings));


            // --- PRICING MAPPINGS ---

            // PricingDto <-> RentalPricing (Çift yönlü)
            CreateMap<PricingDto, RentalPricing>().ReverseMap();


            // --- AUTH & USER MAPPINGS ---

            // RegisterRequestDto -> User
            CreateMap<RegisterRequestDto, User>();

            // User -> AuthResponseDto (Giriş cevabı)
            CreateMap<User, AuthResponseDto>();


            // --- RENTAL MAPPINGS ---

            // Rental -> RentalListDto (Kullanıcının kiralamaları)
            CreateMap<Rental, RentalListDto>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie.Title));


            // --- REVIEW MAPPINGS ---

            // CreateReviewDto -> Review
            CreateMap<CreateReviewDto, Review>();

            // Review -> ReviewDto (Film detayındaki yorum listesi)
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));
        }
    }
}