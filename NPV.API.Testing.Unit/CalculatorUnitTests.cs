using NPV.Shared.Models;
using NPV.API.Services;

namespace NPV.API.Testing.Unit;

public class CalculatorUnitTests
{
    private readonly NpvCalculator _calculator = new();

    [Fact]
    public void Calculate_WithValidInput_ReturnsCorrectCount()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m, 200m, 300m },
            LowerBound = 1.0m,
            UpperBound = 2.0m,
            Increment = 0.5m
        };

        var response = _calculator.Calculate(request);

        Assert.NotNull(response);
        Assert.NotNull(response.NpvResults);
        Assert.Equal(3, response.NpvResults.Count);
    }

    [Fact]
    public void Calculate_WithEmptyCashFlows_ReturnsEmptyResults()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal>(),
            LowerBound = 1.0m,
            UpperBound = 2.0m,
            Increment = 0.5m
        };

        var response = _calculator.Calculate(request);

        Assert.NotNull(response);
        Assert.NotNull(response.NpvResults);
        Assert.Empty(response.NpvResults);
    }

    [Fact]
    public void Calculate_WithNegativeCashFlows_ReturnsNegativeNPV()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { -100m, -200m, -300m },
            LowerBound = 5.0m,
            UpperBound = 5.0m,
            Increment = 0.25m
        };

        var response = _calculator.Calculate(request);

        Assert.NotNull(response);
        Assert.NotNull(response.NpvResults);
        Assert.Single(response.NpvResults);
        Assert.True(response.NpvResults.First().NPV < 0);
    }

    [Fact]
    public void Calculate_WithSingleCashFlow_ComputesCorrectly()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m },
            LowerBound = 10.0m,
            UpperBound = 10.0m,
            Increment = 1.0m
        };

        var response = _calculator.Calculate(request);

        decimal expected = Math.Round(100m / Pow(1.10m, 1), 2);

        Assert.NotNull(response);
        Assert.NotNull(response.NpvResults);
        Assert.Single(response.NpvResults);
        Assert.Equal(expected, response.NpvResults.First().NPV);
    }

    [Fact]
    public void Calculate_WithReversedBounds_ThrowsArgumentException()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m },
            LowerBound = 10.0m,
            UpperBound = 5.0m,
            Increment = 0.5m
        };

        Assert.Throws<ArgumentException>(() => _calculator.Calculate(request));
    }

    [Fact]
    public void Calculate_WithLargeIncrement_ReturnsSingleResult()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 100m, 200m },
            LowerBound = 5.0m,
            UpperBound = 10.0m,
            Increment = 10.0m
        };

        var response = _calculator.Calculate(request);

        Assert.NotNull(response);
        Assert.NotNull(response.NpvResults);
        Assert.Single(response.NpvResults);
    }

    [Fact]
    public void Calculate_RoundsResultsToTwoDecimalPlaces()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 123.456m, 789.123m },
            LowerBound = 7.1234m,
            UpperBound = 7.1234m,
            Increment = 1.0m
        };

        var response = _calculator.Calculate(request);

        Assert.NotNull(response);
        Assert.NotNull(response.NpvResults);

        var result = response.NpvResults.First();
        Assert.Equal(Math.Round(result.DiscountRate, 2), result.DiscountRate);
        Assert.Equal(Math.Round(result.NPV, 2), result.NPV);
    }

    private static decimal Pow(decimal baseVal, int exp)
    {
        decimal result = 1m;
        for (int i = 0; i < exp; i++)
        {
            result *= baseVal;
        }
        return result;
    }
}
