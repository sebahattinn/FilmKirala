namespace FilmKirala.Application.DTOs
{
    public class UpdateBalanceDto
    {
        public required string Email { get; set; }
        public decimal NewBalance { get; set; }
    }
}