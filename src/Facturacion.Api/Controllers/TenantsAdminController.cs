using Facturacion.Application.Commands.RegistrarTenant;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Controllers;

/// <summary>
/// Onboarding de tenants. Protegido por X-Admin-Key (AdminApiKeyMiddleware),
/// no por X-Api-Key de tenant — acá es donde el tenant todavía no existe.
/// </summary>
[ApiController]
[Route("api/v1/admin/tenants")]
public class TenantsAdminController : ControllerBase
{
    /// <summary>
    /// Da de alta un tenant y genera su API key. La API key se devuelve en
    /// texto plano una única vez: el servicio solo persiste su hash.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarTenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarTenantRequest request,
        [FromServices] RegistrarTenantHandler handler,
        CancellationToken ct)
    {
        try
        {
            var respuesta = await handler.HandleAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, respuesta);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { codigo = ex.Codigo, mensaje = ex.Message });
        }
    }
}
