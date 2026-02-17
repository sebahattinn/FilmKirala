using FilmKirala.Notification.Api.Data;
using FilmKirala.Notification.Api.Entities;
using FilmKirala.Shared.Events;
using MassTransit;
using FilmKirala.Infrastructure;

namespace FilmKirala.Notification.Api.Consumers
{
    public class FilmRentedConsumer : IConsumer<FilmRentedEvent>
    {
        private readonly NotificationAppDbContext _context;
        private readonly ILogger<FilmRentedConsumer> _logger;

        public FilmRentedConsumer(NotificationAppDbContext context, ILogger<FilmRentedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FilmRentedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("Kiralama bildirimi işleniyor: {Email}", message.Email);

            try
            {
                var notificationLog = new NotificationLog
                {
                    UserEmail = message.Email,
                    Subject = message.Subject,
                    Message = message.Message,
                    SentAt = DateTime.UtcNow,
                    IsSuccess = true
                };

                await _context.NotificationLogs.AddAsync(notificationLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bildirim başarıyla kaydedildi: {Email}", message.Email);
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Bildirim kaydedilirken hata oluştu: {Email}", message.Email);
                throw;
            }
        }
    }
}