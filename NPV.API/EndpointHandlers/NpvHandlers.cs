using FluentValidation;
using NPV.Shared.Models;
using NPV.Shared.Interfaces;

namespace NPV.API.EndpointHandlers;

public static class NpvHandlers
{
    public static async Task<IResult> CalculateAsync(
        NpvRequest? request,
        INpvCalculator calculator,
        IValidator<NpvRequest> validator)
    {
        if (request is null)
        {
            var errors = new[]
            {
                new { PropertyName = "Request", ErrorMessage = "Request cannot be null." }
            };
            return Results.BadRequest(errors);
        }

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            return Results.BadRequest(errors);
        }

        var response = calculator.Calculate(request);
        return Results.Ok(response);
    }
}
