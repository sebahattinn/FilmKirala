using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.OpenApi.Models;

// Kendi Namespace'lerin (Bunlarý eklemeyi unutma)
using FilmKirala.Infrastructure.Persistence;
using FilmKirala.Application.Interfaces.Repositories;
using FilmKirala.Infrastructure.Repositories;
using FilmKirala.Application.Interfaces;
using FilmKirala.Application.Interfaces.Services;
using FilmKirala.Application.Services;
using FilmKirala.Application.Mappings; // AutoMapper profili buradaysa

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Ayarý
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

var configuration = builder.Configuration;

#region SERVICES

builder.Services.AddControllers();

// 2. Database Baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// 3. AutoMapper (En saðlam yöntem assembly belirtmektir)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly); // MappingProfile hangi katmandaysa onun assembly'sini tarar

// 4. FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Validatorlar API katmanýndaysa Program yeterli, Application'daysa oradan bir class ver.

// ============================================================
// ?? EKSÝK OLAN KISIM: DEPENDENCY INJECTION (DI) ??
// ============================================================

// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMovieService, MovieService>();

builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// ============================================================

// 5. JWT Authentication
var jwtSection = configuration.GetSection("JwtSettings"); // appsettings.json'da bu alanýn olduðundan emin ol!
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

// 6. Swagger Config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FilmKirala API", Version = "v1" });

    // JWT Kilidi Ekleme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

#endregion

var app = builder.Build();

#region PIPELINE

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sýralama Önemli: Önce Kimlik, Sonra Yetki
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();