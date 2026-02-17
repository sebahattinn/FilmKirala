using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class User(string username, string email, string passwordHash, string passwordSalt, int walletBalance, Roles roles)
    {
        public int Id { get; private set; }
        public string Username { get; private set; } = username;
        public string Email { get; private set; } = email;
        public string PasswordHash { get; private set; } = passwordHash;
        public string PasswordSalt { get; private set; } = passwordSalt;
        public int WalletBalance { get; private set; } = walletBalance;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Roles Roles { get; private set; } = roles;

    
        private readonly List<UserRefreshToken> _refreshTokens = [];
        public IReadOnlyCollection<UserRefreshToken> RefreshTokens => _refreshTokens;

        public void AddRefreshToken(string token, DateTime expiryTime)
        {
            _refreshTokens.Add(new UserRefreshToken(token, expiryTime, Id));
        }

        public void DecreaseBalance(int amount)
        {
            if (amount < 0) throw new ArgumentException("Düşülecek miktar eksi olamaz.");

            if (WalletBalance < amount)
                throw new InvalidOperationException($"Bakiye yetersiz! Mevcut: {WalletBalance}, Gereken: {amount}");

            WalletBalance -= amount;
        }

        public void UpdateBalance(decimal newBalance)
        {
            if (newBalance < 0) throw new ArgumentException("Bakiye 0'dan küçük olamaz!");
            WalletBalance = (int)newBalance;
        }
    }
}