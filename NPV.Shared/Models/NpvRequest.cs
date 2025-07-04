namespace NPV.Shared.Models;

public class NpvRequest
{
    public List<decimal> CashFlows { get; set; } = new();
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
    public decimal Increment { get; set; }
}
