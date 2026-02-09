using System.Net;
using System.Text.Json;

namespace FilmKirala.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İstek normal yolunda devam etsin
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                // Eğer "Bulunamadı" hatası gelirse burası yakalar (404)
                await HandleExceptionAsync(context, ex, (int)HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                // Başka herhangi bir hata gelirse burası yakalar (500)
                // Loglama (Serilog) buraya eklenebilir.
                await HandleExceptionAsync(context, ex, (int)HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = exception.Message,
                // İstersen buraya "Detail" veya "Timestamp" de ekleyebilirsin
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}