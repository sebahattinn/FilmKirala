using System;
using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class NotificationLog
    {
        public int Id { get; private set; }
        public string UserEmail { get; private set; }
        public string Subject { get; private set; }
        public string Message { get; private set; }
        public NotificationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? SentAt { get; private set; }
        public string ErrorMessage { get; private set; }
        public string? Type { get; private set; }
        public int RetryCount { get; private set; }

        protected NotificationLog() { }

        // İlk oluşturma (Constructor) - Genelde Pending olarak başlar
        public NotificationLog(string userEmail, string subject, string message, string? type)
        {
            UserEmail = userEmail;
            Subject = subject;
            Message = message;
            Type = type;
            Status = NotificationStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            ErrorMessage = string.Empty;
            RetryCount = 0;
        }

        // Başarılı gönderim durumunda çağrılır
        public void MarkAsSent()
        {
            Status = NotificationStatus.Sent;
            SentAt = DateTime.UtcNow;
            ErrorMessage = string.Empty;
        }

        // Hata durumunda çağrılır
        public void MarkAsFailed(string errorMessage)
        {
            Status = NotificationStatus.Failed;
            ErrorMessage = errorMessage;
        }

        // Yeniden deneme durumunda çağrılır
        public void MarkAsRetrying()
        {
            Status = NotificationStatus.Retrying;
            RetryCount++;
        }

        // DLQ veya kalıcı hata durumunda çağrılır
        public void MarkAsDeadLetter(string reason)
        {
            Status = NotificationStatus.DeadLetter;
            ErrorMessage = reason;
        }
    }
}