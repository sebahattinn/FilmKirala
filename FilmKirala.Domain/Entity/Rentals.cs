using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmKirala.Domain.Entity;

namespace FilmKirala.Domain.Entity
{
    public class Rentals
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int MovieId { get; private set; }
        public int RentalPriceId { get; private set; }
        public DateTime StartRentalDate { get; private set; }
        public DateTime EndRentalDate { get; private set; }
        public int TotalPrice { get; private set; }
        public bool Status { get; private set; } //kiralandı mı kiralanmadı mı zart zurt cart curt

        public Users User { get; private set; }
        public Movies Movie { get; private set; }
        public RentalPricing RentalPricing { get; private set; }


        public void RentalsCreate( DateTime startRentalDate, DateTime endRentalDate, int totalPrice, bool status, Users users,Movies movies, RentalPricing rentalPricing)
        {
            // UserId = userId;
            // MovieId = movieId;
            // RentalPriceId = rentalPriceId;

            if (totalPrice<=0)
            {
                throw new Exception("bedavaya film olmaz ab");
            }

            User = users;
            Movie = movies;
            RentalPricing = rentalPricing;
            StartRentalDate = startRentalDate;
            EndRentalDate = endRentalDate;
            TotalPrice = totalPrice;
            Status = status;
        }



    }
}
