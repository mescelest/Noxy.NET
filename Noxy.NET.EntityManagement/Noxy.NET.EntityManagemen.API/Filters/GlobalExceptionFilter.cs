using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Noxy.NET.EntityManagement.API.Filters;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        Exception exception = context.Exception;

        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        ObjectResult result = exception switch
        {
            UnauthorizedAccessException => new(new { error = exception.Message })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            },
            ArgumentNullException argEx => new(new { error = $"Invalid input: {argEx.ParamName}" })
            {
                StatusCode = StatusCodes.Status400BadRequest
            },
            InvalidOperationException => new(new { error = "Resource not found" })
            {
                StatusCode = StatusCodes.Status404NotFound
            },
            KeyNotFoundException => new(new { error = "Resource not found" })
            {
                StatusCode = StatusCodes.Status404NotFound
            },
            _ => new(new { error = "An internal server error occurred" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };

        context.Result = result;
        context.ExceptionHandled = true;
    }
}
