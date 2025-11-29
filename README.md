# 🚌 BusX - Otobüs Biletleme Sistemi (Case Study)

Atlas Yazılım için geliştirilen, yüksek performanslı, ölçeklenebilir ve eşzamanlılık (concurrency) sorunlarını çözen modern bir otobüs biletleme altyapısıdır.

## 🚀 Proje Durumu: Modül 3 Tamamlandı (Satış & Concurrency)
Şu anki sürüm **"Modül 3"** olup, aşağıdaki kritik özellikleri içerir:

### 🏗️ Mimari & Teknolojiler
* **.NET 8 Web API:** Backend motoru.
* **Clean Architecture:** Core, Infrastructure ve API katmanlı yapı.
* **SQLite & EF Core:** Veritabanı ve ORM.
* **Optimistic Concurrency Control:** Aynı koltuğun aynı anda iki kişiye satılmasını önleyen kilit mekanizması (`RowVersion`).
* **Transaction Management:** Satış ve ödeme işlemlerinin atomik (ya hep ya hiç) olarak yönetilmesi.
* **Lazy Loading Pattern:** Koltuklar dinamik oluşturulur.
* **Strategy Pattern:** Sağlayıcı bazlı fiyatlandırma.

### 🔌 Endpoint'ler
| Metot | URL | Açıklama |
|-------|-----|----------|
| `GET` | `/api/journeys` | Şehirler arası sefer arama (Cache destekli). |
| `GET` | `/api/journeys/{id}` | Sefer detayını getirme. |
| `GET` | `/api/journeys/{id}/seats` | Seferin anlık koltuk durumunu (Dolu/Boş) getirir. |
| `POST` | `/api/tickets/checkout` | **(Yeni)** Güvenli bilet satışı. Eşzamanlılık kontrolü ve Mock ödeme içerir. |

### 🧪 Test Senaryoları
* **Mock Ödeme:** %10 ihtimalle ödeme reddedilir (402 Payment Required).
* **Çifte Rezervasyon:** Aynı koltuğa aynı anda gelen isteklerden sadece biri başarılı olur, diğeri reddedilir (409 Conflict).

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
