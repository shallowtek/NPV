using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NPV.Shared.Models;

namespace NPV.API.Testing.Integration;

public class CalculatorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CalculatorIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task CalculateEndpoint_ReturnsOk_WithValidRequest()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m, 200m, 300m },
            LowerBound = 1.0m,
            UpperBound = 2.0m,
            Increment = 0.5m
        };

        var response = await _client.PostAsJsonAsync("/api/npv/calculate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<NpvResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.NpvResults);
        Assert.True(result.NpvResults.Any());
    }

    [Fact]
    public async Task CalculateEndpoint_ReturnsBadRequest_WithEmptyCashFlows()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal>(),
            LowerBound = 1.0m,
            UpperBound = 2.0m,
            Increment = 0.5m
        };

        var response = await _client.PostAsJsonAsync("/api/npv/calculate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
