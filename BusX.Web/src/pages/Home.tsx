import { useState, useEffect } from 'react';
import api from '../api';
import type { Journey } from '../models/types';
import { useNavigate } from 'react-router-dom';

export default function Home() {
  const navigate = useNavigate();

  // Şehir ID'leri (İstanbul=34, Ankara=6 Veritabanına göre sabitlendi)
  const [fromId, setFromId] = useState(34); 
  const [toId, setToId] = useState(6);
  
  // Varsayılan Tarih: BUGÜN
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);

  const [journeys, setJourneys] = useState<Journey[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Sayfa ilk açıldığında veya Tarih/Şehir değiştiğinde OTOMATİK ARA
  useEffect(() => {
    searchJourneys();
  }, [date, fromId, toId]);

  const searchJourneys = async () => {
    setLoading(true);
    setError('');
    setJourneys([]);

    try {
      const response = await api.get<Journey[]>('/journeys', {
        params: { fromId, toId, date }
      });
      setJourneys(response.data);
      if (response.data.length === 0) setError("Bu tarihte sefer bulunamadı.");
    } catch (err) {
      console.error(err);
      setError("Bağlantı hatası.");
    } finally {
      setLoading(false);
    }
  };

  // Tarih Sekmeleri Oluşturucu (Bugün + 4 gün)
  const renderDateTabs = () => {
    const tabs = [];
    for (let i = 0; i < 5; i++) {
        const d = new Date();
        d.setDate(d.getDate() + i);
        const dateStr = d.toISOString().split('T')[0];
        
        // Gün ismini bul (Pzt, Sal...)
        const dayName = d.toLocaleDateString('tr-TR', { weekday: 'short' });
        const dayNum = d.getDate();

        const isActive = date === dateStr;

        tabs.push(
            <button 
                key={i}
                className={`btn me-2 ${isActive ? 'btn-primary' : 'btn-outline-secondary'}`}
                style={{ minWidth: '80px' }}
                onClick={() => setDate(dateStr)}
            >
                <div style={{ fontSize: '12px' }}>{dayName}</div>
                <div style={{ fontSize: '18px', fontWeight: 'bold' }}>{dayNum}</div>
            </button>
        );
    }
    return tabs;
  };

  return (
    <div className="container mt-5">
      <h1 className="text-center mb-4 text-primary fw-bold">🚌 BusX Bilet</h1>

      {/* ARAMA KUTUSU */}
      <div className="card p-4 shadow-sm mb-4 border-0 bg-light">
        <div className="row g-3">
          <div className="col-md-5">
            <label className="form-label fw-bold text-muted">Nereden</label>
            <select className="form-select form-select-lg" value={fromId} onChange={e => setFromId(Number(e.target.value))}>
              <option value={34}>İstanbul</option>
              <option value={6}>Ankara</option>
              <option value={35}>İzmir</option>
              <option value={1}>Adana</option>
              <option value={7}>Antalya</option>
              <option value={16}>Bursa</option>
            </select>
          </div>
          <div className="col-md-2 d-flex align-items-center justify-content-center">
            <span className="fs-3 text-muted">➡️</span>
          </div>
          <div className="col-md-5">
            <label className="form-label fw-bold text-muted">Nereye</label>
            <select className="form-select form-select-lg" value={toId} onChange={e => setToId(Number(e.target.value))}>
              <option value={6}>Ankara</option>
              <option value={34}>İstanbul</option>
              <option value={35}>İzmir</option>
              <option value={1}>Adana</option>
              <option value={7}>Antalya</option>
            </select>
          </div>
        </div>
      </div>

      {/* TARİH SEKMELERİ (TABS) */}
      <div className="d-flex justify-content-center mb-4 overflow-auto pb-2">
        {renderDateTabs()}
        {/* Takvim Butonu (Daha ileri tarih için) */}
        <div className="ms-2">
            <input 
                type="date" 
                className="form-control" 
                style={{ height: '58px' }}
                value={date}
                onChange={e => setDate(e.target.value)}
            />
        </div>
      </div>

      {/* YÜKLENİYOR / HATA / SONUÇLAR */}
      {loading && <div className="text-center my-5"><div className="spinner-border text-primary"></div></div>}
      
      {!loading && error && <div className="alert alert-warning text-center shadow-sm">{error}</div>}

      <div className="list-group">
        {journeys.map(j => (
          <div 
            key={j.id} 
            className="card mb-3 border-0 shadow-sm hover-shadow"
            onClick={() => navigate(`/journey/${j.id}`)} 
            style={{ cursor: 'pointer', transition: 'transform 0.2s' }}
            onMouseEnter={(e) => e.currentTarget.style.transform = 'scale(1.01)'}
            onMouseLeave={(e) => e.currentTarget.style.transform = 'scale(1)'}
          >
            <div className="card-body d-flex justify-content-between align-items-center p-4">
                {/* Sol: Logo ve Saat */}
                <div className="d-flex align-items-center">
                    <div className="me-4 text-center">
                        <div className="fw-bold fs-4">{new Date(j.departure).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</div>
                        <div className="small text-muted">Hareket</div>
                    </div>
                    <div>
                        <h5 className="mb-0 text-primary fw-bold">{j.providerName}</h5>
                        <small className="text-muted">{j.fromCity} ➡️ {j.toCity}</small>
                    </div>
                </div>

                {/* Sağ: Fiyat ve Ok */}
                <div className="text-end">
                    <h3 className="text-success fw-bold mb-0">{j.price} ₺</h3>
                    <button className="btn btn-sm btn-outline-primary mt-2 rounded-pill px-4">
                        Koltuk Seç ➜
                    </button>
                </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}