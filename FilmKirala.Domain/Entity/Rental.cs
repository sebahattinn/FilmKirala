using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Domain.Entity
{
    public class Rental
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int MovieId { get; private set; }

        public int RentalPricingId { get; private set; }

        public DateTime StartRentalDate { get; private set; }
        public DateTime EndRentalDate { get; private set; }
        public int TotalPrice { get; private set; }
        public bool Status { get; private set; }

        public User? User { get; private set; }
        public Movie? Movie { get; private set; }
        public RentalPricing? RentalPricing { get; private set; }

        public void RentalsCreate(DateTime startRentalDate, DateTime endRentalDate, int totalPrice, bool status, User user, Movie movie, RentalPricing rentalPricing)
        {
            if (totalPrice <= 0)
            {
                throw new Exception("Bedavaya film olmaz ab");
            }

            // İlişkileri kuruyorum
            User = user;
            UserId = user.Id; // ID'yi garantiye alıyoruz

            Movie = movie;
            MovieId = movie.Id;

            RentalPricing = rentalPricing;
            RentalPricingId = rentalPricing.Id; 

            StartRentalDate = startRentalDate;
            EndRentalDate = endRentalDate;
            TotalPrice = totalPrice;
            Status = status;
        }
    }
}