// BusX.Core/DTOs/JourneyDto.cs
namespace BusX.Core.DTOs
{
    // Arama sonuçlarında döneceğimiz sade veri
    public class JourneyDto
    {
        public int Id { get; set; }
        public string FromCity { get; set; } = string.Empty;
        public string ToCity { get; set; } = string.Empty;
        public DateTime Departure { get; set; }
        public DateTime ArrivalEstimate { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public decimal Price { get; set; } // Hesaplanmış son fiyat
    }
}