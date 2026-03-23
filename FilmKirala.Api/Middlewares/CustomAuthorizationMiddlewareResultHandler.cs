using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace FilmKirala.Api.Middlewares
{
    /// <summary>
    /// Yetersiz yetki (403) ve kimlik doğrulama eksikliği (401) durumlarında
    /// açıklayıcı JSON mesajı döndürür.
    /// </summary>
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // Kimlik doğrulandı ama yetki yetersiz (örn: Admin rolü yok) → 403
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 403,
                    message = "Bu işlem için Admin yetkisi gereklidir. Hesabınızın bu kaynağa erişim izni bulunmamaktadır."
                });
                return;
            }

            // Token gönderilmemiş veya geçersiz → 401
            if (authorizeResult.Challenged)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 401,
                    message = "Bu işlem için giriş yapmanız gerekmektedir. Lütfen geçerli bir Bearer token gönderin."
                });
                return;
            }

            await next(context);
        }
    }
}
