using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.EmitirFactura;

/// <summary>
/// Caso de uso ejecutado por el WORKER (no por la API):
/// generar documento → enviar al SIN → actualizar estado → notificar webhook.
/// Si el SIN no está disponible, pasa la factura a contingencia.
/// </summary>
public class ProcesarEmisionHandler
{
    private readonly IFacturaRepository _facturas;
    private readonly ITenantRepository _tenants;
    private readonly IProveedorFiscal _proveedor;
    private readonly INotificadorWebhook _webhook;

    public ProcesarEmisionHandler(
        IFacturaRepository facturas,
        ITenantRepository tenants,
        IProveedorFiscal proveedor,
        INotificadorWebhook webhook)
    {
        _facturas = facturas;
        _tenants = tenants;
        _proveedor = proveedor;
        _webhook = webhook;
    }

    public async Task HandleAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerAsync(tenantId, facturaId, ct)
            ?? throw new InvalidOperationException($"Factura {facturaId} no encontrada.");
        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        // 1. Número correlativo por punto de venta
        var numero = await _facturas.SiguienteNumeroAsync(factura.PuntoVentaId, ct);
        factura.AsignarNumero(numero);

        // 2. ¿SIN disponible? Si no, contingencia.
        if (!await _proveedor.ComunicacionDisponibleAsync(ct))
        {
            factura.MarcarEnContingencia();
            await _facturas.GuardarCambiosAsync(ct);
            return; // el worker de contingencia la retomará
        }

        // 3. Generar XML + CUF
        var generada = await _proveedor.GenerarDocumentoAsync(factura, ct);
        factura.MarcarGenerada(generada.Cuf, generada.Xml);

        // 4. Enviar al SIN
        factura.MarcarEnviada();
        var resultado = await _proveedor.EnviarAsync(factura, ct);

        if (resultado.Exitoso)
            factura.MarcarValidada(resultado.CodigoRecepcion!, resultado.RespuestaRaw);
        else
            factura.MarcarRechazada(
                string.Join("; ", resultado.Errores.Select(e => $"{e.Codigo}: {e.Descripcion}")),
                resultado.RespuestaRaw);

        await _facturas.GuardarCambiosAsync(ct);

        // 5. Notificar al sistema cliente
        await _webhook.NotificarCambioEstadoAsync(tenant, factura, ct);
    }
}
