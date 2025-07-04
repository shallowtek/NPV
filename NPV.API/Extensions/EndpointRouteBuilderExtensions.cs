using NPV.API.EndpointHandlers;

namespace NPV.API.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void RegisterNpvEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var npvEndpoints = endpointRouteBuilder.MapGroup("api/npv/calculate");
        npvEndpoints.MapPost("", NpvHandlers.CalculateAsync)
            .WithName("Calculate")
            .WithOpenApi();
    }
}
