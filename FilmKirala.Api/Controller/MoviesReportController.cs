using FilmKirala.Report.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FilmKirala.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public MoviesReportController(IReportService reportService) => _reportService = reportService;

        [HttpPost("export-background")]
        public async Task<IActionResult> ExportBackground([FromQuery] bool isCsv = false)
        {
            var jobId = await _reportService.EnqueueReportAsync(isCsv);
            return Accepted(new { Status = "Hazırlanıyor", JobId = jobId, CheckStatusUrl = $"/api/MoviesReport/check-status/{jobId}" });
        }

        [HttpGet("download/{jobId}")]
        public async Task<IActionResult> DownloadReport(string jobId)
        {
            var (isReady, fileBytes, fileName) = await _reportService.GetReportFileAsync(jobId);

            if (!isReady) return NotFound("Rapor henüz hazır değil.");

            string contentType = fileName!.EndsWith(".csv") ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(fileBytes!, contentType, fileName);
        }

        [HttpGet("check-status/{jobId}")]
        public async Task<IActionResult> CheckStatus(string jobId)
        {
            var status = await _reportService.GetJobStatusAsync(jobId);

            return status switch
            {
                "Completed" => Ok(new { Status = "Completed", Message = "Rapor hazır!", DownloadUrl = $"/api/MoviesReport/download/{jobId}" }),
                "Failed" => Ok(new { Status = "Failed", Message = "Rapor oluşturulurken bir hata oluştu." }),
                "NotFound" => NotFound(new { Message = "Bu JobId ile bir rapor bulunamadı." }),
                _ => Ok(new { Status = status, Message = "Rapor hazırlanıyor, lütfen bekleyin..." })
            };
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMovieSummary([FromQuery] int lastId = 0, [FromQuery] int pageSize = 10)
        {
            return Ok(await _reportService.GetMovieSummaryAsync(lastId, pageSize));
        }

        [HttpGet("export-instant")]
        public async Task<IActionResult> Export([FromQuery] int? lastId = null, [FromQuery] int? pageSize = null, [FromQuery] bool? isCsv = false)
        {
            bool csvRequest = isCsv.GetValueOrDefault();
            var stream = await _reportService.ExportMoviesAsync(lastId, pageSize, csvRequest);
            return File(stream, csvRequest ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Film_Raporu_{DateTime.Now:yyyyMMdd}.{(csvRequest ? "csv" : "xlsx")}");
        }
    }
}
