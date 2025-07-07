using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using NPV.Shared.Models;
using NPV.Shared.Interfaces;

namespace NPV.API.EndpointHandlers;

public static class NpvHandler
{
    public static async Task<IResult> CalculateAsync(
        NpvRequest? request,
        INpvCalculator calculator,
        IValidator<NpvRequest> validator,
        HttpContext httpContext)
    {
        if (request is null)
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Request", new[] { "Request cannot be null." } }
            };

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation error.",
                Instance = httpContext.Request.Path
            };

            return Results.Json(problem, statusCode: 400);
        }

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation error.",
                Instance = httpContext.Request.Path
            };

            return Results.Json(problem, statusCode: 400);
        }

        var response = calculator.Calculate(request);
        return Results.Ok(response);
    }
}
