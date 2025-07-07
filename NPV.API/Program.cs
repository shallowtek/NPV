using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NPV.API.Data;
using NPV.API.Extensions;
using NPV.API.Middleware;
using NPV.API.Services;
using NPV.Shared.Interfaces;
using NPV.Shared.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCorsPolicy();
}

// Register NPV/business services
builder.Services.AddScoped<INpvCalculator, NpvCalculator>();
builder.Services.AddValidatorsFromAssemblyContaining<NpvRequestValidator>();

// Register Identity + DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("DemoAuthDb"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Use your extension for all other default configs, including JWT authentication, Swagger, health, etc
builder.AddServiceDefaults();

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

// Authentication/authorization middlewares are registered in your MapDefaultEndpoints extension!

#if DEBUG
app.MapGet("/api/test-exception", (HttpContext ctx) =>
{
    throw new Exception("Test exception!");
});
#endif

app.MapHealthChecks("/health");
app.MapDefaultEndpoints();
app.RegisterNpvEndpoints();
app.RegisterAuthEndpoints();

// ... Registration/Login endpoints, seed roles, etc (as in earlier answers) ...

app.Run();


// Use for Integration Tests
public partial class Program { }
