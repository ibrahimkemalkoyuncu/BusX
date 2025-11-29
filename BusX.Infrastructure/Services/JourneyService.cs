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


    // ... Önceki kodlar (GenerateFakeSeats metodunun altına ekle)

        public async Task<TicketResultDto> SellTicketsAsync(CreateTicketDto request)
        {
            // 1. Validasyonlar
            if (request.Seats.Count > 4)
                return new TicketResultDto { Success = false, Message = "Aynı anda en fazla 4 koltuk alabilirsiniz." };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var journey = await _context.Journeys.FindAsync(request.JourneyId);
                if (journey == null) return new TicketResultDto { Success = false, Message = "Sefer bulunamadı." };

                foreach (var seatReq in request.Seats)
                {
                    // Koltuğu bul
                    var seat = await _context.Seats.FindAsync(seatReq.SeatId);
                    if (seat == null) 
                        return new TicketResultDto { Success = false, Message = $"Koltuk ({seatReq.SeatId}) bulunamadı." };

                    // Kontrol 1: Zaten satılmış mı?
                    if (seat.IsSold)
                        return new TicketResultDto { Success = false, Message = $"Koltuk {seat.SeatNumber} zaten satılmış." };

                    // Kontrol 2: Cinsiyet Kuralı (Basit versiyon: Yan koltuk kontrolü eklenebilir)
                    if (seat.GenderLock.HasValue && seat.GenderLock != seatReq.Gender)
                        return new TicketResultDto { Success = false, Message = $"Koltuk {seat.SeatNumber} sadece {(seat.GenderLock == 1 ? "Erkek" : "Kadın")} yolcu içindir." };

                    // ⚡ KRİTİK NOKTA: Güncelleme
                    seat.IsSold = true;
                    seat.GenderLock = seatReq.Gender; // Satılınca o cinsiyete kilitlenir
                    
                    // SQLite Hilesi: RowVersion'ı manuel değiştiriyoruz ki EF Core farkı anlasın.
                    // MSSQL olsa buna gerek kalmazdı.
                    seat.RowVersion = Guid.NewGuid().ToByteArray(); 

                    // Bileti Oluştur
                    var ticket = new Ticket
                    {
                        JourneyId = request.JourneyId,
                        SeatId = seat.Id,
                        PassengerName = seatReq.PassengerName,
                        PassengerTc = seatReq.PassengerTc,
                        PassengerGender = seatReq.Gender,
                        PaidAmount = journey.BasePrice, // Şimdilik düz fiyat
                        Pnr = GeneratePNR()
                    };

                    _context.Tickets.Add(ticket);
                }

                // 2. Mock Ödeme (%10 Hata Simülasyonu)
                if (!MockPaymentService())
                {
                    return new TicketResultDto { Success = false, Message = "Ödeme alınamadı (Yetersiz Bakiye)." };
                }

                // 3. Veritabanına Kaydet (Concurrency Kontrolü Burada Yapılır)
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return new TicketResultDto { Success = true, Message = "İşlem Başarılı", Pnr = "PNR-" + new Random().Next(10000,99999) };
            }
            catch (DbUpdateConcurrencyException)
            {
                // ⚡⚡⚡ BİRİ BİZDEN ÖNCE DAVRANDI! ⚡⚡⚡
                await transaction.RollbackAsync();
                return new TicketResultDto { Success = false, Message = "Seçtiğiniz koltuklardan biri işlem sırasında başkası tarafından satın alındı. Lütfen tekrar deneyin." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new TicketResultDto { Success = false, Message = "Hata: " + ex.Message };
            }
        }

        // Yardımcı Metotlar
        private bool MockPaymentService()
        {
            // %90 Başarılı, %10 Başarısız
            return new Random().Next(100) > 10;
        }

        private string GeneratePNR()
        {
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

    }
}