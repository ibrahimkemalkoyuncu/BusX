using Serilog.Context;

namespace BusX.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // 1. İstekte ID var mı? Yoksa yeni oluştur.
            string correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString();

            // 2. Cevap başlığına (Response Header) bu ID'yi ekle ki istemci de bilsin.
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

            // 3. Serilog'un "LogContext"ine bu ID'yi itiyoruz.
            // Artık bu blok içindeki TÜM loglarda bu ID otomatik görünecek.
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context); // Bir sonraki aşamaya geç (Controller'a git)
            }
        }
    }
}