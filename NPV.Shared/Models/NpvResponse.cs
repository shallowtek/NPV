namespace NPV.Shared.Models;

public class NpvResponse
{
    public List<NpvResult>? NpvResults { get; set; }
    
}

public class NpvResult
{
    public decimal DiscountRate { get; set; }
    public decimal NPV { get; set; }
}