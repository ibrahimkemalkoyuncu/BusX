// BusX.Core/Interfaces/IPriceStrategy.cs
namespace BusX.Core.Interfaces
{
    public interface IPriceStrategy
    {
        string ProviderName { get; }
        decimal CalculatePrice(decimal basePrice);
    }
}