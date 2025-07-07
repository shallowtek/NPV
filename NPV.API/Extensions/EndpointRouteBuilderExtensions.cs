using NPV.API.EndpointHandlers;

namespace NPV.API.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterNpvEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var npvEndpoints = endpointRouteBuilder.MapGroup("api/npv/calculate")
            .RequireAuthorization();

        npvEndpoints.MapPost("", NpvHandler.CalculateAsync)

            .WithName("Calculate")
            .WithOpenApi();
    }

    public static void RegisterAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/register", AuthHandler.RegisterAsync)
            .WithName("Register")
            .WithOpenApi();

        endpoints.MapPost("/api/login", AuthHandler.LoginAsync)
            .WithName("Login")
            .WithOpenApi();
    }
}
