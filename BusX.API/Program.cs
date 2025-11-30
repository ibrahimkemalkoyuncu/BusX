using BusX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BusX.Core.Interfaces;
using BusX.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Veritabanı Bağlantısı
// Not: ConnectionString appsettings.json dosyasından gelir.
builder.Services.AddDbContext<BusXDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cache Servisini Aktif Et (IMemoryCache)
builder.Services.AddMemoryCache();

// 3. Provider Stratejilerini Ekle (Strategy Pattern)
builder.Services.AddScoped<IPriceStrategy, ProviderAStrategy>();
builder.Services.AddScoped<IPriceStrategy, ProviderBStrategy>();

// 4. Journey Servisini Ekle
builder.Services.AddScoped<IJourneyService, JourneyService>();

builder.Services.AddControllers();

// Swagger/OpenAPI konfigürasyonu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ============================================================
// 🚑 SELF-HEALING: Otomatik Veritabanı Kurulumu ve Migration
// ============================================================
// Uygulama ayağa kalkmadan önce veritabanının varlığından emin oluyoruz.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BusXDbContext>();

        // Bu komut, henüz uygulanmamış migration'ları veritabanına uygular.
        // Veritabanı yoksa oluşturur (BusX.db).
        context.Database.Migrate();

        Console.WriteLine("✅ Veritabanı başarıyla güncellendi ve seed datalar kontrol edildi.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Veritabanı oluşturulurken kritik bir hata oluştu.");
    }
}
// ============================================================

app.Run();