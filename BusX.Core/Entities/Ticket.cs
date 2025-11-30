// BusX.Core/Entities/Ticket.cs
namespace BusX.Core.Entities
{
    public class Ticket : BaseEntity
    {
        public string Pnr { get; set; } = string.Empty; // Referans Kodu
        public int JourneyId { get; set; }
        public int SeatId { get; set; } // Hangi koltuk?
        
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerTc { get; set; } = string.Empty;
        
        // 1: Erkek, 2: Kadın
        public int PassengerGender { get; set; }
        
        public decimal PaidAmount { get; set; }
    }
}