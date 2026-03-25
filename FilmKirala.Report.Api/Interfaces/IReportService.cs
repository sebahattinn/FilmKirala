namespace FilmKirala.Report.Api.Interfaces
{
    public interface IReportService
    {
   
        Task<string> QueueReportAsync(bool isCsv);

        // Controller: Returns the report file (only if Status=Completed)
        Task<(bool IsReady, byte[]? FileBytes, string? FileName)> GetReportFileAsync(string jobId);

        Task<string> GetJobStatusAsync(string jobId); 

        // Real-time summary and export
        Task<object> GetMovieSummaryAsync(int lastId, int pageSize);
        Task<MemoryStream> ExportMoviesAsync(int? lastId = null, int? pageSize = null, bool isCsv = false);
    }
}