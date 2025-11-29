using BusX.Core.DTOs;
using BusX.Core.Interfaces;
using BusX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using BusX.Core.Entities;

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

        #region  Koltukları Getir
        public async Task<List<SeatDto>> GetSeatPlanAsync(int journeyId)
        {
            // 1. Önce sefer var mı diye bak
            var journey = await _context.Journeys.FindAsync(journeyId);
            if (journey == null) return new List<SeatDto>();

            // 2. Bu seferin koltukları DB'de var mı?
            var seats = await _context.Seats
                .Where(s => s.JourneyId == journeyId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();

            // 3. Eğer hiç koltuk yoksa (İlk kez tıklanıyorsa), OTOMATİK OLUŞTUR!
            if (!seats.Any())
            {
                seats = GenerateFakeSeats(journeyId);
                _context.Seats.AddRange(seats);
                await _context.SaveChangesAsync(); // Veritabanına kaydet
            }

            // 4. Stratejiye göre fiyatı hesapla
            var strategy = _strategies.FirstOrDefault(s => s.ProviderName == journey.ProviderName);
            decimal finalPrice = strategy != null ? strategy.CalculatePrice(journey.BasePrice) : journey.BasePrice;

            // 5. Entity -> DTO Dönüşümü
            return seats.Select(s => new SeatDto
            {
                Id = s.Id,
                SeatNumber = s.SeatNumber,
                Row = s.Row,
                Column = s.Column,
                Type = s.Type,
                IsSold = s.IsSold,
                GenderLock = s.GenderLock,
                Price = finalPrice // Her koltuk aynı fiyat (şimdilik)
            }).ToList();
        }

        // Sahte Koltuk Fabrikası (2+1 Otobüs Düzeni)
        private List<Seat> GenerateFakeSeats(int journeyId)
        {
            var seats = new List<Seat>();
            int seatNumber = 1;

            // 10 Sıra koltuk olsun
            for (int row = 1; row <= 10; row++)
            {
                // Sol taraf (Tekli Koltuk - Cam Kenarı)
                seats.Add(new Seat { JourneyId = journeyId, SeatNumber = seatNumber++, Row = row, Column = 1, Type = 2, RowVersion = Array.Empty<byte>() });

                // Sağ taraf (İkili Koltuk)
                seats.Add(new Seat { JourneyId = journeyId, SeatNumber = seatNumber++, Row = row, Column = 3, Type = 0, RowVersion = Array.Empty<byte>() }); // Koridor
                seats.Add(new Seat { JourneyId = journeyId, SeatNumber = seatNumber++, Row = row, Column = 4, Type = 1, RowVersion = Array.Empty<byte>() }); // Cam Kenarı
            }

            return seats;
        }
        #endregion


    }
}