using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Application.Commands.EmitirFactura;
using Hangfire;

namespace Facturacion.Infrastructure.Colas;

/// <summary>
/// Encola de verdad: <see cref="IBackgroundJobClient.Enqueue"/> solo persiste
/// el job y devuelve — el procesamiento corre en el servidor de Hangfire
/// (mismo proceso que la Api, ver <c>Program.cs</c>), no en el scope del
/// request HTTP. Antes de esto (<c>EncoladorEmisionInmediato</c>) el request
/// de emisión esperaba todo el ciclo generar→enviar→SIN→webhook antes de
/// responder, contradiciendo el diseño "202 Aceptada" documentado.
/// </summary>
public class EncoladorEmisionHangfire : IEncoladorEmision
{
    private readonly IBackgroundJobClient _cliente;

    public EncoladorEmisionHangfire(IBackgroundJobClient cliente) => _cliente = cliente;

    public Task EncolarEmisionAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default)
    {
        _cliente.Enqueue<ProcesarEmisionHandler>(h => h.HandleAsync(tenantId, facturaId, CancellationToken.None));
        return Task.CompletedTask;
    }

    public Task EncolarAnulacionAsync(Guid tenantId, Guid facturaId, int codigoMotivo, CancellationToken ct = default)
    {
        _cliente.Enqueue<ProcesarAnulacionHandler>(h => h.HandleAsync(tenantId, facturaId, codigoMotivo, CancellationToken.None));
        return Task.CompletedTask;
    }
}
