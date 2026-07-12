using Facturacion.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Job recurrente (Hangfire, registrado en <c>Program.cs</c>): renueva CUFD
/// (y CUIS si corresponde) de todas las credenciales registradas, antes de
/// que las necesite una emisión. Sin este job, <c>CredencialesService</c>
/// solo renueva bajo demanda (lazy) en cada emisión — sigue funcionando así
/// como red de seguridad, pero este job reduce la latencia agregada por
/// renovación en el camino caliente de emisión.
/// </summary>
public class JobRenovacionCufd
{
    private readonly ICredencialSiatRepository _credenciales;
    private readonly ITenantRepository _tenants;
    private readonly CredencialesService _credencialesService;
    private readonly ILogger<JobRenovacionCufd> _logger;

    public JobRenovacionCufd(
        ICredencialSiatRepository credenciales, ITenantRepository tenants,
        CredencialesService credencialesService, ILogger<JobRenovacionCufd> logger)
    {
        _credenciales = credenciales;
        _tenants = tenants;
        _credencialesService = credencialesService;
        _logger = logger;
    }

    public async Task EjecutarAsync(CancellationToken ct = default)
    {
        var credenciales = await _credenciales.ListarTodasAsync(ct);

        foreach (var credencial in credenciales)
        {
            try
            {
                var tenant = await _tenants.ObtenerAsync(credencial.TenantId, ct);
                var sucursal = tenant?.Sucursales.SingleOrDefault(s => s.Id == credencial.SucursalId);
                if (tenant is null || sucursal is null)
                {
                    _logger.LogWarning(
                        "Credencial {CredencialId} apunta a un tenant/sucursal inexistente, se omite.", credencial.Id);
                    continue;
                }

                var puntoVenta = credencial.PuntoVentaId is { } puntoVentaId
                    ? sucursal.PuntosVenta.SingleOrDefault(p => p.Id == puntoVentaId)
                    : null;

                await _credencialesService.ObtenerCufdVigenteAsync(
                    credencial.TenantId, credencial.SucursalId, credencial.PuntoVentaId,
                    tenant.Nit.Valor, sucursal.CodigoSiat, puntoVenta?.CodigoSiat, ct);
            }
            catch (Exception ex)
            {
                // Una credencial con problemas (token revocado, etc.) no debe frenar
                // la renovación de las demás.
                _logger.LogError(ex, "Fallo renovando CUFD para la credencial {CredencialId}.", credencial.Id);
            }
        }
    }
}
