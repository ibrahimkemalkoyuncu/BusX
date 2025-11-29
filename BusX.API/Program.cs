using BusX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BusX.Core.Interfaces;
using BusX.Infrastructure.Services;
using Serilog;
using BusX.API.Middlewares;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. SERILOG AYARLARI (Kara Kutu) 📝
// ============================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext() // Correlation ID buradan gelecek
    .WriteTo.Console()       // Konsola yaz
    .WriteTo.File("logs/busx-.txt", rollingInterval: RollingInterval.Day) // Dosyaya yaz (Günlük)
    .CreateLogger();

builder.Host.UseSerilog(); // .NET'in log mekanizmasını Serilog ile değiştir

// Add services to the container.

// Veritabanı
builder.Services.AddDbContext<BusXDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cache
builder.Services.AddMemoryCache();

// Servisler & Stratejiler
builder.Services.AddScoped<IPriceStrategy, ProviderAStrategy>();
builder.Services.AddScoped<IPriceStrategy, ProviderBStrategy>();
builder.Services.AddScoped<IJourneyService, JourneyService>();

// ============================================================
// 2. HEALTH CHECK (Sistem Nabzı) ❤️
// ============================================================
// Sadece "API ayakta mı?" diye bakmaz, "Veritabanına bağlanabiliyor muyum?" diye de bakar.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BusXDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================================================
// 3. MIDDLEWARE SIRALAMASI (Önemli!)
// ============================================================

// Correlation ID Middleware (En başa yakın olmalı)
app.UseMiddleware<CorrelationIdMiddleware>();

// Her isteği logla (Serilog Request Logging)
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Health Check Endpoint'i
app.MapHealthChecks("/health");

app.MapControllers();

// ============================================================
// 4. OTOMATİK MIGRATION
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BusXDbContext>();
        context.Database.Migrate();
        Log.Information("✅ Veritabanı başarıyla güncellendi.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Veritabanı başlatılırken hata oluştu.");
    }
}

app.Run();