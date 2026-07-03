using Facturacion.Application.Dtos;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Queries;

public class ConsultarFacturaHandler
{
    private readonly IFacturaRepository _facturas;

    public ConsultarFacturaHandler(IFacturaRepository facturas) => _facturas = facturas;

    public async Task<FacturaResponse?> HandleAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerAsync(tenantId, facturaId, ct);
        return factura is null ? null : FacturaResponse.Desde(factura);
    }
}
