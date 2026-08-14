using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Infrastructure;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Title = "An unexpected error occurred.",
                    Detail = environment.IsDevelopment() ? exception.Message : null,
                },
            });
    }
}
