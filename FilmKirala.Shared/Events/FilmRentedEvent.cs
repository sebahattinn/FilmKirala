namespace FilmKirala.Shared.Events
{
 
    public record FilmRentedEvent
    {
        public required string Email { get; init; }
        public required string Subject { get; init; }
        public required string Message { get; init; }
    }
}