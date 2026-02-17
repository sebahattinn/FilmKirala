namespace FilmKirala.Notification.Api.Entities
{
    public class NotificationLog
    {
        public int Id { get; set; }
        public required string UserEmail { get; set; }
        public required string Subject { get; set; }
        public required string Message { get; set; }

        public DateTime SentAt { get; set; }
        public bool IsSuccess { get; set; }
    }
}