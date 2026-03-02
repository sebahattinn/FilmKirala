using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Report.Api.Interfaces;
using FilmKirala.Report.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1. MiniProfiler Servislerini Kaydet
builder.Services.AddMemoryCache(); // Gerekli
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler"; // Sonuçlar buradan izlenecek
}).AddEntityFramework(); // EF Core sorgularýný yakalar

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(connectionString),
    poolSize: 1024);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHostedService<ReportBackgroundWorker>();

var app = builder.Build();

// 2. Middleware'i Ekle
app.UseMiniProfiler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();