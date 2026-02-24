using FilmKirala.Report.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FilmKirala.Report.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public MoviesReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMovieSummary([FromQuery] int lastId = 0, [FromQuery] int pageSize = 10)
        {
            var result = await _reportService.GetMovieSummaryAsync(lastId, pageSize);
            return Ok(result);
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] int? lastId = null, [FromQuery] int? pageSize = null)
        {
            var stream = await _reportService.ExportMoviesToExcelAsync(lastId, pageSize);

        
            string suffix = pageSize.HasValue ? $"_top{pageSize}" : "_all";
            string fileName = $"Film_Raporu_{DateTime.Now:yyyyMMdd}{suffix}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}