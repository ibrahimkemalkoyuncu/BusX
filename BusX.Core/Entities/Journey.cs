// BusX.Core/Entities/Journey.cs
namespace BusX.Core.Entities
{
    public class Journey : BaseEntity
    {
        public int FromStationId { get; set; }
        public Station FromStation { get; set; } = null!;

        public int ToStationId { get; set; }
        public Station ToStation { get; set; } = null!;

        public DateTime Departure { get; set; }
        public DateTime ArrivalEstimate { get; set; } // Tahmini varış
        
        public string ProviderName { get; set; } = string.Empty; // ProviderA, ProviderB
        public decimal BasePrice { get; set; }

        // Navigation Property
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}