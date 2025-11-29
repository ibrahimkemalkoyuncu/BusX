import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api';
import type { Seat } from '../models/types';

export default function SeatSelection() {
  const { id } = useParams(); // URL'den Sefer ID'sini al (journey/1)
  const navigate = useNavigate();

  // State Tanımları
  const [seats, setSeats] = useState<Seat[]>([]);
  const [selectedSeatIds, setSelectedSeatIds] = useState<number[]>([]);
  const [gender, setGender] = useState<number>(1); // 1: Erkek (Varsayılan), 2: Kadın
  const [passengerName, setPassengerName] = useState('');
  const [loading, setLoading] = useState(true);

  // Sayfa açılınca koltukları çek
  useEffect(() => {
    // Önce temizle
    setSeats([]); 
    
    api.get<Seat[]>(`/journeys/${id}/seats`)
      .then(res => {
        setSeats(res.data);
      })
      .catch(err => {
        console.error(err);
        alert("Koltuk verisi yüklenemedi. Backend çalışıyor mu?");
      })
      .finally(() => setLoading(false));
  }, [id]);

  // Koltuk Tıklama Mantığı
  const toggleSeat = (seat: Seat) => {
    if (seat.isSold) return; // Satılmışsa dokunma

    if (selectedSeatIds.includes(seat.id)) {
      // Zaten seçiliyse çıkar
      setSelectedSeatIds(prev => prev.filter(sid => sid !== seat.id));
    } else {
      // Yeni seçiliyse ekle (Max 4 kontrolü)
      if (selectedSeatIds.length >= 4) {
        alert("En fazla 4 koltuk seçebilirsiniz.");
        return;
      }
      setSelectedSeatIds(prev => [...prev, seat.id]);
    }
  };

  // Satın Al Butonu
  const handleCheckout = async () => {
    if (selectedSeatIds.length === 0) return alert("Lütfen koltuk seçiniz.");
    if (!passengerName) return alert("Lütfen yolcu adı giriniz.");

    const requestBody = {
      journeyId: Number(id),
      seats: selectedSeatIds.map(seatId => ({
        seatId: seatId,
        passengerName: passengerName,
        passengerTc: "11111111111", // Test için sabit TC
        gender: gender
      }))
    };

    try {
      const response = await api.post('/tickets/checkout', requestBody);
      alert(`✅ İşlem Başarılı! İyi yolculuklar.\nPNR Kodunuz: ${response.data.pnr}`);
      navigate('/'); // Ana sayfaya dön
    } catch (error: any) {
        // Backend'den gelen 409 (Conflict) veya 400 hatalarını göster
        const msg = error.response?.data?.message || "Satın alma başarısız.";
        alert("❌ HATA: " + msg);
        // Sayfayı yenile ki güncel durumu görsünler (belki başkası almıştır)
        window.location.reload();
    }
  };

  // Tek bir koltuğu çizen yardımcı fonksiyon
  const renderSeatButton = (seat: Seat | undefined) => {
    // Eğer koltuk yoksa (örn: kapı boşluğu veya koridor), boş div dön
    if (!seat) return <div style={{ width: '45px', height: '45px' }}></div>;

    let btnClass = "btn fw-bold border shadow-sm ";
    
    // Duruma göre renk belirle
    if (seat.isSold) {
        // Satılmışsa Cinsiyete göre renk (Opak)
        btnClass += seat.genderLock === 1 ? "btn-primary opacity-50" : "btn-danger opacity-50";
    } else if (selectedSeatIds.includes(seat.id)) {
        // Biz seçtiysek Yeşil
        btnClass += "btn-success text-white";
    } else {
        // Boşsa Gri Çerçeve
        btnClass += "btn-light text-dark border-secondary";
    }

    return (
      <button
        key={seat.id}
        className={btnClass}
        style={{ width: '45px', height: '45px' }}
        onClick={() => toggleSeat(seat)}
        disabled={seat.isSold}
        title={`Koltuk ${seat.seatNumber} - ${seat.isSold ? (seat.genderLock === 1 ? 'Bay Dolu' : 'Bayan Dolu') : 'Boş'}`}
      >
        {seat.seatNumber}
      </button>
    );
  };

  if (loading) return <div className="text-center mt-5 p-5"><h3>🚌 Otobüs planı yükleniyor...</h3></div>;

  // Seçilen koltukların toplam fiyatı
  const unitPrice = seats[0]?.price || 0;
  const totalPrice = unitPrice * selectedSeatIds.length;

  return (
    <div className="container mt-4 mb-5">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Koltuk Seçimi</h2>
        <button className='btn btn-outline-secondary' onClick={() => navigate(-1)}>🔙 Geri Dön</button>
      </div>
      
      <div className="row">
        {/* SOL: OTOBÜS PLANI (TRAVEGO DÜZENİ) */}
        <div className="col-lg-6 d-flex justify-content-center mb-4">
          <div className="bus-container bg-white p-4 rounded border border-2 shadow" style={{ width: '360px', minHeight: '600px' }}>
            
            {/* Şoför Mahalli */}
            <div className="d-flex justify-content-start mb-5 border-bottom pb-3">
               <div className="bg-secondary text-white rounded p-2 px-3 shadow-sm">
                 👮 Kaptan
               </div>
            </div>

            {/* Koltuk Grid Yapısı */}
            <div className="d-flex flex-column gap-2">
              {/* 1'den 14'e kadar satırları dönüyoruz */}
              {Array.from({ length: 14 }, (_, i) => i + 1).map(rowNum => {
                
                const rowSeats = seats.filter(s => s.row === rowNum);
                if (rowSeats.length === 0) return null; // Boş satır varsa çizme

                // Sütunlara göre koltukları bul (Backend'de tanımladığımız Column ID'ler)
                const leftSeat = rowSeats.find(s => s.column === 1);      // Sol Tekli
                const rightInner = rowSeats.find(s => s.column === 4);    // Sağ Koridor Yanı
                const rightWindow = rowSeats.find(s => s.column === 5);   // Sağ Cam Kenarı

                return (
                  <div key={rowNum} className="d-flex justify-content-between align-items-center">
                    {/* SOL TARAFTAKİ KOLTUK (Veya Boşluk) */}
                    <div className="me-4">
                      {renderSeatButton(leftSeat)}
                    </div>
                    
                    {/* KORİDOR BOŞLUĞU */}
                    <div style={{ width: '30px', textAlign: 'center', fontSize: '10px', color: '#ccc' }}>
                        {rowNum === 7 ? 'KAPI' : ''} 
                    </div>

                    {/* SAĞ TARAFTAKİ İKİLİ (Veya Kapı Boşluğu) */}
                    <div className="d-flex gap-2">
                      {renderSeatButton(rightInner)}
                      {renderSeatButton(rightWindow)}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* SAĞ: ÖDEME FORMU */}
        <div className="col-lg-5 offset-lg-1">
          <div className="card shadow border-0 sticky-top" style={{ top: '20px' }}>
            <div className="card-header bg-primary text-white">
                <h5 className="mb-0">Bilet İşlemleri</h5>
            </div>
            <div className="card-body p-4">
                
                {/* Cinsiyet Seçimi */}
                <div className="mb-4">
                    <label className="form-label fw-bold">Yolcu Cinsiyeti</label>
                    <div className="btn-group w-100" role="group">
                        <button 
                            type="button" 
                            className={`btn ${gender === 1 ? 'btn-primary' : 'btn-outline-primary'}`} 
                            onClick={() => setGender(1)}
                        >
                            Erkek 👨
                        </button>
                        <button 
                            type="button" 
                            className={`btn ${gender === 2 ? 'btn-danger' : 'btn-outline-danger'}`} 
                            onClick={() => setGender(2)}
                        >
                            Kadın 👩
                        </button>
                    </div>
                </div>
                
                {/* İsim Girişi */}
                <div className="mb-4">
                    <label className="form-label fw-bold">Yolcu Adı Soyadı</label>
                    <input 
                        type="text" 
                        className="form-control form-control-lg" 
                        value={passengerName}
                        onChange={e => setPassengerName(e.target.value)}
                        placeholder="Örn: Ahmet Yılmaz"
                    />
                </div>

                {/* Özet Bilgi */}
                <div className="alert alert-light border mb-4">
                    <div className="d-flex justify-content-between mb-2">
                        <span>Seçilen Koltuklar:</span>
                        <strong>
                            {seats.filter(s => selectedSeatIds.includes(s.id))
                                  .map(s => s.seatNumber)
                                  .join(', ') || '-'}
                        </strong>
                    </div>
                    <div className="d-flex justify-content-between text-success">
                        <span>Birim Fiyat:</span>
                        <strong>{unitPrice} ₺</strong>
                    </div>
                    <hr />
                    <div className="d-flex justify-content-between fs-4 fw-bold text-dark">
                        <span>TOPLAM:</span>
                        <span>{totalPrice} ₺</span>
                    </div>
                </div>

                {/* Satın Al Butonu */}
                <button 
                    className="btn btn-success w-100 btn-lg py-3 fw-bold shadow-sm" 
                    onClick={handleCheckout} 
                    disabled={selectedSeatIds.length === 0}
                >
                    {selectedSeatIds.length === 0 ? 'Koltuk Seçiniz' : `ÖDEME YAP (${totalPrice} ₺)`}
                </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}