using BusX.Core.DTOs;
using BusX.Core.Interfaces;
using BusX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BusX.Infrastructure.Services
{
    public class JourneyService : IJourneyService
    {
        private readonly BusXDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEnumerable<IPriceStrategy> _strategies;

        public JourneyService(BusXDbContext context, IMemoryCache cache, IEnumerable<IPriceStrategy> strategies)
        {
            _context = context;
            _cache = cache;
            _strategies = strategies;
        }

        public async Task<List<JourneyDto>> SearchJourneysAsync(int fromId, int toId, DateTime date)
        {
            // 1. Cache Key Oluştur (Örn: "Journey_1_2_2025-11-29")
            string cacheKey = $"Journey_{fromId}_{toId}_{date:yyyy-MM-dd}";

            // 2. Cache Kontrolü
            if (!_cache.TryGetValue(cacheKey, out List<JourneyDto>? journeys))
            {
                // Cache'de yoksa Veritabanına git 🐢
                var query = await _context.Journeys
                    .Include(j => j.FromStation)
                    .Include(j => j.ToStation)
                    .Where(j => j.FromStationId == fromId && 
                                j.ToStationId == toId && 
                                j.Departure.Date == date.Date &&
                                j.Departure > DateTime.UtcNow) // Geçmiş seferleri getirme kuralı
                    .ToListAsync();

                // Entity -> DTO Dönüşümü ve Fiyat Hesaplama
                journeys = query.Select(j =>
                {
                    // İlgili Provider'ın stratejisini bul
                    var strategy = _strategies.FirstOrDefault(s => s.ProviderName == j.ProviderName);
                    decimal finalPrice = strategy != null ? strategy.CalculatePrice(j.BasePrice) : j.BasePrice;

                    return new JourneyDto
                    {
                        Id = j.Id,
                        FromCity = j.FromStation.City,
                        ToCity = j.ToStation.City,
                        Departure = j.Departure,
                        ArrivalEstimate = j.ArrivalEstimate,
                        ProviderName = j.ProviderName,
                        Price = finalPrice
                    };
                }).ToList();

                // 3. Cache'e Yaz (60 Saniye TTL - İster Gereği)
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                _cache.Set(cacheKey, journeys, cacheOptions);
            }

            return journeys ?? new List<JourneyDto>();
        }

        public async Task<JourneyDto?> GetJourneyByIdAsync(int id)
        {
             // Detay sayfasını şu an cachelemiyoruz (Basitlik için)
             var j = await _context.Journeys
                    .Include(j => j.FromStation)
                    .Include(j => j.ToStation)
                    .FirstOrDefaultAsync(x => x.Id == id);
            
             if (j == null) return null;

             var strategy = _strategies.FirstOrDefault(s => s.ProviderName == j.ProviderName);
             decimal finalPrice = strategy != null ? strategy.CalculatePrice(j.BasePrice) : j.BasePrice;

             return new JourneyDto
             {
                 Id = j.Id,
                 FromCity = j.FromStation.City,
                 ToCity = j.ToStation.City,
                 Departure = j.Departure,
                 ArrivalEstimate = j.ArrivalEstimate,
                 ProviderName = j.ProviderName,
                 Price = finalPrice
             };
        }
    }
}