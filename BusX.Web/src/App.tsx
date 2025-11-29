import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import SeatSelection from './pages/SeatSelection'; // <-- İMPORT ET

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        {/* ARTIK GERÇEK SAYFAYA GİDİYORUZ */}
        <Route path="/journey/:id" element={<SeatSelection />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App