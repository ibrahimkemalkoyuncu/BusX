namespace BusX.Core.DTOs
{
    public class CreateTicketDto
    {
        public int JourneyId { get; set; }
        public List<SeatSelectionDto> Seats { get; set; } = new();
    }

    public class SeatSelectionDto
    {
        public int SeatId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerTc { get; set; } = string.Empty;
        public int Gender { get; set; } // 1: Erkek, 2: Kadın
    }
    
    // İşlem sonucunda döneceğimiz cevap
    public class TicketResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Pnr { get; set; } = string.Empty;
    }
}