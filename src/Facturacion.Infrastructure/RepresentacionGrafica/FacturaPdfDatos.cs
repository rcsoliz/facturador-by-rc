namespace Facturacion.Infrastructure.RepresentacionGrafica;

public sealed record FacturaPdfDatos(
    string NitEmisor,
    string RazonSocialEmisor,
    string Municipio,
    string Direccion,
    int CodigoSucursal,
    int? CodigoPuntoVenta,
    long NumeroFactura,
    string Cuf,
    DateTime FechaEmision,
    string NombreRazonSocialComprador,
    string NumeroDocumentoComprador,
    string? Complemento,
    decimal MontoTotal,
    int CodigoMoneda,
    string Leyenda,
    string UrlVerificacionQr,
    IReadOnlyList<FacturaPdfDetalle> Detalles);

public sealed record FacturaPdfDetalle(
    string CodigoProducto,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal MontoDescuento,
    decimal SubTotal);
