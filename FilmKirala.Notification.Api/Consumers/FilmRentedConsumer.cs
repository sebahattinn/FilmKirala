using FilmKirala.Domain.Entity;
using FilmKirala.Notification.Api.Data;
using FilmKirala.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FilmKirala.Notification.Api.Consumers
{
    public class FilmRentedConsumer : IConsumer<FilmRentedEvent>
    {
        private readonly NotificationAppDbContext _dbContext;
        private readonly ILogger<FilmRentedConsumer> _logger;

        public FilmRentedConsumer(NotificationAppDbContext dbContext, ILogger<FilmRentedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FilmRentedEvent> context)
        {
            _logger.LogInformation("Kiralama bildirimi işleniyor: {Email}", context.Message.Email);

            try
            {
                
                var notificationLog = new NotificationLog(
                    userEmail: context.Message.Email,
                    subject: context.Message.Subject,
                    message: context.Message.Message,
                    isSent: true,
                    createdAt: DateTime.UtcNow,
                    sentAt: DateTime.UtcNow,
                    errorMessage: string.Empty, 
                    type: "FilmRental" 
                );

                await _dbContext.NotificationLogs.AddAsync(notificationLog);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Bildirim başarıyla kaydedildi: {Email}", context.Message.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim kaydedilirken hata oluştu: {Email}", context.Message.Email);
                throw;
            }
        }
    }
}