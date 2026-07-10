using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Extensions;

public static class ProblemExtensions
{
    public static IResult ToProblem(List<Error> errors)
    {
        if (!errors.Any())
        {
            return Results.Problem();
        }

        if (errors.All(e => e.Type == ErrorKind.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors.First());
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        var validationErrors = errors.ToDictionary(e => e.Code, e => new[] { e.Description });

        var problemDetails = new ValidationProblemDetails(validationErrors)
        {
            Status = StatusCodes.Status400BadRequest,
        };
        return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
    }
}
