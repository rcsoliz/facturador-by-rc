using Facturacion.Domain.Entities;

namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Arma <see cref="FacturaXmlDatos"/> combinando <see cref="Factura"/>, el
/// <see cref="Tenant"/> emisor (razón social, NIT), la <see cref="Sucursal"/>
/// (municipio, dirección, actividad económica) y el <see cref="PuntoVenta"/>,
/// más el CUF/CUFD ya calculados y los valores fijos de <see cref="SiatOptions"/>.
/// Compartido entre <c>SiatFakeAdapter</c> y el futuro <c>SiatComputarizadaAdapter</c>
/// para no duplicar este mapeo — ver Siat/Common/README.md.
/// </summary>
public static class FacturaXmlDatosFactory
{
    public static FacturaXmlDatos Crear(
        Factura factura, Tenant tenant, Sucursal sucursal, PuntoVenta? puntoVenta,
        string cuf, string cufd, SiatOptions opciones)
    {
        return new FacturaXmlDatos(
            NitEmisor: tenant.Nit.Valor,
            RazonSocialEmisor: tenant.RazonSocial,
            Municipio: sucursal.Municipio,
            Telefono: null,
            NumeroFactura: factura.NumeroFactura,
            Cuf: cuf,
            Cufd: cufd,
            CodigoSucursal: sucursal.CodigoSiat,
            Direccion: sucursal.Direccion,
            CodigoPuntoVenta: puntoVenta?.CodigoSiat,
            FechaEmision: factura.FechaEmision,
            NombreRazonSocial: factura.RazonSocialComprador,
            CodigoTipoDocumentoIdentidad: factura.CodigoTipoDocumentoIdentidad,
            NumeroDocumento: factura.NumeroDocumentoComprador,
            Complemento: factura.Complemento,
            CodigoCliente: factura.NumeroDocumentoComprador,
            CodigoMetodoPago: factura.CodigoMetodoPago,
            NumeroTarjeta: factura.NumeroTarjeta,
            MontoTotal: factura.MontoTotal,
            MontoTotalSujetoIva: factura.MontoTotalSujetoIva,
            CodigoMoneda: factura.CodigoMoneda,
            TipoCambio: factura.TipoCambio,
            MontoTotalMoneda: factura.MontoTotal * factura.TipoCambio,
            MontoGiftCard: null,
            DescuentoAdicional: null,
            CodigoExcepcion: null,
            Cafc: null,
            Leyenda: opciones.LeyendaFacturaDefault,
            Usuario: opciones.UsuarioSistema,
            CodigoDocumentoSector: factura.CodigoDocumentoSector,
            Detalles: factura.Detalles.Select(d => new FacturaXmlDetalle(
                ActividadEconomica: sucursal.ActividadEconomica,
                CodigoProductoSin: d.CodigoProductoSin,
                CodigoProducto: d.CodigoProducto,
                Descripcion: d.Descripcion,
                Cantidad: d.Cantidad,
                UnidadMedida: d.UnidadMedida,
                PrecioUnitario: d.PrecioUnitario,
                MontoDescuento: d.MontoDescuento,
                SubTotal: d.SubTotal,
                NumeroSerie: null,
                NumeroImei: null)).ToList());
    }
}
