using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FilmKirala.Api.Filters
{
    public class ValidationFilter : IActionFilter
    {
       
        public void OnActionExecuting(ActionExecutingContext context)
        {
            
            if (!context.ModelState.IsValid)
            {
                // Tüm hataları topla
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                var firstError = errors.FirstOrDefault();

               
                context.Result = new BadRequestObjectResult(new
                {
                    error = firstError
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}