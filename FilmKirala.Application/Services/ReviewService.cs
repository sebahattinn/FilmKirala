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
            // Kullanıcı bu filmi kiralamış mı kontrolü yapılabilir (İleri Seviye)
            // Şimdilik herkes yorum yapabilsin.

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