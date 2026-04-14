using System.Net;
using System.Text.Json;
using FilmKirala.Domain.Exceptions;
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
                // Central Logging is here
                Log.Error(ex, "An error occurred: {Message}", ex.Message);

                if (ex is PasswordChangeRequiredException passwordEx)
                {
                    await HandlePasswordChangeRequiredAsync(context, passwordEx);            //Password Change part of this here
                    return;
                }

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
       
        private static Task HandlePasswordChangeRequiredAsync(HttpContext context, PasswordChangeRequiredException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var response = new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Code = exception.Code,
                Message = exception.Message
            };

            return context.Response.WriteAsJsonAsync(response);
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = exception.Message, // İşte burası bizim servis mesajını basacak yer
                Detailed = exception.InnerException?.Message // Varsa iç hatayı da görelim
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}