using Facturacion.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infrastructure.Colas;

/// <summary>
/// Job recurrente (Hangfire, registrado en <c>Program.cs</c>): reintenta enviar
/// al SIN las facturas que quedaron <c>EnContingencia</c> (SIN no disponible al
/// momento de emitirlas), agrupadas por tenant — un paquete de contingencia es
/// una operación por NIT, no se mezclan tenants en un mismo envío.
/// </summary>
public class JobEnvioPaquetesContingencia
{
    private readonly IFacturaRepository _facturas;
    private readonly ITenantRepository _tenants;
    private readonly IProveedorFiscal _proveedor;
    private readonly INotificadorWebhook _webhook;
    private readonly ILogger<JobEnvioPaquetesContingencia> _logger;

    public JobEnvioPaquetesContingencia(
        IFacturaRepository facturas, ITenantRepository tenants,
        IProveedorFiscal proveedor, INotificadorWebhook webhook, ILogger<JobEnvioPaquetesContingencia> logger)
    {
        _facturas = facturas;
        _tenants = tenants;
        _proveedor = proveedor;
        _webhook = webhook;
        _logger = logger;
    }

    public async Task EjecutarAsync(CancellationToken ct = default)
    {
        if (!await _proveedor.ComunicacionDisponibleAsync(ct))
            return; // el SIN sigue caído, no tiene sentido insistir todavía

        var pendientes = await _facturas.ListarEnContingenciaAsync(ct);

        foreach (var grupo in pendientes.GroupBy(f => f.TenantId))
        {
            try
            {
                var tenant = await _tenants.ObtenerAsync(grupo.Key, ct);
                if (tenant is null)
                {
                    _logger.LogWarning("Tenant {TenantId} no encontrado, se omite su paquete de contingencia.", grupo.Key);
                    continue;
                }

                var facturasDelTenant = grupo.ToList();
                var resultado = await _proveedor.EnviarPaqueteContingenciaAsync(facturasDelTenant, ct);

                foreach (var factura in facturasDelTenant)
                {
                    if (resultado.Exitoso)
                        factura.MarcarValidada(resultado.CodigoRecepcion ?? string.Empty, resultado.RespuestaRaw);
                    else
                        factura.MarcarRechazada(
                            string.Join("; ", resultado.Errores.Select(e => $"{e.Codigo}: {e.Descripcion}")),
                            resultado.RespuestaRaw);
                }

                await _facturas.GuardarCambiosAsync(ct);

                foreach (var factura in facturasDelTenant)
                    await _webhook.NotificarCambioEstadoAsync(tenant, factura, ct);
            }
            catch (Exception ex)
            {
                // Un paquete con problemas no debe frenar el de los demás tenants.
                _logger.LogError(ex, "Fallo enviando el paquete de contingencia del tenant {TenantId}.", grupo.Key);
            }
        }
    }
}
