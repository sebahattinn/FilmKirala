using MassTransit;
using FilmKirala.Notification.Api.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace FilmKirala.Notification.Api.Extensions
{
    public static class ServiceRegistration
    {
        public static void AddMassTransitRegistration(this IServiceCollection services)
        {
            services.AddMassTransit(x =>   
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