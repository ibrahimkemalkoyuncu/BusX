# 🚌 BusX - Otobüs Biletleme Sistemi (Case Study)

Atlas Yazılım için geliştirilen, yüksek performanslı, ölçeklenebilir ve eşzamanlılık (concurrency) sorunlarını çözen modern bir otobüs biletleme altyapısıdır.

## 🚀 Proje Durumu: Modül 2 Tamamlandı (Koltuk Planı)
Şu anki sürüm **"Modül 2"** olup, aşağıdaki özellikleri içerir:

### 🏗️ Mimari & Teknolojiler
* **.NET 8 Web API:** Backend motoru.
* **Clean Architecture:** Core, Infrastructure ve API katmanlı yapı.
* **SQLite & EF Core:** Veritabanı ve ORM.
* **Lazy Loading Pattern:** Koltuklar veritabanında peşinen değil, sefer ilk kez sorgulandığında dinamik olarak oluşturulur (Database Optimization).
* **Strategy Pattern:** Farklı sağlayıcılar (ProviderA/B) için dinamik fiyat hesaplama.
* **Concurrency Control:** (Hazırlık aşamasında) Optimistic Locking altyapısı.

### 🔌 Endpoint'ler
| Metot | URL | Açıklama |
|-------|-----|----------|
| `GET` | `/api/journeys` | Şehirler arası sefer arama (Cache destekli). |
| `GET` | `/api/journeys/{id}` | Sefer detayını getirme. |
| `GET` | `/api/journeys/{id}/seats` | **(Yeni)** Seferin anlık koltuk durumunu (Dolu/Boş) getirir. |
| `POST` | `/api/tickets/checkout` | **(Yeni)** Bilet satışı ve rezervasyon işlemi. |

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
