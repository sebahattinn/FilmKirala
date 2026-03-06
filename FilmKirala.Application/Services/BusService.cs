using FilmKirala.Application.Interfaces;
using FilmKirala.Domain.Entity;
using FilmKirala.Shared.Events;
using MassTransit;

namespace FilmKirala.Infrastructure.Services
{
    public class BusService : IBusService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IUnitOfWork _unitOfWork;

        public BusService(IPublishEndpoint publishEndpoint, IUnitOfWork unitOfWork)
        {
            _publishEndpoint = publishEndpoint;
            _unitOfWork = unitOfWork;
        }
        public async Task PublishAsync<T>(T message) where T : class
        {
            string email = "System";
            string subject = "Notification";
            string content = "Message content unknown";

            if (message is FilmRentedEvent rentedEvent)
            {
                email = rentedEvent.Email;
                subject = rentedEvent.Subject;
                content = rentedEvent.Message;
            }

            var log = new NotificationLog(email, subject, content, "Email");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                await _publishEndpoint.Publish(message, cts.Token);

                log.MarkAsSent();
            }
            catch (Exception ex)
            {
                log.MarkAsFailed(ex.Message);
                Console.WriteLine($"[RabbitMQ Error]: Mesaj gönderilemedi. Hata: {ex.Message}");
            }
            finally
            {
                await _unitOfWork.NotificationLogs.AddAsync(log);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}