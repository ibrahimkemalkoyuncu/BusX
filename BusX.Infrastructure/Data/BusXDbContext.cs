// BusX.Infrastructure/Data/BusXDbContext.cs
using BusX.Core.Entities;
using Microsoft.EntityFrameworkCore;


namespace BusX.Infrastructure.Data
{
    public class BusXDbContext : DbContext
    {
        public BusXDbContext(DbContextOptions<BusXDbContext> options) : base(options)
        {
        }

        // Tablolarımız
        public DbSet<Station> Stations { get; set; }
        public DbSet<Journey> Journeys { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Concurrency (Eşzamanlılık) Ayarı 🚨
            // SQLite için RowVersion alanını ConcurrencyToken olarak işaretliyoruz.
            // Bu alan değiştiğinde EF Core, güncelleme sırasında hata fırlatacak.
            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
               .IsConcurrencyToken(); 

            // 2. İlişkiler ve Kısıtlamalar
            modelBuilder.Entity<Journey>()
                .HasOne(j => j.FromStation)
                .WithMany()
                .HasForeignKey(j => j.FromStationId)
                .OnDelete(DeleteBehavior.Restrict); // İstasyon silinirse sefer silinmesin

            modelBuilder.Entity<Journey>()
                .HasOne(j => j.ToStation)
                .WithMany()
                .HasForeignKey(j => j.ToStationId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Data Seeding (Tohumlama) 🌱
            // Uygulama ilk açıldığında boş gelmemesi için verileri gömüyoruz.
            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // İstasyonlar
            modelBuilder.Entity<Station>().HasData(
                new Station { Id = 1, City = "Istanbul", Name = "Esenler Otogarı" },
                new Station { Id = 2, City = "Ankara", Name = "AŞTİ" },
                new Station { Id = 3, City = "Izmir", Name = "İzotaş" }
            );

            // Örnek Sefer (Yarın sabah 10:00)
            modelBuilder.Entity<Journey>().HasData(
                new Journey 
                { 
                    Id = 1, 
                    FromStationId = 1, // İst -> Ank
                    ToStationId = 2, 
                    Departure = DateTime.UtcNow.AddDays(1).Date.AddHours(10), // Yarın 10:00
                    ArrivalEstimate = DateTime.UtcNow.AddDays(1).Date.AddHours(16), 
                    BasePrice = 500, 
                    ProviderName = "ProviderA" // Pahalı ama kaliteli
                },
                new Journey 
                { 
                    Id = 2, 
                    FromStationId = 1, // İst -> Ank
                    ToStationId = 2, 
                    Departure = DateTime.UtcNow.AddDays(1).Date.AddHours(11), 
                    ArrivalEstimate = DateTime.UtcNow.AddDays(1).Date.AddHours(18), 
                    BasePrice = 450, 
                    ProviderName = "ProviderB" // Biraz daha ucuz
                }
            );
            
            // Koltukları burada seed etmek uzun sürer, onu Servis katmanında 
            // "Sefer oluşturulduğunda otomatik koltuk ekle" mantığıyla yapacağız.
        }
    }
}