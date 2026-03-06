using FilmKirala.Application.Interfaces;
using MassTransit;

namespace FilmKirala.Infrastructure.Services
{
    public class BusService : IBusService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public BusService(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

        public async Task PublishAsync<T>(T message) where T : class
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                await _publishEndpoint.Publish(message, cts.Token);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"[RabbitMQ Error]: Mesaj gönderilemedi. Hata: {ex.Message}");
            }
        }
    }
}