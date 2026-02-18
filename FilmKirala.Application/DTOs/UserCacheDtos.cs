using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmKirala.Application.DTOs
{
    public record UserCacheDtos(
        int Id,
        string Username,
        string Email,
        int WalletBalance,
        string Role
    );
}
