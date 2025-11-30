// BusX.Core/Entities/Seat.cs
namespace BusX.Core.Entities
{
    public class Seat : BaseEntity
    {
        public int JourneyId { get; set; }
        // Navigation property'i virtual yapmıyorum, lazy loading kullanmayacağız (performans)
        
        public int SeatNumber { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        
        // 0: Koridor, 1: Cam Kenarı, 2: Tekli vb. (Enum yapılabilir)
        public int Type { get; set; } 
        
        // Bu koltuk satıldı mı?
        public bool IsSold { get; set; } = false;
        
        // Cinsiyet kısıtlaması (0: Yok, 1: Erkek, 2: Kadın)
        public int? GenderLock { get; set; }

        // ⚡ CONCURRENCY TOKEN ⚡
        // Bu alan veritabanında her güncellemede değişir.
        // Eğer client eski bir versiyonla gelirse, DB reddeder.
        public byte[] RowVersion { get; set; } = Array.Empty<byte>(); 
    }
}