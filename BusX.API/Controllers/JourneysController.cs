using BusX.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneysController : ControllerBase
    {
        private readonly IJourneyService _journeyService;
        private readonly ILogger<JourneysController> _logger;

        public JourneysController(IJourneyService journeyService, ILogger<JourneysController> logger)
        {
            _journeyService = journeyService;
            _logger = logger;
        }


        // GET: api/journeys?fromId=1&toId=2&date=2025-11-30
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] int fromId, [FromQuery] int toId, [FromQuery] DateTime date)
        {
            // Basit validasyon
            if (fromId == 0 || toId == 0)
                return BadRequest("Kalkış ve Varış noktaları seçilmelidir.");

            _logger.LogInformation("Sefer aranıyor: {From} -> {To} Tarih: {Date}", fromId, toId, date);

            var result = await _journeyService.SearchJourneysAsync(fromId, toId, date);
            return Ok(result);
        }

        // GET: api/journeys/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var journey = await _journeyService.GetJourneyByIdAsync(id);
            if (journey == null) return NotFound("Sefer bulunamadı.");
            return Ok(journey);
        }


        // GET: api/journeys/1/seats
        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetSeats(int id)
        {
            var seats = await _journeyService.GetSeatPlanAsync(id);
            if (seats == null || !seats.Any()) return NotFound("Koltuk planı bulunamadı.");

            return Ok(seats);
        }
    }
}


