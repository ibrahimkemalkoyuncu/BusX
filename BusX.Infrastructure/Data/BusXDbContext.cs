using BusX.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusX.Infrastructure.Data
{
    public class BusXDbContext : DbContext
    {
        public BusXDbContext(DbContextOptions<BusXDbContext> options) : base(options)
        {
        }

        public DbSet<Station> Stations { get; set; }
        public DbSet<Journey> Journeys { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seat>().Property(s => s.RowVersion).IsConcurrencyToken();
            modelBuilder.Entity<Journey>().HasOne(j => j.FromStation).WithMany().HasForeignKey(j => j.FromStationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Journey>().HasOne(j => j.ToStation).WithMany().HasForeignKey(j => j.ToStationId).OnDelete(DeleteBehavior.Restrict);

            SeedData(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // --- A. 81 İL ---
            var cities = new string[] {
                "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
                "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
                "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari",
                "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
                "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
                "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
                "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
                "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
            };

            var stations = new List<Station>();
            for (int i = 0; i < cities.Length; i++)
            {
                stations.Add(new Station { Id = i + 1, City = cities[i], Name = $"{cities[i]} Otogarı" });
            }
            modelBuilder.Entity<Station>().HasData(stations);

            // --- B. 60 GÜNLÜK GARANTİ SEFERLER ---
            var journeys = new List<Journey>();
            int journeyId = 1;

            // Şehir ID'leri (Plaka Sırası değil, Dizi Index+1 sırası)
            // İstanbul=34, Ankara=6, İzmir=35, Antalya=7, Bursa=16, Adana=1
            int ist = 34; int ank = 6; int izm = 35; int ant = 7; int bur = 16; int ada = 1;

            for (int i = 0; i < 60; i++)
            {
                DateTime targetDate = DateTime.UtcNow.Date.AddDays(i);

                // --- 1. İSTANBUL - ANKARA HATTI (Çok Sık) ---
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ist, ToStationId = ank, Departure = targetDate.AddHours(9), ArrivalEstimate = targetDate.AddHours(15), BasePrice = 550, ProviderName = "ProviderA" });
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ist, ToStationId = ank, Departure = targetDate.AddHours(13), ArrivalEstimate = targetDate.AddHours(19), BasePrice = 450, ProviderName = "ProviderB" });
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ist, ToStationId = ank, Departure = targetDate.AddHours(19), ArrivalEstimate = targetDate.AddHours(25), BasePrice = 600, ProviderName = "ProviderA" });
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ist, ToStationId = ank, Departure = targetDate.AddHours(23).AddMinutes(30), ArrivalEstimate = targetDate.AddDays(1).AddHours(5), BasePrice = 400, ProviderName = "ProviderB" });

                // --- 2. ANKARA - İSTANBUL HATTI ---
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ank, ToStationId = ist, Departure = targetDate.AddHours(10), ArrivalEstimate = targetDate.AddHours(16), BasePrice = 500, ProviderName = "ProviderA" });
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ank, ToStationId = ist, Departure = targetDate.AddHours(15), ArrivalEstimate = targetDate.AddHours(21), BasePrice = 450, ProviderName = "ProviderB" });

                // --- 3. İSTANBUL - İZMİR HATTI ---
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ist, ToStationId = izm, Departure = targetDate.AddHours(11), ArrivalEstimate = targetDate.AddHours(18), BasePrice = 700, ProviderName = "ProviderA" });
                journeys.Add(new Journey { Id = journeyId++, FromStationId = izm, ToStationId = ist, Departure = targetDate.AddHours(22), ArrivalEstimate = targetDate.AddDays(1).AddHours(6), BasePrice = 650, ProviderName = "ProviderB" });

                // --- 4. İZMİR - ANTALYA HATTI ---
                journeys.Add(new Journey { Id = journeyId++, FromStationId = izm, ToStationId = ant, Departure = targetDate.AddHours(08), ArrivalEstimate = targetDate.AddHours(14), BasePrice = 500, ProviderName = "ProviderB" });

                // --- 5. İSTANBUL - BURSA HATTI ---
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ist, ToStationId = bur, Departure = targetDate.AddHours(12), ArrivalEstimate = targetDate.AddHours(14), BasePrice = 250, ProviderName = "ProviderA" });
                journeys.Add(new Journey { Id = journeyId++, FromStationId = bur, ToStationId = ist, Departure = targetDate.AddHours(16), ArrivalEstimate = targetDate.AddHours(18), BasePrice = 200, ProviderName = "ProviderB" });

                // --- 6. ADANA - ANKARA HATTI ---
                journeys.Add(new Journey { Id = journeyId++, FromStationId = ada, ToStationId = ank, Departure = targetDate.AddHours(20), ArrivalEstimate = targetDate.AddDays(1).AddHours(04), BasePrice = 550, ProviderName = "ProviderA" });
            }

            modelBuilder.Entity<Journey>().HasData(journeys);
        }
    }
}