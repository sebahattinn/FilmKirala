namespace FilmKirala.Report.Api.Interfaces
{
    public interface IReportService
    {
        Task<object> GetMovieSummaryAsync(int lastId, int pageSize);
        Task<MemoryStream> ExportMoviesAsync(int? lastId = null, int? pageSize = null, bool isCsv = false);
        Task CreateLargeReportInBackgroundAsync(string jobId, bool isCsv);

        Task<(bool IsReady, byte[]? FileBytes, string? FileName)> GetReportFileAsync(string jobId);

        string EnqueueReport(bool isCsv);
    }
}