namespace BusX.Core.DTOs
{
    public class SeatDto
    {
        public int Id { get; set; }
        public int SeatNumber { get; set; }
        public int Row { get; set; }     // Hangi sırada?
        public int Column { get; set; }  // Hangi sütunda?
        public int Type { get; set; }    // 0: Koridor, 1: Cam Kenarı, 2: Tekli
        public bool IsSold { get; set; }
        public int? GenderLock { get; set; } // 1: Erkek, 2: Kadın
        public decimal Price { get; set; }
    }
}