using Bogus;
using FilmKirala.Domain.Entity;
using FilmKirala.Domain.Enums;
using FilmKirala.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

var connectionString = "Server=.;Database=FilmKiralaDev;Trusted_Connection=True;TrustServerCertificate=True;";

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(connectionString, sqlOptions => {
    sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
    sqlOptions.CommandTimeout(120);
});

using var context = new AppDbContext(optionsBuilder.Options);
context.ChangeTracker.AutoDetectChangesEnabled = false;

Console.WriteLine("🚀 TÜM TABLOLARA EŞ ZAMANLI VERİ BASILIYOR...");
var stopwatch = Stopwatch.StartNew();

// --- AYARLAR ---
int totalTarget = 1_000_000;
int batchSize = 100; // Stabilite için paket boyutunu küçük tutuyoruz
var globalFaker = new Faker("tr");

// --- FAKERS ---
var userFaker = new Faker<User>("tr")
    .CustomInstantiator(f => new User(
        f.Internet.UserName() + f.UniqueIndex, // Username çakışmasın
        $"{f.Internet.UserName().ToLower()}{f.UniqueIndex}{Guid.NewGuid().ToString().Substring(0, 4)}@filmkirala.com", // Email asla çakışmaz
        Convert.ToBase64String(Encoding.UTF8.GetBytes(f.Internet.Password(10))), // dummyHash yerine gerçekçi veri
        Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
        f.Random.Int(100, 50000),
        Roles.User
    ));

var movieFaker = new Faker<Movie>("tr")
    .CustomInstantiator(f => new Movie(
        f.Commerce.ProductName() + " " + f.Random.Int(1, 1000000), // Title çakışmasın diye yazdık bunu da 
        f.Lorem.Sentence(10),
        f.Music.Genre(),
        f.Random.Int(10, 100),
        true
    ));

for (int i = 0; i < totalTarget; i += batchSize)
{
    try
    {
        var users = userFaker.Generate(batchSize);
        var movies = movieFaker.Generate(batchSize);

        // Önce bunları ekleyip ID'lerini alalım
        await context.Users.AddRangeAsync(users);
        await context.Movies.AddRangeAsync(movies);
        await context.SaveChangesAsync(); // Önce ana tabloları basıyoruz

        // Şimdi oluşan ID'lerle Rental ve Review basalım
        foreach (var user in users)
        {
            var randomMovie = movies[globalFaker.Random.Int(0, batchSize - 1)];

            // Fiyat ekle
            randomMovie.AddRentalPricing(DurationType.Günlük, 1, globalFaker.Random.Int(20, 100));
            var pricing = randomMovie.RentalPricings.First();

            // Rental
            var rental = new Rental();
            rental.RentalsCreate(DateTime.UtcNow, DateTime.UtcNow.AddDays(7), pricing.Price, true, user, randomMovie, pricing);
            await context.Rentals.AddAsync(rental);

            // Review
            var review = new Review(user.Id, randomMovie.Id, globalFaker.Lorem.Sentence(5), (Rating)globalFaker.Random.Int(1, 5));
            await context.Reviews.AddAsync(review);
        }

        await context.SaveChangesAsync(); //  İlişkili tabloları basıyoruz
        context.ChangeTracker.Clear();

        Console.Write($"\r[OK] {DateTime.Now:HH:mm:ss} -> Toplam Kayıt Grubu: {i + batchSize:N0}");
    }
    catch (Exception ex)
    {
        // Hata detayını tam gör ki neymiş derdi anlayalım
        Console.WriteLine($"\n❌ HATA: {ex.InnerException?.Message ?? ex.Message}");
        context.ChangeTracker.Clear();
        await Task.Delay(1000);
    }
}

stopwatch.Stop();
Console.WriteLine("\n\n🎉 BİTTİ! Sistem artık gerçek bir canavar.");
Console.ReadKey();