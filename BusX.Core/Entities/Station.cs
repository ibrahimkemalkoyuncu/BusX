// BusX.Core/Entities/Station.cs
namespace BusX.Core.Entities
{
    public class Station : BaseEntity
    {
        public string City { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // Örn: Esenler Otogarı
    }
}