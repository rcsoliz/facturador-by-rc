namespace Facturacion.Application.Dtos;

/// <summary>
/// Contrato público de emisión — lo que ve el sistema cliente.
/// Nota: el cliente NO envía CUF, CUFD, número de factura ni nada del SIN.
/// </summary>
public sealed record EmitirFacturaRequest(
    string ReferenciaExterna,          // idempotencia: misma referencia => misma factura
    Guid SucursalId,
    Guid PuntoVentaId,
    int CodigoDocumentoSector,
    CompradorDto Comprador,
    int CodigoMoneda,
    decimal TipoCambio,
    int CodigoMetodoPago,              // paramétrica SIAT: 1=Efectivo, etc.
    long? NumeroTarjeta,                // solo si el método de pago lo requiere
    IReadOnlyList<DetalleDto> Detalles);

public sealed record CompradorDto(
    string RazonSocial,
    int CodigoTipoDocumentoIdentidad,  // paramétrica SIAT: 1=CI, 5=NIT, etc.
    string NumeroDocumento,
    string? Complemento,
    string? Email);

public sealed record DetalleDto(
    int CodigoProductoSin,
    string CodigoProducto,
    string Descripcion,
    decimal Cantidad,
    int UnidadMedida,
    decimal PrecioUnitario,
    decimal MontoDescuento);
