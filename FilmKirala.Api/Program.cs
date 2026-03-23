using System.Text;
using System.Threading.RateLimiting;
using FilmKirala.Api.BackgroundServices;
using FilmKirala.Api.Filters;
using FilmKirala.Api.HealthChecks;
using FilmKirala.Api.Middlewares;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Application.Mappings;
using FilmKirala.Application.Services;
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Infrastructure.Repositories;
using FilmKirala.Infrastructure.Services;
using FilmKirala.Report.Api.Interfaces;
using FilmKirala.Report.Api.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Graylog;

var builder = WebApplication.CreateBuilder(args);

// MessagePack Konfigürasyonu
var mcOptions = MessagePackSerializerOptions.Standard
    .WithResolver(CompositeResolver.Create(
        NativeGuidResolver.Instance,
        NativeDecimalResolver.Instance,
        StandardResolver.Instance
    ));
MessagePackSerializer.DefaultOptions = mcOptions;

#region LOGGING
builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FilmKirala.Api")
    .WriteTo.Console()
    .WriteTo.Graylog(new GraylogSinkOptions
    {
        HostnameOrAddress = "localhost",
        Port = 12201,
        TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp
    })
    .CreateLogger();
builder.Host.UseSerilog();
#endregion

var configuration = builder.Configuration;

#region SERVICES

// Redis Cache Yapılandırması
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379,connectTimeout=3000,syncTimeout=200,responseTimeout=200,abortConnect=false";
    options.InstanceName = "FilmKirala_";
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Veritabanı Bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

// Repository ve UnitOfWork Kayıtları
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRentalPricingRepository, RentalPricingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Uygulama Servisleri (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IBusService, BusService>();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MappingProfile>();

//  MassTransit ve RabbitMQ (Producer Yapılandırması)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// JWT Kimlik Doğrulama
var jwtSection = configuration.GetSection("JwtSettings");
var jwtKey = jwtSection["Key"] ?? throw new Exception("JwtSettings:Key not found!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// Swagger Yapılandırması
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FilmKirala API", Version = "v1" });

    // XML doc yorumlarını Swagger'a dahil et (controller + DTO açıklamaları)
    var apiXml    = Path.Combine(AppContext.BaseDirectory, "FilmKirala.Api.xml");
    var appXml    = Path.Combine(AppContext.BaseDirectory, "FilmKirala.Application.xml");
    var domainXml = Path.Combine(AppContext.BaseDirectory, "FilmKirala.Domain.xml");
    if (File.Exists(apiXml))    options.IncludeXmlComments(apiXml);
    if (File.Exists(appXml))    options.IncludeXmlComments(appXml);
    if (File.Exists(domainXml)) options.IncludeXmlComments(domainXml);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// MiniProfiler — Raporlama sorgularını profillemek için
builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(30)));
});
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
}).AddEntityFramework();

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHostedService<ReportBackgroundWorker>();        //worker bundan alt alta daha eklersem worker sayısı da artar ab
builder.Services.AddHostedService<RentalExpirationBackgroundWorker>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("sqlserver");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", config =>
    {
        config.PermitLimit = 20;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5000,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

#endregion

var app = builder.Build();

// Warm up the database connection pool and SQL Server query plans at startup
// so the first real user request is never slow.
_ = Task.Run(async () =>
{
    await Task.Delay(2000); // let background workers settle first
    try
    {
        using var scope = app.Services.CreateScope();
        var movieService = scope.ServiceProvider.GetRequiredService<IMovieService>();
        // Execute the full GetMovieWithDetailsAsync query (QueryMultiple) so SQL Server
        // compiles and caches all three query plans before any user hits the endpoint.
        await movieService.GetMovieByIdAsync(1);
    }
    catch
    {
        // Warmup failure is non-fatal — app continues normally.
    }
});

#region PIPELINE
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

// 401 / 403 yanıtlarına açıklayıcı JSON mesajı ekle
app.UseStatusCodePages(async ctx =>
{
    var response = ctx.HttpContext.Response;
    response.ContentType = "application/json";
    var body = response.StatusCode switch
    {
        401 => new { statusCode = 401, message = "Giriş yapmanız gerekmektedir." },
        403 => new { statusCode = 403, message = "Admin yetkisi gereklidir, bu işlemi yapamazsınız." },
        _   => new { statusCode = response.StatusCode, message = "Bir hata oluştu." }
    };
    await response.WriteAsJsonAsync(body);
});

app.UseMiniProfiler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseOutputCache();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
#endregion
await app.RunAsync();
