using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool Role { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Users(string username, string email, string passwordHash, string passwordSalt, int walletBalance, bool role)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            WalletBalance = walletBalance;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }

    }
}
