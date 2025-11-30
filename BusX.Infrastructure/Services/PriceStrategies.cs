// BusX.Infrastructure/Services/PriceStrategies.cs
using BusX.Core.Interfaces;

namespace BusX.Infrastructure.Services
{
    // Provider A: Lüks firma, taban fiyata %10 servis ücreti ekler.
    public class ProviderAStrategy : IPriceStrategy
    {
        public string ProviderName => "ProviderA";
        public decimal CalculatePrice(decimal basePrice) => basePrice * 1.10m; 
    }

    // Provider B: Halk otobüsü, taban fiyat neyse odur.
    public class ProviderBStrategy : IPriceStrategy
    {
        public string ProviderName => "ProviderB";
        public decimal CalculatePrice(decimal basePrice) => basePrice;
    }
}