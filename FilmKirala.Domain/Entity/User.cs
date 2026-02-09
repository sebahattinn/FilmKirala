using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string PasswordSalt { get; private set; }
        public int WalletBalance { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Roles Roles { get; private set; }

        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiryTime { get; private set; }

        public User(string username, string email, string passwordHash, string passwordSalt, int walletBalance, Roles roles)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            WalletBalance = walletBalance;
            Roles = roles;
            CreatedAt = DateTime.UtcNow;
        }

        public void DecreaseBalance(int amount)
        {
            if (amount < 0) throw new Exception("Düşülecek miktar eksi olamaz.");
            if (WalletBalance < amount) throw new Exception($"Bakiye yetersiz! Mevcut: {WalletBalance}, Gereken: {amount}");

            WalletBalance -= amount;
        }

        public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpiryTime = expiryTime;
        }
    }
}