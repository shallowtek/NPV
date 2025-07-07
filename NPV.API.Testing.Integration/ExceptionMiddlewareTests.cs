using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NPV.API.Testing.Integration;

public class ExceptionMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExceptionMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExceptionMiddleware_Returns_ProblemDetails_On_Unhandled_Exception()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/test-exception");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var problem = JsonSerializer.Deserialize<ProblemDetailsDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problem);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Equal(500, problem.Status);
        Assert.Equal("/api/test-exception", problem.Instance);
    }

    public class ProblemDetailsDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int? Status { get; set; }
        public string Detail { get; set; }
        public string Instance { get; set; }
    }
}
