# 🚌 BusX - Otobüs Biletleme Sistemi (Full-Stack Case Study)

![Status](https://img.shields.io/badge/Status-Production%20Ready-success?style=for-the-badge)
![Backend](https://img.shields.io/badge/.NET%208.0-Web%20API-purple?style=for-the-badge)
![Frontend](https://img.shields.io/badge/React%2018-TypeScript-blue?style=for-the-badge)
![Database](https://img.shields.io/badge/SQLite-EF%20Core-green?style=for-the-badge)

**Atlas Yazılım** Kıdemli .NET Developer pozisyonu için geliştirilmiş; yüksek performanslı, ölçeklenebilir ve özellikle **Eşzamanlılık (Concurrency)** sorunlarını çözen modern bir biletleme altyapısıdır.

Bu proje, standart bir CRUD uygulaması değildir; gerçek hayat senaryolarındaki **Race Condition** (Yarış Durumu), **Transaction Yönetimi** ve **Clean Architecture** prensiplerini uygulayan kapsamlı bir mühendislik çalışmasıdır.

---

## 🏗️ Mimari ve Teknik Özellikler

Proje, S.O.L.I.D. prensiplerine sadık kalınarak **Onion Architecture** (Soğan Mimarisi) deseninde geliştirilmiştir.

### 🔙 Backend (.NET 8 Web API)
* **Clean Architecture:** `Core` (Domain), `Infrastructure` (Data) ve `API` (Presentation) katmanları ile tam bağımlılık yönetimi.
* **Optimistic Concurrency Control:** Aynı koltuğun aynı milisaniyede iki farklı kullanıcıya satılmasını önleyen, `RowVersion` tabanlı kilit mekanizması.
* **Self-Healing Database:** Uygulama her başlatıldığında veritabanını otomatik kontrol eder, sıfırlar ve **81 il için 60 günlük** gerçekçi demo verisi üretir.
* **Akıllı Sefer Motoru:** "Bugün" yapılan aramalarda saati geçmiş seferleri gizler, ileri tarihli aramalarda tüm gün planını gösterir.
* **Strategy Pattern:** Farklı sağlayıcılar (ProviderA/B) için dinamik fiyatlandırma algoritmaları içerir.
* **Travego Koltuk Düzeni:** Koltuklar veritabanında standart bir döngüyle değil; kapı boşlukları, koridor hizalamaları ve 2+1 düzeniyle (Travego stili) dinamik olarak oluşturulur.
* **Observability:** `Serilog` ile yapısal loglama, `Correlation-ID` ile uçtan uca istek takibi ve Health Check endpointleri.

### ⚛️ Frontend (React + TypeScript)
* **Modern Stack:** Vite, React 18 ve TypeScript ile tip güvenli, hızlı geliştirme.
* **Dinamik UI:** Koltuk haritası (`Seat Map`) statik bir resim değil, Backend'den gelen koordinat verisine göre çizilen interaktif bir bileşendir.
* **Responsive Design:** Bootstrap 5 ile mobil uyumlu arayüz.
* **Service Layer:** API istekleri merkezi ve modüler bir yapıda (`api.ts`) yönetilir.

---

## 🚀 Kurulum ve Çalıştırma (Plug & Play)

Projeyi çalıştırmak için SQL Server kurulumuna veya karmaşık konfigürasyonlara **ihtiyaç yoktur**. Proje, taşınabilir **SQLite** veritabanı ile gelir ve kendini otomatik kurar.

### Gereksinimler
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Node.js](https://nodejs.org/) (Frontend için)

### 1. Backend'i Ayağa Kaldırma (API)

Terminali projenin **ana dizininde** (`BusX_System` klasöründe) açın ve şu komutu çalıştırın:

```bash
dotnet run --project BusX.API/BusX.API.csproj

Not: İlk açılışta sistem BusX.db dosyasını oluşturup içine 81 il ve binlerce sefer eklediği için açılması 5-10 saniye sürebilir. Terminalde Now listening on: https://localhost:7061 yazısını gördüğünüzde hazırdır.

Swagger UI: https://localhost:7061/swagger

Health Check: https://localhost:7061/health

2. Frontend'i Başlatma (Web Arayüzü)
Yeni bir terminal penceresi açın, Web klasörüne gidin ve başlatın:

Bash

cd BusX.Web
npm install  # (Sadece ilk kurulumda gereklidir)
npm run dev
Uygulama Adresi: http://localhost:5173

🧪 Test Senaryoları (Reviewer İçin)
Sistemin yeteneklerini ve sağlamlığını test etmek için aşağıdaki adımları izleyebilirsiniz:

Senaryo 1: Sefer Arama (Data Generation Testi)
Arayüzden İstanbul -> Ankara (veya İzmir -> Antalya) seçimini yapın.

Tarih sekmelerinden **"Bugün"**ü seçin. Saat 18:00 ise, sadece 18:00 sonrası seferlerin listelendiğini doğrulayın.

"Yarın" sekmesine tıklayın. Sabah 09:00'dan itibaren tüm seferlerin listelendiğini doğrulayın.

Senaryo 2: Koltuk Planı (Görsel Test)
Listelenen seferlerden birinde "Koltuk Seç" butonuna tıklayın.

Açılan ekranda 2+1 Travego düzenini kontrol edin:

Sol taraf tekli koltuklar.

Sağ taraf ikili koltuklar.

Orta sıralarda (7-8. sıra) KAPI boşluğunun olduğunu ve numara atlamasını (19...22) doğrulayın.

Senaryo 3: Concurrency (Çakışma) Testi 🔥
Bu test, sistemin "Race Condition" durumunda veri bütünlüğünü nasıl koruduğunu gösterir.

Swagger'ı açın (/api/tickets/checkout).

Aşağıdaki JSON verisiyle aynı seferin aynı koltuğuna (Örn: Koltuk No 10) iki farklı istek hazırlayın.

İki isteği de "Execute" butonuna basarak arka arkaya gönderin.

JSON

{
  "journeyId": 1,
  "seats": [
    { "seatId": 10, "passengerName": "Test User", "passengerTc": "111", "gender": 1 }
  ]
}
Sonuç: İlk istek 200 Success dönerken, ikinci istek 409 Conflict dönecek ve "Koltuk zaten satılmış" hatası verecektir.

📂 Proje Klasör Yapısı
BusX_System/
├── BusX.Core/            # Domain Layer (Varlıklar, Interface'ler, DTO'lar) - Saf C#
├── BusX.Infrastructure/  # Data Layer (EF Core, Services, Data Seeding)
├── BusX.API/             # Presentation Layer (Controllers, Middlewares, Serilog)
└── BusX.Web/             # Frontend (React, Vite, Bootstrap, TypeScript)
Geliştirici: İbrahim Kemal Koyuncu Teslim Tarihi: 29.11.2025
