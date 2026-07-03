using Facturacion.Domain.Entities;

namespace Facturacion.Application.Dtos;

public sealed record FacturaResponse(
    Guid Id,
    string ReferenciaExterna,
    string Estado,
    long? NumeroFactura,
    string? Cuf,
    decimal MontoTotal,
    DateTime FechaEmision,
    string? MotivoRechazo)
{
    public static FacturaResponse Desde(Factura f) => new(
        f.Id,
        f.ReferenciaExterna,
        f.Estado.ToString(),
        f.NumeroFactura == 0 ? null : f.NumeroFactura,
        f.Cuf?.Valor,
        f.MontoTotal,
        f.FechaEmision,
        f.MotivoRechazo);
}
