using FilmKirala.Report.Api.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FilmKirala.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService) => _reportService = reportService;

        [HttpPost("queue")]
        public async Task<IActionResult> QueueReport([FromQuery] bool isCsv = false)
        {
            var jobId = await _reportService.QueueReportAsync(isCsv);
            return Accepted(new { Status = "Queued", JobId = jobId, StatusUrl = $"/api/Reports/{jobId}/status" });
        }
        [HttpGet("{jobId}/download")] 
        public async Task<IActionResult> DownloadReport(string jobId)
        {
            var (isReady, fileBytes, fileName) = await _reportService.GetReportFileAsync(jobId);

            if (!isReady)
            {
                // First, check if such a record exists in the DB (check the service for status control)
                var status = await _reportService.GetJobStatusAsync(jobId);
                if (status == "NotFound") return NotFound(new { message = "Böyle bir rapor talebi hiç oluþmamýþ." });

                return Accepted(new { message = "Rapor henüz hazýrlanýyor, lütfen bekleyin.", status = status });
            }

            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound(new { message = "Rapor dosyasý as a physical not founded." });

            string contentType = fileName!.EndsWith(".csv") ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(fileBytes, contentType, fileName);
        }

        [HttpGet("{jobId}/status")]
        public async Task<IActionResult> GetJobStatus(string jobId)
        {
            var status = await _reportService.GetJobStatusAsync(jobId);

            return status switch
            {
                "Completed" => Ok(new { Status = "Completed", Message = "Report is ready.", DownloadUrl = $"/api/Reports/{jobId}/download" }),
                "Failed"    => Ok(new { Status = "Failed",    Message = "Report generation failed." }),
                "NotFound"  => NotFound(new { Message = "No report found for the given job ID." }),
                _           => Ok(new { Status = status,      Message = "Report is being generated, please check back later." })
            
            };
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMovieSummary([FromQuery] int lastId = 0, [FromQuery] int pageSize = 10)
        {
            return Ok(await _reportService.GetMovieSummaryAsync(lastId, pageSize));
        }

        [HttpGet("export")]  
        public async Task<IActionResult> ExportInstant([FromQuery] int? lastId = null, [FromQuery] int? pageSize = null, [FromQuery] bool isCsv = false)
        {
            var stream = await _reportService.ExportMoviesAsync(lastId, pageSize, isCsv);
            string contentType = isCsv ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"movies_report_{DateTime.UtcNow:yyyyMMdd}.{(isCsv ? "csv" : "xlsx")}";
            return File(stream, contentType, fileName);
        }
    }
}
