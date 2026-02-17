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
            
            try { await _publishEndpoint.Publish(message); }
            catch { /* Log atılabilir */ }
        }
    }
}