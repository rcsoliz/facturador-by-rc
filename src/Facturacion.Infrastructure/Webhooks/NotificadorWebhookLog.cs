using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infrastructure.Webhooks;

/// <summary>
/// Implementación provisional de <see cref="INotificadorWebhook"/>: solo loguea
/// lo que se hubiera enviado, sin HTTP real ni firma HMAC. Existe para no bloquear
/// el flujo completo (API → cola → worker → webhook) mientras no se implementa el
/// item de roadmap "Webhooks firmados (HMAC) con reintentos".
/// TODO(claude-code): reemplazar por un notificador HTTP real con firma HMAC del
/// payload y reintentos (Polly), usando Tenant.WebhookUrl.
/// </summary>
public class NotificadorWebhookLog : INotificadorWebhook
{
    private readonly ILogger<NotificadorWebhookLog> _logger;

    public NotificadorWebhookLog(ILogger<NotificadorWebhookLog> logger) => _logger = logger;

    public Task NotificarCambioEstadoAsync(Tenant tenant, Factura factura, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[webhook simulado] tenant={TenantId} webhookUrl={WebhookUrl} facturaId={FacturaId} " +
            "referenciaExterna={ReferenciaExterna} estado={Estado} cuf={Cuf}",
            tenant.Id, tenant.WebhookUrl, factura.Id, factura.ReferenciaExterna,
            factura.Estado, factura.Cuf?.Valor);

        return Task.CompletedTask;
    }
}
