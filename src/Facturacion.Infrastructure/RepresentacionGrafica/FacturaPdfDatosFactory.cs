using Facturacion.Domain.Entities;
using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Infrastructure.RepresentacionGrafica;

/// <summary>
/// Arma <see cref="FacturaPdfDatos"/> combinando <see cref="Factura"/> +
/// <see cref="Tenant"/> + <see cref="Sucursal"/> + <see cref="PuntoVenta"/> +
/// <see cref="SiatOptions"/> — mismo patrón que <c>FacturaXmlDatosFactory</c>,
/// pero solo con los campos que la representación gráfica necesita mostrar
/// (no los que existen únicamente para el XSD del XML, como Cafc/CodigoExcepcion).
/// </summary>
public static class FacturaPdfDatosFactory
{
    public static FacturaPdfDatos Crear(Factura factura, Tenant tenant, Sucursal sucursal, PuntoVenta? puntoVenta, SiatOptions opciones)
    {
        if (factura.Cuf is null)
            throw new InvalidOperationException($"La factura {factura.Id} todavía no tiene CUF.");

        return new FacturaPdfDatos(
            NitEmisor: tenant.Nit.Valor,
            RazonSocialEmisor: tenant.RazonSocial,
            Municipio: sucursal.Municipio,
            Direccion: sucursal.Direccion,
            CodigoSucursal: sucursal.CodigoSiat,
            CodigoPuntoVenta: puntoVenta?.CodigoSiat,
            NumeroFactura: factura.NumeroFactura,
            Cuf: factura.Cuf.Valor,
            FechaEmision: factura.FechaEmision,
            NombreRazonSocialComprador: factura.RazonSocialComprador,
            NumeroDocumentoComprador: factura.NumeroDocumentoComprador,
            Complemento: factura.Complemento,
            MontoTotal: factura.MontoTotal,
            CodigoMoneda: factura.CodigoMoneda,
            Leyenda: opciones.LeyendaFacturaDefault,
            UrlVerificacionQr: ArmarUrlQr(opciones.UrlBaseVerificacionQr, tenant.Nit.Valor, factura.Cuf.Valor, factura.NumeroFactura),
            Detalles: factura.Detalles.Select(d => new FacturaPdfDetalle(
                d.CodigoProducto, d.Descripcion, d.Cantidad, d.PrecioUnitario, d.MontoDescuento, d.SubTotal)).ToList());
    }

    private static string ArmarUrlQr(string urlBase, string nit, string cuf, long numeroFactura) =>
        $"{urlBase}?nit={Uri.EscapeDataString(nit)}&cuf={Uri.EscapeDataString(cuf)}&numero={numeroFactura}";
}
