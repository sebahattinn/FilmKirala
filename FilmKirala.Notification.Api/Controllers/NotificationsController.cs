using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmKirala.Notification.Api.Data;

namespace FilmKirala.Notification.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationAppDbContext _context;

        public NotificationsController(NotificationAppDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetReportNotifications()
        {
            var notifications = await _context.NotificationLogs
                .AsNoTracking()
                .Where(x => x.Type == "ReportReady")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }
    }
}