using FilmKirala.Application.DTOs;

namespace FilmKirala.Application.Interfaces.Services
{
    public interface IRentalService
    {
        // Kiralama İsteği:
        // 1. Kullanıcının bakiyesi yeterli mi?
        // 2. Film stokta var mı?
        // 3. Tarih hesaplaması (Günlük/Haftalık)
        // 4. Kiralama kaydı oluşturma
        Task RentMovieAsync(RentRequestDto request, int userId); // UserId token'dan gelecek

        // Kullanıcının geçmiş ve aktif kiralamalarını listeler
        Task<IEnumerable<RentalListDto>> GetUserRentalsAsync(int userId);

        // Kiralamayı süresi dolunca pasife çekme veya iade alma işlemleri (Opsiyonel/İleri Seviye)
        Task CheckExpiredRentalsAsync();
    }
}