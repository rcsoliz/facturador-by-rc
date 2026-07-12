using Facturacion.Domain.Entities;

namespace Facturacion.Infrastructure.Webhooks;

public sealed record WebhookFacturaPayload(
    Guid FacturaId,
    Guid TenantId,
    string ReferenciaExterna,
    string Estado,
    string? Cuf,
    long NumeroFactura,
    string? CodigoRecepcionSin,
    string? MotivoRechazo,
    DateTime FechaEmision)
{
    public static WebhookFacturaPayload Desde(Factura factura) => new(
        factura.Id,
        factura.TenantId,
        factura.ReferenciaExterna,
        factura.Estado.ToString(),
        factura.Cuf?.Valor,
        factura.NumeroFactura,
        factura.CodigoRecepcionSin,
        factura.MotivoRechazo,
        factura.FechaEmision);
}
