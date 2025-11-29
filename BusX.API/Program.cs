using BusX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BusX.Core.Interfaces;
using BusX.Infrastructure.Services;
using Serilog;
using BusX.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. SERILOG AYARLARI (Loglama) 📝
// ============================================================
// Logları hem konsola hem de 'logs' klasörüne dosyalar halinde yazar.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/busx-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

// ============================================================
// 2. CORS AYARLARI (Frontend İzni) 🔓
// ============================================================
// React uygulamasının (http://localhost:5173) API'ye erişmesine izin ver.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ============================================================
// 3. VERİTABANI VE SERVİSLER 🔌
// ============================================================
builder.Services.AddDbContext<BusXDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache(); // Cache servisi

// Strategy Pattern (Fiyatlandırma için)
builder.Services.AddScoped<IPriceStrategy, ProviderAStrategy>();
builder.Services.AddScoped<IPriceStrategy, ProviderBStrategy>();

// Ana İş Mantığı Servisi
builder.Services.AddScoped<IJourneyService, JourneyService>();

// ============================================================
// 4. HEALTH CHECK (Sistem Sağlığı) ❤️
// ============================================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BusXDbContext>(); // DB bağlantısını da kontrol et

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================================
// 5. MIDDLEWARE SIRALAMASI (Pipeline) 🚦
// ============================================================

// Correlation ID (Her isteğe takip numarası ata - En başa yakın olmalı)
app.UseMiddleware<CorrelationIdMiddleware>();

// Serilog Request Logging (HTTP isteklerini logla)
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS Middleware'i (Authorization'dan ÖNCE olmalı)
app.UseCors("AllowReactApp");

app.UseAuthorization();

// Health Check Endpoint'i
app.MapHealthChecks("/health");

app.MapControllers();

// ============================================================
// 6. OTOMATİK MIGRATION (Self-Healing) 🚑
// ============================================================
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<BusXDbContext>();
//        context.Database.Migrate(); // Veritabanı yoksa oluştur, varsa güncelle
//        Log.Information("✅ Veritabanı başarıyla güncellendi/kontrol edildi.");
//    }
//    catch (Exception ex)
//    {
//        Log.Error(ex, "❌ Veritabanı başlatılırken hata oluştu.");
//    }
//}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BusXDbContext>();

        // ⚠️ DİKKAT: Bu iki satır her çalıştırmada veritabanını sıfırlar!
        // Böylece SeedData'daki 60 günlük verinin yüklendiğinden %100 emin oluruz.
        context.Database.EnsureDeleted(); // Varsa sil
        context.Database.EnsureCreated(); // Sıfırdan oluştur ve Seed Data'yı bas

        Log.Information("✅ Veritabanı sıfırlandı ve 60 günlük demo verisi yüklendi.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Veritabanı başlatılırken hata oluştu.");
    }
}

app.Run();