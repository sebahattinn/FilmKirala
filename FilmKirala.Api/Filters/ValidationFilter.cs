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
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

             
                context.Result = new BadRequestObjectResult(new
                {
                    Message = "Validation Error: The data you submitted is not in the expected format.",
                    Errors = errors
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}