namespace FilmKirala.Report.Api.Interfaces
{
    public interface IReportService
    {
        // Kullanıcı rapor istediğinde DB'ye Pending kaydı atar, JobId döner
        Task<string> EnqueueReportAsync(bool isCsv);

        // BackgroundWorker tarafından çağrılır: asıl Excel/CSV üretim işi
        Task CreateLargeReportInBackgroundAsync(string jobId, bool isCsv);

        // Controller: rapor dosyasını döner (sadece Status=Completed ise)
        Task<(bool IsReady, byte[]? FileBytes, string? FileName)> GetReportFileAsync(string jobId);

        // Controller: DB'deki iş durumunu string olarak döner
        Task<string> GetJobStatusAsync(string jobId);

        // Anlık özet ve export
        Task<object> GetMovieSummaryAsync(int lastId, int pageSize);
        Task<MemoryStream> ExportMoviesAsync(int? lastId = null, int? pageSize = null, bool isCsv = false);
    }
}