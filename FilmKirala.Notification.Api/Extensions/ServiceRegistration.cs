using MassTransit;
using FilmKirala.Notification.Api.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace FilmKirala.Notification.Api.Extensions
{
    public static class ServiceRegistration
    {
        public static void AddMassTransitRegistration(this IServiceCollection services)
        {
            services.AddMassTransit(x =>   //MassTransit’i ve consumer’ları DI container’a ve RabbitMQ’ya kaydeder.Consumerlar bu sayede event dinleyebilir.
            {
               
                x.AddConsumer<FilmRentedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h => {
                        h.Username("guest");
                        h.Password("guest");
                    });

                   
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}