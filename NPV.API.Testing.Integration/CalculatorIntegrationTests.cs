using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NPV.Shared.Models;
using Xunit;

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

    // Helper DTO for login response
    private class LoginResultDto
    {
        public string token { get; set; }
    }

    // Helper to register/login and get a JWT token
    private async Task<string> GetJwtTokenAsync()
    {
        // Register user (ignore error if user already exists)
        var registerResponse = await _client.PostAsJsonAsync("/api/register", new
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Test@12345"
        });

        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            Username = "testuser",
            Password = "Test@12345"
        });

        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {(int)loginResponse.StatusCode} {loginResponse.ReasonPhrase}\n{body}");
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResultDto>();
        return loginResult?.token ?? throw new Exception("Token not found in response");
    }


    [Fact]
    public async Task CalculateEndpoint_ReturnsOk_WithValidRequest()
    {
        var token = await GetJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
        var token = await GetJwtTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

    [Fact]
    public async Task CalculateEndpoint_ReturnsUnauthorized_IfNoToken()
    {
        // Remove any existing auth header
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m, 200m, 300m },
            LowerBound = 1.0m,
            UpperBound = 2.0m,
            Increment = 0.5m
        };

        var response = await _client.PostAsJsonAsync("/api/npv/calculate", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
