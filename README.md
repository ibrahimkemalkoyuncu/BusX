# 🚌 BusX - Otobüs Biletleme Sistemi (Case Study)

Atlas Yazılım için geliştirilen, yüksek performanslı, ölçeklenebilir ve eşzamanlılık (concurrency) sorunlarını çözen modern bir otobüs biletleme altyapısıdır.

## 🚀 Proje Durumu: Modül 4 Tamamlandı (Backend Bitti)
Backend geliştirmesi tamamlanmış olup, proje şu yeteneklere sahiptir:

### 🏗️ Mimari & Teknolojiler
* **.NET 8 Web API:** Backend motoru.
* **Clean Architecture:** Core, Infrastructure ve API katmanlı yapı.
* **SQLite & EF Core:** Veritabanı ve ORM.
* **Serilog & Structured Logging:** Dosya tabanlı, yapısal loglama sistemi.
* **Correlation ID:** Her isteğin benzersiz bir kimlikle (GUID) uçtan uca takibi.
* **Health Checks:** Sistem ve veritabanı sağlık durumu izleme.
* **Optimistic Concurrency Control:** Çifte rezervasyon engelleme.
* **Lazy Loading & Strategy Patterns:** Performans ve esneklik desenleri.

### 🔌 Endpoint'ler
| Metot | URL | Açıklama |
|-------|-----|----------|
| `GET` | `/api/journeys` | Sefer arama (Cache destekli). |
| `GET` | `/api/journeys/{id}/seats` | Seferin anlık koltuk durumu. |
| `POST` | `/api/tickets/checkout` | Güvenli bilet satışı (Concurrency Korumalı). |
| `GET` | `/health` | **(Yeni)** Sistem sağlık kontrolü (Status: Healthy). |

### 🔍 Gözlemlenebilirlik
* **Loglar:** `BusX.API/logs` klasöründe günlük olarak tutulur.
* **İzleme:** Her HTTP yanıtı `X-Correlation-Id` başlığı içerir.

---

## 🛠️ Kurulum ve Çalıştırma

1.  Repoyu klonlayın:
    ```bash
    git clone [https://github.com/ibrahimkemalkoyuncu/BusX.git](https://github.com/ibrahimkemalkoyuncu/BusX.git)
    ```
2.  Bağımlılıkları yükleyin:
    ```bash
    dotnet restore
    ```
3.  Uygulamayı başlatın (Veritabanı otomatik oluşur):
    ```bash
    dotnet run --project BusX.API/BusX.API.csproj
    ```
4.  Swagger: `http://localhost:5XXX/swagger`
5.  Health Check: `http://localhost:5XXX/health`

---
**Geliştirici:** İbrahim Kemal Koyuncu
