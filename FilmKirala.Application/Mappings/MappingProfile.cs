using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()              //Tertemiz automapper ab
        {
            CreateMap<CreateMovieDto, Movie>();
            CreateMap<Movie, MovieListDto>();

            CreateMap<Movie, MovieDetailDto>()
                .ForMember(dest => dest.RentalOptions, opt => opt.MapFrom(src => src.RentalPricings));

            CreateMap<PricingDto, RentalPricing>().ReverseMap();

            CreateMap<RegisterRequestDto, User>();

            CreateMap<User, AuthResponseDto>();

            CreateMap<Rental, RentalListDto>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie.Title));

            CreateMap<CreateReviewDto, Review>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));
        }
    }
}