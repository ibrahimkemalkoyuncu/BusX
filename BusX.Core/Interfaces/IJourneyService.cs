using BusX.Core.DTOs;

namespace BusX.Core.Interfaces
{
    public interface IJourneyService
    {
        Task<List<JourneyDto>> SearchJourneysAsync(int fromId, int toId, DateTime date);
        Task<JourneyDto?> GetJourneyByIdAsync(int id);

        // Koltukları Getir
        Task<List<SeatDto>> GetSeatPlanAsync(int journeyId);

        // Bilet Satışı
        Task<TicketResultDto> SellTicketsAsync(CreateTicketDto request);
    }
}