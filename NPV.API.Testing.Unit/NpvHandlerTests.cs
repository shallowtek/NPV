using FluentValidation;
using FluentValidation.Results;
using Moq;
using NPV.API.EndpointHandlers;
using NPV.Shared.Models;
using NPV.Shared.Interfaces;
using Microsoft.AspNetCore.Http;

namespace NPV.API.Testing.Unit;

public class NpvHandlersTests
{
    private readonly Mock<INpvCalculator> _calculatorMock = new();
    private readonly Mock<IValidator<NpvRequest>> _validatorMock = new();

    [Fact]
    public async Task CalculateAsync_NullRequest_ReturnsBadRequest()
    {
        var result = await NpvHandlers.CalculateAsync(null, _calculatorMock.Object, _validatorMock.Object);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        Assert.NotNull(valueResult.Value);
        var errorList = ((IEnumerable<object>)valueResult.Value).ToList();
        Assert.Single(errorList);

        var error = errorList[0];
        var propertyName = error.GetType().GetProperty("PropertyName")?.GetValue(error)?.ToString();
        var errorMessage = error.GetType().GetProperty("ErrorMessage")?.GetValue(error)?.ToString();

        Assert.Equal("Request", propertyName);
        Assert.Equal("Request cannot be null.", errorMessage);
    }

    [Fact]
    public async Task CalculateAsync_InvalidModel_ReturnsBadRequest()
    {
        var invalidRequest = new NpvRequest();

        _validatorMock.Setup(v => v.ValidateAsync(invalidRequest, default))
            .ReturnsAsync(new ValidationResult(new[] {
                new ValidationFailure("CashFlows", "CashFlows required")
            }));

        var result = await NpvHandlers.CalculateAsync(invalidRequest, _calculatorMock.Object, _validatorMock.Object);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        Assert.NotNull(valueResult.Value);
        var errorList = ((IEnumerable<object>)valueResult.Value).ToList();
        Assert.Contains(errorList, e =>
        {
            var propertyName = e.GetType().GetProperty("PropertyName")?.GetValue(e)?.ToString();
            var errorMessage = e.GetType().GetProperty("ErrorMessage")?.GetValue(e)?.ToString();
            return propertyName == "CashFlows" && errorMessage == "CashFlows required";
        });
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

        var result = await NpvHandlers.CalculateAsync(request, _calculatorMock.Object, _validatorMock.Object);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<NpvResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.NpvResults);
        Assert.Single(okResult.Value.NpvResults);
        Assert.Equal(expectedResponse.NpvResults.First().NPV, okResult.Value.NpvResults.First().NPV);
    }
}
