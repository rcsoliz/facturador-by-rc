using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;

namespace Facturacion.Domain.Ports;

public interface IFacturaRepository
{
    Task<Factura?> ObtenerAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default);
    Task<Factura?> ObtenerPorReferenciaAsync(Guid tenantId, string referenciaExterna, CancellationToken ct = default);
    Task<IReadOnlyList<Factura>> ListarPorEstadoAsync(Guid tenantId, EstadoFactura estado, CancellationToken ct = default);
    Task<long> SiguienteNumeroAsync(Guid puntoVentaId, CancellationToken ct = default);
    Task AgregarAsync(Factura factura, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
