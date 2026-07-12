using Facturacion.Domain.Entities;

namespace Facturacion.Domain.Ports;

/// <summary>Genera el PDF con QR de la factura (representación gráfica SIAT).</summary>
public interface IGeneradorRepresentacionGrafica
{
    Task<byte[]> GenerarPdfAsync(
        Factura factura, Tenant tenant, Sucursal sucursal, PuntoVenta? puntoVenta, CancellationToken ct = default);
}
