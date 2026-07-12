using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.AnularFactura;

/// <summary>
/// Caso de uso ejecutado por el WORKER (no por la API): anular ante el SIN →
/// actualizar estado → notificar webhook. Antes de esto la anulación quedaba
/// encolada pero nunca se procesaba (TODO explícito en el encolador anterior).
/// Si <see cref="IProveedorFiscal.AnularAsync"/> falla, se deja que la
/// excepción se propague: Hangfire reintenta automáticamente y, si se agotan
/// los reintentos, el job queda visible como fallido en el dashboard para
/// revisión manual — no hay estado de dominio para "anulación rechazada".
/// </summary>
public class ProcesarAnulacionHandler
{
    private readonly IFacturaRepository _facturas;
    private readonly ITenantRepository _tenants;
    private readonly IProveedorFiscal _proveedor;
    private readonly INotificadorWebhook _webhook;

    public ProcesarAnulacionHandler(
        IFacturaRepository facturas, ITenantRepository tenants, IProveedorFiscal proveedor, INotificadorWebhook webhook)
    {
        _facturas = facturas;
        _tenants = tenants;
        _proveedor = proveedor;
        _webhook = webhook;
    }

    public async Task HandleAsync(Guid tenantId, Guid facturaId, int codigoMotivo, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerAsync(tenantId, facturaId, ct)
            ?? throw new InvalidOperationException($"Factura {facturaId} no encontrada.");
        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        var resultado = await _proveedor.AnularAsync(factura, codigoMotivo, ct);
        if (!resultado.Exitoso)
            throw new InvalidOperationException(
                $"El SIN rechazó la anulación de la factura {facturaId}: " +
                string.Join("; ", resultado.Errores.Select(e => $"{e.Codigo}: {e.Descripcion}")));

        factura.MarcarAnulada(codigoMotivo, resultado.RespuestaRaw);
        await _facturas.GuardarCambiosAsync(ct);

        await _webhook.NotificarCambioEstadoAsync(tenant, factura, ct);
    }
}
