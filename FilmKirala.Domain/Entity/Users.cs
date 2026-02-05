using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class Users
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public int WalletBalance { get; private set; }
        //public bool Role { get; private set; }           bunun yerine roles enum'unu açtım.
        public DateTime CreatedAt { get; private set; }
        public Roles Roles { get; private set; }

        public Users(string username, string email, string passwordHash, string passwordSalt, int walletBalance,Roles roles )
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            WalletBalance = walletBalance;
            Roles = roles;
            CreatedAt = DateTime.UtcNow;
        }


    }
}
