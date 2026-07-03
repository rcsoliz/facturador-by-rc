using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.AnularFactura;

public class AnularFacturaHandler
{
    private readonly IFacturaRepository _facturas;
    private readonly IEncoladorEmision _encolador;

    public AnularFacturaHandler(IFacturaRepository facturas, IEncoladorEmision encolador)
    {
        _facturas = facturas;
        _encolador = encolador;
    }

    public async Task<FacturaResponse?> HandleAsync(
        Guid tenantId, Guid facturaId, int codigoMotivo, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerAsync(tenantId, facturaId, ct);
        if (factura is null) return null;

        // La anulación real contra el SIN la hace el worker; acá solo encolamos.
        await _encolador.EncolarAnulacionAsync(facturaId, codigoMotivo, ct);
        return FacturaResponse.Desde(factura);
    }
}
