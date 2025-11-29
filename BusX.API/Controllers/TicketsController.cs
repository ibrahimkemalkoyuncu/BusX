using BusX.Core.DTOs;
using BusX.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly IJourneyService _journeyService;

        public TicketsController(IJourneyService journeyService)
        {
            _journeyService = journeyService;
        }

        // POST: api/tickets/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CreateTicketDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _journeyService.SellTicketsAsync(request);

            if (!result.Success)
            {
                // Eğer hata Concurrency veya Satılmış koltuk ise 409 Conflict dönmek daha doğrudur
                if (result.Message.Contains("satılmış") || result.Message.Contains("başkası"))
                    return Conflict(result);

                // Ödeme hatası ise 402 Payment Required
                if (result.Message.Contains("Ödeme"))
                    return StatusCode(402, result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
