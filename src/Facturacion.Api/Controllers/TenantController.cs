using Facturacion.Api.Autenticacion;
using Facturacion.Application.Commands.ConfigurarWebhook;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Controllers;

/// <summary>
/// Autoservicio del tenant autenticado sobre sus propios datos (a diferencia
/// de <see cref="TenantsAdminController"/>, acá el tenant ya existe).
/// Autenticación: header X-Api-Key (igual que SucursalesController/FacturasController).
/// </summary>
[ApiController]
[Route("api/v1/tenant")]
public class TenantController : ControllerBase
{
    private readonly ICurrentTenant _tenant;

    public TenantController(ICurrentTenant tenant) => _tenant = tenant;

    /// <summary>
    /// Configura (o rota) la URL y el secreto HMAC del webhook al que se
    /// notifican los cambios de estado de las facturas del tenant autenticado.
    /// </summary>
    [HttpPut("webhook")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigurarWebhook(
        [FromBody] ConfigurarWebhookRequest request,
        [FromServices] ConfigurarWebhookHandler handler,
        CancellationToken ct)
    {
        try
        {
            await handler.HandleAsync(_tenant.TenantId, request, ct);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { codigo = ex.Codigo, mensaje = ex.Message });
        }
    }
}
