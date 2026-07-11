using Facturacion.Application.Dtos;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.EmitirFactura;

/// <summary>
/// Caso de uso: aceptar la solicitud de emisión.
/// Diseño asíncrono: acá solo se valida, se persiste en Pendiente y se encola.
/// El worker hace generar → enviar → validar y notifica por webhook.
///
/// TODO(claude-code): migrar a MediatR (IRequestHandler) al agregar el paquete.
/// </summary>
public class EmitirFacturaHandler
{
    private readonly IFacturaRepository _facturas;
    private readonly IEncoladorEmision _encolador;

    public EmitirFacturaHandler(IFacturaRepository facturas, IEncoladorEmision encolador)
    {
        _facturas = facturas;
        _encolador = encolador;
    }

    public async Task<FacturaResponse> HandleAsync(
        Guid tenantId, EmitirFacturaRequest request, CancellationToken ct = default)
    {
        // Idempotencia: si el cliente reintenta con la misma referencia, devolvemos la existente.
        var existente = await _facturas.ObtenerPorReferenciaAsync(tenantId, request.ReferenciaExterna, ct);
        if (existente is not null)
            return FacturaResponse.Desde(existente);

        var detalles = request.Detalles.Select(d => new DetalleFactura(
            d.CodigoProductoSin, d.CodigoProducto, d.Descripcion,
            d.Cantidad, d.UnidadMedida, d.PrecioUnitario, d.MontoDescuento));

        var factura = new Factura(
            tenantId, request.SucursalId, request.PuntoVentaId,
            request.CodigoDocumentoSector, request.ReferenciaExterna,
            request.Comprador.RazonSocial, request.Comprador.CodigoTipoDocumentoIdentidad,
            request.Comprador.NumeroDocumento, request.Comprador.Complemento, request.Comprador.Email,
            request.CodigoMoneda, request.TipoCambio,
            request.CodigoMetodoPago, request.NumeroTarjeta,
            detalles);

        await _facturas.AgregarAsync(factura, ct);
        await _facturas.GuardarCambiosAsync(ct);

        await _encolador.EncolarEmisionAsync(factura.Id, ct);

        return FacturaResponse.Desde(factura);
    }
}
