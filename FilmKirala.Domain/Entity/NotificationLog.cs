using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmKirala.Domain.Entity
{
    public class NotificationLog
    {
        public int Id { get; private set; }
        public string UserEmail { get; private set; }
        public string Subject { get; private set; }
        public string Message { get; private set; }
        public bool IsSent { get; private set; } 
        public DateTime CreatedAt { get; private set; }
        public DateTime? SentAt { get; private set; } 
        public string ErrorMessage { get; private set; }

        public string? Type { get; private set; }

        protected NotificationLog() { }
        public NotificationLog(string userEmail,string subject, string message, bool isSent, DateTime createdAt, DateTime sentAt, string errorMessage, string type)
        {
            UserEmail = userEmail;
            Subject = subject;
            Message = message;
            IsSent = isSent;
            CreatedAt = createdAt;
            SentAt = sentAt;
            ErrorMessage = errorMessage;
            Type = type;
        }
    }
}