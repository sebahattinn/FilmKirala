using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // AutoMapperAb
            CreateMap<CreateMovieDto, Movie>();
            CreateMap<Movie, MovieListDto>()
                .ForMember(dest => dest.IsStockAvailable, opt => opt.MapFrom(src => src.Stock > 0))
                .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src =>
                    (src.RentalPricings != null && src.RentalPricings.Any()) ? src.RentalPricings.Min(p => p.Price) : 0));

            CreateMap<Movie, MovieDetailDto>()
                .ForMember(dest => dest.RentalOptions, opt => opt.MapFrom(src => src.RentalPricings))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    (src.Reviews != null && src.Reviews.Any()) ? src.Reviews.Average(r => (int)r.Rating) : 0));

            CreateMap<PricingDto, RentalPricing>().ReverseMap();

            CreateMap<RegisterRequestDto, User>()
                .ConstructUsing(src => new User(src.Username, src.Email, string.Empty, string.Empty, 0, Domain.Enums.Roles.User));

            CreateMap<User, AuthResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Roles.ToString()));

            // Rental Mappings - Null Check for Movie added
            CreateMap<Rental, RentalListDto>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie != null ? src.Movie.Title : "Bilinmeyen Film"));

            // Review Mappings - Null Check for User added
            CreateMap<CreateReviewDto, Review>();
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : "Anonim"))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => (int)src.Rating));
        }
    }
}