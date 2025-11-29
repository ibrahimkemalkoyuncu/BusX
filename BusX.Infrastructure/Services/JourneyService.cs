using BusX.Core.DTOs;
using BusX.Core.Entities;
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

        // 1. SEFER ARAMA (Akıllı Tarih Filtreli) 🧠
        public async Task<List<JourneyDto>> SearchJourneysAsync(int fromId, int toId, DateTime date)
        {
            // Cache Key
            string cacheKey = $"Journey_{fromId}_{toId}_{date:yyyy-MM-dd}";

            // Cache'de yoksa veya süre dolduysa
            if (!_cache.TryGetValue(cacheKey, out List<JourneyDto>? journeys))
            {
                // Sorguyu Hazırla
                var query = _context.Journeys
                    .Include(j => j.FromStation)
                    .Include(j => j.ToStation)
                    .Where(j => j.FromStationId == fromId &&
                                j.ToStationId == toId &&
                                j.Departure >= date.Date &&
                                j.Departure < date.Date.AddDays(1)); // O günün tamamı

                // ⚡ DÜZELTME BURADA:
                // Eğer aranan tarih BUGÜN ise, şu anki saatten (UtcNow) öncekileri gizle!
                if (date.Date == DateTime.UtcNow.Date)
                {
                    query = query.Where(j => j.Departure > DateTime.UtcNow);
                }

                // Sıralama ve Çalıştırma
                var resultEntities = await query.OrderBy(j => j.Departure).ToListAsync();

                // Entity -> DTO Dönüşümü
                journeys = resultEntities.Select(j =>
                {
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

                // Cache Ayarları (60 sn)
                var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                _cache.Set(cacheKey, journeys, cacheOptions);
            }

            return journeys ?? new List<JourneyDto>();
        }

        // 2. SEFER DETAYI
        public async Task<JourneyDto?> GetJourneyByIdAsync(int id)
        {
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

        // 3. KOLTUK PLANI (Travego 2+1)
        public async Task<List<SeatDto>> GetSeatPlanAsync(int journeyId)
        {
            var journey = await _context.Journeys.FindAsync(journeyId);
            if (journey == null) return new List<SeatDto>();

            var seats = await _context.Seats
                .Where(s => s.JourneyId == journeyId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();

            if (!seats.Any())
            {
                seats = GenerateFakeSeats(journeyId);
                _context.Seats.AddRange(seats);
                await _context.SaveChangesAsync();
            }

            var strategy = _strategies.FirstOrDefault(s => s.ProviderName == journey.ProviderName);
            decimal finalPrice = strategy != null ? strategy.CalculatePrice(journey.BasePrice) : journey.BasePrice;

            return seats.Select(s => new SeatDto
            {
                Id = s.Id,
                SeatNumber = s.SeatNumber,
                Row = s.Row,
                Column = s.Column,
                Type = s.Type,
                IsSold = s.IsSold,
                GenderLock = s.GenderLock,
                Price = finalPrice
            }).ToList();
        }

        // 4. TRAVEGO DÜZENİ OLUŞTURUCU
        private List<Seat> GenerateFakeSeats(int journeyId)
        {
            var seats = new List<Seat>();
            void AddSeat(int number, int row, int col, int type)
            {
                seats.Add(new Seat { JourneyId = journeyId, SeatNumber = number, Row = row, Column = col, Type = type, RowVersion = Array.Empty<byte>() });
            }

            // A. ÖN BÖLÜM (1-6)
            for (int r = 1; r <= 6; r++)
            {
                int baseNum = 1 + (r - 1) * 3;
                AddSeat(baseNum, r, 1, 2); AddSeat(baseNum + 1, r, 4, 0); AddSeat(baseNum + 2, r, 5, 1);
            }
            // B. KAPI ÖNÜ (7)
            AddSeat(19, 7, 1, 2); AddSeat(22, 7, 4, 0); AddSeat(23, 7, 5, 1);
            // C. KAPI HİZASI (8-9 Sol)
            AddSeat(20, 8, 1, 2); AddSeat(21, 9, 1, 2);
            // D. ARKA BÖLÜM (10-13)
            int cR = 10; int[] lS = { 24, 27, 30, 33 };
            foreach (var l in lS) { AddSeat(l, cR, 1, 2); AddSeat(l + 1, cR, 4, 0); AddSeat(l + 2, cR, 5, 1); cR++; }
            // E. EN ARKA (14 Sağ)
            AddSeat(37, 14, 4, 0); AddSeat(38, 14, 5, 1);
            return seats;
        }

        // 5. BİLET SATIŞ
        public async Task<TicketResultDto> SellTicketsAsync(CreateTicketDto request)
        {
            if (request.Seats.Count > 4) return new TicketResultDto { Success = false, Message = "Max 4 koltuk." };
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var journey = await _context.Journeys.FindAsync(request.JourneyId);
                if (journey == null) return new TicketResultDto { Success = false, Message = "Sefer yok." };

                foreach (var seatReq in request.Seats)
                {
                    var seat = await _context.Seats.FindAsync(seatReq.SeatId);
                    if (seat == null || seat.IsSold) return new TicketResultDto { Success = false, Message = "Koltuk müsait değil." };
                    if (seat.GenderLock.HasValue && seat.GenderLock != seatReq.Gender) return new TicketResultDto { Success = false, Message = "Cinsiyet uyuşmazlığı." };

                    seat.IsSold = true;
                    seat.GenderLock = seatReq.Gender;
                    seat.RowVersion = Guid.NewGuid().ToByteArray(); // SQLite Concurrency

                    _context.Tickets.Add(new Ticket
                    {
                        JourneyId = request.JourneyId,
                        SeatId = seat.Id,
                        PassengerName = seatReq.PassengerName,
                        PassengerTc = seatReq.PassengerTc,
                        PassengerGender = seatReq.Gender,
                        PaidAmount = journey.BasePrice,
                        Pnr = Guid.NewGuid().ToString().Substring(0, 6).ToUpper()
                    });
                }

                if (new Random().Next(100) < 10) return new TicketResultDto { Success = false, Message = "Ödeme Başarısız (Mock)." };

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new TicketResultDto { Success = true, Message = "İşlem Başarılı", Pnr = "PNR-SUCCESS" };
            }
            catch { await transaction.RollbackAsync(); return new TicketResultDto { Success = false, Message = "Çakışma: Koltuk başkası tarafından alındı." }; }
        }
    }
}