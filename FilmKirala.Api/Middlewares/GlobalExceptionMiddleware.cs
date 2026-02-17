using System.Net;
using System.Text.Json;
using Serilog;

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
                await _next(context);
            }
            catch (Exception ex)
            {
                // Loglama burada merkezileşir
                Log.Error(ex, "Bir hata oluştu: {Message}", ex.Message);

                var statusCode = ex switch
                {
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    InvalidOperationException => (int)HttpStatusCode.BadRequest,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                await HandleExceptionAsync(context, ex, statusCode);
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
                // buraya "Detail" veya "Timestamp" de eklenebilir.
            };
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}