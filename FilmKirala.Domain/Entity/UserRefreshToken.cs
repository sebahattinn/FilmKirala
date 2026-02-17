namespace FilmKirala.Domain.Entity
{
    public class UserRefreshToken
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }

        public string Token { get; private set; } = string.Empty;

        public DateTime ExpiryTime { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsRevoked { get; private set; }

        public User User { get; private set; } = null!;

        private UserRefreshToken() { } // EF Core için

        public UserRefreshToken(string token, DateTime expiryTime, int userId)
        {
            Token = token;
            ExpiryTime = expiryTime;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public void Revoke() => IsRevoked = true;

        public bool IsExpired => DateTime.UtcNow >= ExpiryTime;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}