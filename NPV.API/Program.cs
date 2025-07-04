using FluentValidation;
using NPV.API.Extensions;
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.MapHealthChecks("/health");
app.MapDefaultEndpoints();
app.RegisterNpvEndpoints();

app.Run();

public partial class Program { }
