using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class Movie
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Genre { get; private set; }
        public int Stock { get; private set; }
        public bool IsActive { get; private set; }
        private Movie() { }
        private readonly List<RentalPricing> _rentalPricings = new();

        private readonly List<Review> _reviews = new();
        public IReadOnlyCollection<Review> Reviews => _reviews;
        public IReadOnlyCollection<RentalPricing> RentalPricings => _rentalPricings;
        public Movie(string title, string description, string genre, int stock, bool isActive)
        {

            if (stock < 0)
            {
                throw new ArgumentException("Kiralanacak film eksi olamaz"); //ben dşrekt exception kullanıyodum da pek sağlıklı değilmiş.
            }

            if (string.IsNullOrWhiteSpace(title) || title.Length >= 300)
            {
                throw new ArgumentException("filmin adı boş olamaz ve 300 karakterden az olmalı");

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
                throw new Exception($"'{Title}' filmi için stok kalmadı!");
            }
            Stock--;
        }
        public void IncreaseStock()
        {
            Stock++;
        }
    }
}