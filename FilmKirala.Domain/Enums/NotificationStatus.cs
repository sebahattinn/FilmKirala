using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmKirala.Domain.Enums
{
    public enum NotificationStatus
    {
        Pending = 0,      // Sırada, henüz işlem görmedi
        Processing = 1,   // İşleniyor (Rabbite gönderilmeye çalışılıyor)
        Sent = 2,         // Başarıyla gönderildi
        Failed = 3,       // Hata alındı (try-catch'e düştü)
        Retrying = 4,     // Yeniden deneniyor
        DeadLetter = 5    // DLQ'ya düştü, artık sistem pes etti
    }
}
