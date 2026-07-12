using Facturacion.Api.Autenticacion;
using Facturacion.Application.Commands.AgregarPuntoVenta;
using Facturacion.Application.Commands.AgregarSucursal;
using Facturacion.Application.Commands.RegistrarCredencialSiat;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.Api.Controllers;

/// <summary>
/// Autoservicio de sucursales/puntos de venta del tenant autenticado.
/// Autenticación: header X-Api-Key (igual que FacturasController) — a diferencia
/// del onboarding de tenants, acá el tenant ya existe y gestiona sus propios datos.
/// </summary>
[ApiController]
[Route("api/v1/sucursales")]
public class SucursalesController : ControllerBase
{
    private readonly ICurrentTenant _tenant;

    public SucursalesController(ICurrentTenant tenant) => _tenant = tenant;

    /// <summary>Lista las sucursales (con sus puntos de venta) del tenant autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SucursalResponse>), StatusCodes.Status200OK)]
    public IActionResult Listar() =>
        Ok(_tenant.Tenant.Sucursales.Select(SucursalResponse.Desde).ToList());

    /// <summary>Da de alta una sucursal para el tenant autenticado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SucursalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Agregar(
        [FromBody] AgregarSucursalRequest request,
        [FromServices] AgregarSucursalHandler handler,
        CancellationToken ct)
    {
        try
        {
            var respuesta = await handler.HandleAsync(_tenant.TenantId, request, ct);
            return StatusCode(StatusCodes.Status201Created, respuesta);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { codigo = ex.Codigo, mensaje = ex.Message });
        }
    }

    /// <summary>Da de alta un punto de venta bajo una sucursal del tenant autenticado.</summary>
    [HttpPost("{sucursalId:guid}/puntos-venta")]
    [ProducesResponseType(typeof(PuntoVentaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgregarPuntoVenta(
        Guid sucursalId,
        [FromBody] AgregarPuntoVentaRequest request,
        [FromServices] AgregarPuntoVentaHandler handler,
        CancellationToken ct)
    {
        var respuesta = await handler.HandleAsync(_tenant.TenantId, sucursalId, request, ct);
        return respuesta is null ? NotFound() : StatusCode(StatusCodes.Status201Created, respuesta);
    }

    /// <summary>
    /// Registra (o rota) el token delegado que el SIN entregó para una sucursal
    /// o punto de venta del tenant autenticado — prerequisito para que el
    /// servicio pueda obtener/renovar CUIS y CUFD al emitir facturas.
    /// </summary>
    [HttpPost("{sucursalId:guid}/credencial-siat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarCredencialSiat(
        Guid sucursalId,
        [FromBody] RegistrarCredencialSiatRequest request,
        [FromServices] RegistrarCredencialSiatHandler handler,
        CancellationToken ct)
    {
        var registrado = await handler.HandleAsync(_tenant.TenantId, sucursalId, request, ct);
        return registrado ? NoContent() : NotFound();
    }
}
