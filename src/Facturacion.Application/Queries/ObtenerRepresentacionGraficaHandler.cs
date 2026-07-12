using Facturacion.Domain.Common;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Queries;

/// <summary>Genera el PDF (representación gráfica) de una factura ya validada.</summary>
public class ObtenerRepresentacionGraficaHandler
{
    private readonly IFacturaRepository _facturas;
    private readonly ITenantRepository _tenants;
    private readonly IGeneradorRepresentacionGrafica _generador;

    public ObtenerRepresentacionGraficaHandler(
        IFacturaRepository facturas, ITenantRepository tenants, IGeneradorRepresentacionGrafica generador)
    {
        _facturas = facturas;
        _tenants = tenants;
        _generador = generador;
    }

    /// <summary>Null si la factura no existe. Lanza <see cref="DomainException"/> si todavía no tiene CUF.</summary>
    public async Task<byte[]?> HandleAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerAsync(tenantId, facturaId, ct);
        if (factura is null) return null;

        if (factura.Cuf is null)
            throw new DomainException(
                "FACTURA_SIN_CUF", "La factura todavía no tiene CUF (no fue validada por el SIN).");

        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");
        var sucursal = tenant.Sucursales.Single(s => s.Id == factura.SucursalId);
        var puntoVenta = sucursal.PuntosVenta.SingleOrDefault(p => p.Id == factura.PuntoVentaId);

        return await _generador.GenerarPdfAsync(factura, tenant, sucursal, puntoVenta, ct);
    }
}
