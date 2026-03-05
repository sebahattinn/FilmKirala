using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class Movie
    {
        public int Id { get; private set; }

        // ya constructor ya da EF Core tarafından dolacağını garanti ediyoruz.
        public string Title { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string Genre { get; private set; } = null!;

        public int Stock { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<RentalPricing> _rentalPricings = [];
        private readonly List<Review> _reviews = [];

        public IReadOnlyCollection<Review> Reviews => _reviews;
        public IReadOnlyCollection<RentalPricing> RentalPricings => _rentalPricings;

        private Movie() { } // EF Core için

        public Movie(string title, string description, string genre, int stock, bool isActive)
        {
            if (stock < 0)
            {
                throw new ArgumentException("Kiralanacak film eksi olamaz");
            }

            if (string.IsNullOrWhiteSpace(title) || title.Length >= 300)
            {
                throw new ArgumentException("Filmin adı boş olamaz ve 300 karakterden az olmalı");
            }

            Title = title;
            Description = description;
            Genre = genre;
            Stock = stock;
            IsActive = isActive;
        }

        public void AddRentalPricing(DurationType type, int durationValue, int price)
        {
            var pricing = new RentalPricing(type, durationValue, price, this);
            _rentalPricings.Add(pricing);
        }

        public void DecreaseStock()
        {
            if (Stock <= 0)
            {
                throw new InvalidOperationException($"'{Title}' filmi için stok kalmadı!");
            }
            Stock--;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Miktar sıfırdan büyük olmalı.");
            if (Stock < quantity)
                throw new InvalidOperationException($"'{Title}' stok yetersiz! Mevcut: {Stock}, İstenen: {quantity}");
            Stock -= quantity;
        }

        public void IncreaseStock()
        {
            Stock++;
        }
    }
}