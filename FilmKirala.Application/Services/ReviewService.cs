using AutoMapper;
using FilmKirala.Application.DTOs;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddReviewAsync(CreateReviewDto dto, int userId)
        {
            var movie = await _unitOfWork.Movies.GetByIdAsync(dto.MovieId);
            if (movie == null)
                throw new Exception($"'{dto.MovieId}' ID'li film bulunamadı! Olmayan filme yorum yapamazsınız.");

            var review = new Review(
                userId,
                dto.MovieId,
                dto.Comment,
                dto.Rating
            );
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CompleteAsync();
        }
    }
}