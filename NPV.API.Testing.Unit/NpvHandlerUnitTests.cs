using FluentValidation;
using FluentValidation.Results;
using Moq;
using NPV.API.EndpointHandlers;
using NPV.Shared.Models;
using NPV.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Unit tests for NpvHandler endpoint, testing null, invalid, and valid request scenarios,
/// as well as multi-error validation cases.
/// </summary>
namespace NPV.API.Testing.Unit;

public class NpvHandlersTests
{
    private readonly Mock<INpvCalculator> _calculatorMock = new();
    private readonly Mock<IValidator<NpvRequest>> _validatorMock = new();

    [Fact]
    public async Task CalculateAsync_NullRequest_ReturnsBadRequest()
    {
        var httpContext = new DefaultHttpContext();
        var result = await NpvHandler.CalculateAsync(null, _calculatorMock.Object, _validatorMock.Object, httpContext);

        var details = GetProblemDetailsFromResult(result);
        Assert.Equal(400, details.Status);
        Assert.Equal("Validation error.", details.Title);
        Assert.Contains("Request", details.Errors.Keys);
        Assert.Contains("Request cannot be null.", details.Errors["Request"]);
        Assert.Equal(httpContext.Request.Path, details.Instance);
    }

    [Fact]
    public async Task CalculateAsync_InvalidModel_ReturnsBadRequest()
    {
        var httpContext = new DefaultHttpContext();
        var invalidRequest = new NpvRequest();

        _validatorMock.Setup(v => v.ValidateAsync(invalidRequest, default))
            .ReturnsAsync(new ValidationResult(new[] {
                new ValidationFailure("CashFlows", "CashFlows required")
            }));

        var result = await NpvHandler.CalculateAsync(invalidRequest, _calculatorMock.Object, _validatorMock.Object, httpContext);

        var details = GetProblemDetailsFromResult(result);
        Assert.Equal(400, details.Status);
        Assert.Contains("CashFlows", details.Errors.Keys);
        Assert.Contains("CashFlows required", details.Errors["CashFlows"]);
        Assert.Equal(httpContext.Request.Path, details.Instance);
    }

    [Theory]
    [MemberData(nameof(MultipleValidationFailures))]
    public async Task CalculateAsync_MultipleValidationErrors_ReturnsAllErrors(List<ValidationFailure> failures, string[] expectedKeys)
    {
        var httpContext = new DefaultHttpContext();
        var request = new NpvRequest();

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult(failures));

        var result = await NpvHandler.CalculateAsync(request, _calculatorMock.Object, _validatorMock.Object, httpContext);

        var details = GetProblemDetailsFromResult(result);
        Assert.Equal(400, details.Status);

        foreach (var key in expectedKeys)
        {
            Assert.Contains(key, details.Errors.Keys);
        }
        Assert.Equal(httpContext.Request.Path, details.Instance);
    }

    public static IEnumerable<object[]> MultipleValidationFailures()
    {
        yield return new object[]
        {
            new List<ValidationFailure>
            {
                new ValidationFailure("CashFlows", "CashFlows required"),
                new ValidationFailure("Rate", "Rate required"),
            },
            new[] { "CashFlows", "Rate" }
        };

        yield return new object[]
        {
            new List<ValidationFailure>
            {
                new ValidationFailure("Increment", "Increment required"),
                new ValidationFailure("LowerBound", "LowerBound required"),
                new ValidationFailure("UpperBound", "UpperBound required"),
            },
            new[] { "Increment", "LowerBound", "UpperBound" }
        };
    }

    [Fact]
    public async Task CalculateAsync_ValidRequest_ReturnsOk()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m, 200m },
            LowerBound = 1m,
            UpperBound = 2m,
            Increment = 1m
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        var expectedResponse = new NpvResponse
        {
            NpvResults = new List<NpvResult>
            {
                new NpvResult { DiscountRate = 1.0m, NPV = 295.0m }
            }
        };

        _calculatorMock.Setup(x => x.Calculate(request)).Returns(expectedResponse);

        var httpContext = new DefaultHttpContext();

        var result = await NpvHandler.CalculateAsync(request, _calculatorMock.Object, _validatorMock.Object, httpContext);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<NpvResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.NpvResults);
        Assert.Single(okResult.Value.NpvResults);
        Assert.Equal(expectedResponse.NpvResults.First().NPV, okResult.Value.NpvResults.First().NPV);
    }

    /// <summary>
    /// Helper to extract ValidationProblemDetails from the IResult, or throw if not present.
    /// </summary>
    private static ValidationProblemDetails GetProblemDetailsFromResult(IResult result)
    {
        var httpResult = result as Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ValidationProblemDetails>;
        Assert.NotNull(httpResult);
        var details = httpResult.Value;
        Assert.NotNull(details);
        return details;
    }
}
