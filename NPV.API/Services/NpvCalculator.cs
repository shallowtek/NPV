using NPV.Shared.Interfaces;
using NPV.Shared.Models;

namespace NPV.API.Services;

/// <summary>
/// Provides NPV calculation across a range of discount rates.
/// </summary>
public class NpvCalculator : INpvCalculator
{
    public NpvResponse Calculate(NpvRequest request)
    {
        if (request.CashFlows == null || !request.CashFlows.Any())
            return new NpvResponse { NpvResults = new List<NpvResult>() };

        if (request.Increment <= 0 || request.LowerBound > request.UpperBound)
            throw new ArgumentException("Invalid input: increment must be positive, and lower bound must not exceed upper bound.");

        var results = new List<NpvResult>();
        for (decimal rate = request.LowerBound; rate <= request.UpperBound; rate += request.Increment)
        {
            decimal discountRate = rate / 100m;
            decimal npv = CalculateNpv(request.CashFlows, discountRate);
            results.Add(new NpvResult
            {
                DiscountRate = Math.Round(rate, 2),
                NPV = Math.Round(npv, 2)
            });
        }
        return new NpvResponse { NpvResults = results };
    }

    /// <summary>
    /// Calculates the NPV for a series of cash flows at a given discount rate.
    /// </summary>
    private decimal CalculateNpv(List<decimal> cashFlows, decimal discountRate)
    {
        decimal npv = 0m;
        for (int t = 0; t < cashFlows.Count; t++)
            npv += cashFlows[t] / Pow(1 + discountRate, t + 1);
        return npv;
    }

    /// <summary>
    /// Raises a decimal to an integer exponent.
    /// </summary>
    private static decimal Pow(decimal baseValue, int exponent)
    {
        decimal result = 1m;
        for (int i = 0; i < exponent; i++)
            result *= baseValue;
        return result;
    }
}
