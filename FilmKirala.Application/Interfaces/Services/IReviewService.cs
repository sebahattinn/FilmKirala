using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IReviewService
    {
        // Yorum ekle (Puan hesaplaması Movie üzerinde güncellenebilir veya dinamik çekilir)
        Task AddReviewAsync(CreateReviewDto createReviewDto, int userId);
    }
}