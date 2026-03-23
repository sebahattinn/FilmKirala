using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmKirala.Domain.Enums
{
    /// <summary>
    /// Kiralama süresinin birimi. POST /api/Movies ve POST /api/Rentals body'sinde
    /// <c>durationType</c> alanına bu değerlerden biri gönderilmelidir.
    /// </summary>
    public enum DurationType
    {
        /// <summary>Saatlik kiralama (değer: 1)</summary>
        Saatlik = 1,
        /// <summary>Günlük kiralama (değer: 2)</summary>
        Günlük = 2,
        /// <summary>Haftalık kiralama (değer: 3)</summary>
        Haftalık = 3,
        /// <summary>Aylık kiralama (değer: 4)</summary>
        Aylık = 4,
        /// <summary>Yıllık kiralama (değer: 5)</summary>
        Yıllık = 5
    }
}
