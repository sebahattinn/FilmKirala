namespace FilmKirala.Application.Interfaces
{
    public interface IBusService
    {
       
        Task PublishAsync<T>(T message) where T : class;
    }
}