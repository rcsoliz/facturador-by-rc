namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Datos de entrada para <see cref="XmlFacturaBuilder"/>: un campo por cada elemento
/// del XSD oficial del SIN para factura de compra-venta computarizada
/// (<c>facturaComputarizadaCompraVenta.xsd</c>). No se lee directamente de
/// <see cref="Facturacion.Domain.Entities.Factura"/>: el futuro <c>SiatComputarizadaAdapter</c>
/// lo arma combinando <c>Factura</c> (incl. <c>CodigoMetodoPago</c>/<c>NumeroTarjeta</c>),
/// <c>Sucursal.ActividadEconomica</c> y <see cref="SiatOptions"/> (leyenda/usuario) —
/// igual que ya se hace con <see cref="CufDatos"/> para el CUF, ver Siat/Common/README.md.
/// Los campos nullable son los <c>nillable="true"</c> del XSD: si son null se emite
/// <c>xsi:nil="true"</c>; si no, el valor.
/// </summary>
public sealed record FacturaXmlDatos(
    string NitEmisor,
    string RazonSocialEmisor,
    string Municipio,
    string? Telefono,
    long NumeroFactura,
    string Cuf,
    string Cufd,
    int CodigoSucursal,
    string Direccion,
    int? CodigoPuntoVenta,
    DateTime FechaEmision,
    string? NombreRazonSocial,
    int CodigoTipoDocumentoIdentidad,
    string NumeroDocumento,
    string? Complemento,
    string CodigoCliente,
    int CodigoMetodoPago,
    long? NumeroTarjeta,
    decimal MontoTotal,
    decimal MontoTotalSujetoIva,
    int CodigoMoneda,
    decimal TipoCambio,
    decimal MontoTotalMoneda,
    decimal? MontoGiftCard,
    decimal? DescuentoAdicional,
    int? CodigoExcepcion,
    string? Cafc,
    string Leyenda,
    string Usuario,
    int CodigoDocumentoSector,
    IReadOnlyList<FacturaXmlDetalle> Detalles);

/// <summary>Un ítem (elemento <c>detalle</c>) de la factura, según el mismo XSD.</summary>
public sealed record FacturaXmlDetalle(
    string ActividadEconomica,
    int CodigoProductoSin,
    string CodigoProducto,
    string Descripcion,
    decimal Cantidad,
    int UnidadMedida,
    decimal PrecioUnitario,
    decimal MontoDescuento,
    decimal SubTotal,
    string? NumeroSerie,
    string? NumeroImei);
