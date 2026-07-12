using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace Facturacion.Infrastructure.Webhooks;

/// <summary>
/// Reintentos + circuit breaker para el HttpClient nombrado
/// <see cref="NotificadorWebhookHttp.NombreCliente"/> — igual intención que
/// "Polly para resiliencia" en CLAUDE.md, aplicada acá a webhooks salientes
/// (hacia el sistema cliente) en vez de al SIN.
/// </summary>
public static class PoliticasWebhook
{
    /// <summary>3 reintentos con backoff exponencial (2s, 4s, 8s) ante errores transitorios o 429.</summary>
    public static IAsyncPolicy<HttpResponseMessage> Reintentos() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, intento => TimeSpan.FromSeconds(Math.Pow(2, intento)));

    /// <summary>Abre el circuito tras 5 fallos consecutivos, 30s antes de volver a intentar.</summary>
    public static IAsyncPolicy<HttpResponseMessage> CircuitBreaker() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
