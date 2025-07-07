using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace NPV.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, cannot handle error.");
                throw;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = httpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                    ? ex.ToString()
                    : "An unexpected error occurred.",
                Instance = httpContext.Request.Path,
                Type = "https://httpstatuses.com/500"
            };

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await httpContext.Response.WriteAsync(json);
        }
    }
}
