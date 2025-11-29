import axios from 'axios';

// Backend'in launchSettings.json'daki HTTPS portu: 7061
const api = axios.create({
    baseURL: 'https://localhost:7061/api', 
    headers: {
        'Content-Type': 'application/json',
        // Backend'deki CorrelationIdMiddleware için takip numarası (Opsiyonel)
        'X-Correlation-Id': crypto.randomUUID() 
    }
});

export default api;