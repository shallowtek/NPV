using FluentValidation;
using NPV.API.Extensions;
using NPV.API.Services;
using NPV.Shared.Interfaces;
using NPV.Shared.Validators;
using NPV.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCorsPolicy();
}

builder.Services.AddScoped<INpvCalculator, NpvCalculator>();

builder.Services.AddValidatorsFromAssemblyContaining<NpvRequestValidator>();

builder.AddServiceDefaults();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseMiddleware<ExceptionMiddleware>();

#if DEBUG
app.MapGet("/api/test-exception", (HttpContext ctx) =>
{
    throw new Exception("Test exception!");
});
#endif

app.MapHealthChecks("/health");
app.MapDefaultEndpoints();
app.RegisterNpvEndpoints();

app.Run();

public partial class Program { }
