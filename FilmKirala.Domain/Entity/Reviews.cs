using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class Reviews
    {
        public int Id { get; private set; }
      
        public Users? Users { get; private set; }
        public Movies? Movies { get; private set; }
        public RentalPricing? RentalPricing { get; private set; }    
        public Rating rating { get; private set; }
        public string? Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }


        private readonly List<RentalPricing> _rentalPricings = new();
        private readonly List<Users> _users = new();
        private readonly List<Movies> _movies = new();
        public IReadOnlyCollection<RentalPricing> RentalPricings => _rentalPricings;
        public IReadOnlyCollection<Users> Userss => _users;
        public IReadOnlyCollection<Movies> Moviess => _movies;
        public Reviews(Users users, Movies movies, RentalPricing rental, string comment)
        {
            Users = users;
            Movies = movies;
            RentalPricing = rental;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
        }




    }
}
