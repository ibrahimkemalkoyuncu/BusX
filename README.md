# 🚌 BusX - Otobüs Biletleme Sistemi (Case Study)

Atlas Yazılım için geliştirilen, yüksek performanslı, ölçeklenebilir ve eşzamanlılık (concurrency) sorunlarını çözen modern bir otobüs biletleme altyapısıdır.

## 🚀 Proje Durumu: Modül 1 Tamamlandı
Şu anki sürüm **"Modül 1"** olup, aşağıdaki temel yapıtaşlarını içerir:

### 🏗️ Mimari & Teknolojiler
* **.NET 8 Web API:** Backend motoru.
* **Clean Architecture:** Core, Infrastructure ve API katmanlı yapı.
* **SQLite & EF Core:** Veritabanı ve ORM (Code-First yaklaşımı).
* **Strategy Pattern:** Farklı sağlayıcılar (ProviderA/B) için dinamik fiyat hesaplama.
* **Self-Healing Database:** Uygulama başlangıcında otomatik migration ve veritabanı kurulumu.
* **InMemory Caching:** Sefer aramaları için performans optimizasyonu.

### 🔌 Endpoint'ler (Modül 1)
| Metot | URL | Açıklama |
|-------|-----|----------|
| `GET` | `/api/journeys` | Şehirler arası sefer arama (Cache destekli). |
| `GET` | `/api/journeys/{id}` | Sefer detayını getirme. |

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
4.  Swagger Arayüzü:
    `http://localhost:5XXX/swagger` adresinden API'yi test edebilirsiniz.

---
**Geliştirici:** İbrahim Kemal Koyuncu
