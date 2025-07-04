using NPV.Shared.Models;

namespace NPV.Shared.Interfaces;

public interface INpvCalculator
{
    NpvResponse Calculate(NpvRequest request);
}