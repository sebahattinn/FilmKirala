using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class RentalPricing
    {
        public int Id { get; private set; }
        public int MovieId { get; private set; }
        public DurationType DurationType { get; private set; }
        public int DurationValue { get; private set; }
        public int Price { get; private set; }

        public Movie Movie { get; private set; } = null!;

        private RentalPricing() { } // EF Core için

        public RentalPricing(DurationType durationType, int durationValue, int price, Movie movie)
        {
            if (price < 0)
            {
                throw new ArgumentException("Fiyat eksi olamaz");
            }
            if (durationValue <= 0)
            {
                throw new ArgumentException("Süre değeri sıfır veya eksi olamaz");
            }

            Movie = movie;
            DurationType = durationType;
            DurationValue = durationValue;
            Price = price;
        }
    }
}