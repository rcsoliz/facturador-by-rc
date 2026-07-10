using Facturacion.Api.Autenticacion;
using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Application.Commands.EmitirFactura;
using Facturacion.Application.Dtos;
using Facturacion.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Controllers;

/// <summary>
/// Contrato REST público — lo único que ven los sistemas cliente.
/// Autenticación: header X-Api-Key, resuelto a tenant por <see cref="ApiKeyAuthMiddleware"/>.
/// </summary>
[ApiController]
[Route("api/v1/facturas")]
public class FacturasController : ControllerBase
{
    private readonly ICurrentTenant _tenant;

    public FacturasController(ICurrentTenant tenant) => _tenant = tenant;

    /// <summary>
    /// Emite una factura. Respuesta 202: la emisión es asíncrona; el resultado
    /// final llega por webhook (o polling a GET /{id}).
    /// Idempotente por referenciaExterna.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FacturaResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Emitir(
        [FromBody] EmitirFacturaRequest request,
        [FromServices] EmitirFacturaHandler handler,
        CancellationToken ct)
    {
        var respuesta = await handler.HandleAsync(_tenant.TenantId, request, ct);
        return AcceptedAtAction(nameof(Consultar), new { id = respuesta.Id }, respuesta);
    }

    /// <summary>Consulta el estado de una factura (fallback de polling al webhook).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FacturaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Consultar(
        Guid id,
        [FromServices] ConsultarFacturaHandler handler,
        CancellationToken ct)
    {
        var respuesta = await handler.HandleAsync(_tenant.TenantId, id, ct);
        return respuesta is null ? NotFound() : Ok(respuesta);
    }

    /// <summary>Solicita la anulación (asíncrona) de una factura validada.</summary>
    [HttpPost("{id:guid}/anulacion")]
    [ProducesResponseType(typeof(FacturaResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(
        Guid id,
        [FromBody] AnulacionRequest request,
        [FromServices] AnularFacturaHandler handler,
        CancellationToken ct)
    {
        var respuesta = await handler.HandleAsync(_tenant.TenantId, id, request.CodigoMotivo, ct);
        return respuesta is null ? NotFound() : Accepted(respuesta);
    }

    /// <summary>Descarga la representación gráfica (PDF con QR).</summary>
    [HttpGet("{id:guid}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DescargarPdf(Guid id)
    {
        // TODO(claude-code): IGeneradorRepresentacionGrafica (QuestPDF + QRCoder).
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { mensaje = "Representación gráfica disponible en próxima iteración." });
    }
}

public sealed record AnulacionRequest(int CodigoMotivo);
