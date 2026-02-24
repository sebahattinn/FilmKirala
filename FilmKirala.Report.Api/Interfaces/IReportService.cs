using Microsoft.AspNetCore.Mvc;

namespace FilmKirala.Report.Api.Interfaces
{
    public interface IReportService
    {
        // Rapor özeti dönen metot
        Task<object> GetMovieSummaryAsync(int lastId, int pageSize);

        // Excel stream dönen metot
        Task<MemoryStream> ExportMoviesToExcelAsync(int? lastId = null, int? pageSize = null);
    }
}