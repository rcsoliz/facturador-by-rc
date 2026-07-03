using Facturacion.Domain.Entities;

namespace Facturacion.Domain.Ports;

/// <summary>
/// Notifica al sistema cliente los cambios de estado de sus facturas
/// (validada, rechazada, anulada). Implementación en Infrastructure con
/// firma HMAC del payload y reintentos.
/// </summary>
public interface INotificadorWebhook
{
    Task NotificarCambioEstadoAsync(Tenant tenant, Factura factura, CancellationToken ct = default);
}
