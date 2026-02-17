using FilmKirala.Notification.Api.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using FilmKirala.Notification.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<NotificationAppDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));      // SQL Server Bağlantısı

builder.Services.AddMassTransit(x =>

{
    // Consumer Tanımı

    x.AddConsumer<FilmRentedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");

            h.Password("guest");

        });
        //  ANA KUYRUK YAPILANDIRMASI (MAIN QUEUE)
        cfg.ReceiveEndpoint("film_kiralama_kuyruğu", e =>

        {
            // RETRY: Hata durumunda 3 kez yerinde dene.
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.SetQueueArgument("x-dead-letter-exchange", "film-rented-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "film-rented-dead-letter-key");  //DLX konfigürasyonları
            e.ConfigureConsumer<FilmRentedConsumer>(context);      // Consumer'ı bağla
        });
        cfg.ReceiveEndpoint("film_kiralama_dead_letter_kuyruğu", e =>        //DLQ yapılandırması işlenen veya hataya düşen mesajalr buraya

        {
            e.Bind("film-rented-dlx", s =>
            {
                s.RoutingKey = "film-rented-dead-letter-key";
                s.ExchangeType = "direct";
            });

        });

    });

});
var app = builder.Build();
if (app.Environment.IsDevelopment())

{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();