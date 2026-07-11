using Facturacion.Application.Commands.EmitirFactura;

namespace Facturacion.Infrastructure.Colas;

/// <summary>
/// Implementación provisional: procesa la emisión sincrónicamente, en el mismo
/// scope de la request (no hay cola real todavía).
/// TODO(claude-code): reemplazar por Hangfire —
///   BackgroundJob.Enqueue<ProcesarEmisionHandler>(h => h.HandleAsync(tenantId, facturaId, CancellationToken.None))
/// La anulación queda sin procesar (no hay un handler de worker para eso todavía)
/// hasta que se implemente ese flujo.
/// </summary>
public class EncoladorEmisionInmediato : IEncoladorEmision
{
    private readonly ProcesarEmisionHandler _procesarEmision;

    public EncoladorEmisionInmediato(ProcesarEmisionHandler procesarEmision) => _procesarEmision = procesarEmision;

    public Task EncolarEmisionAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) =>
        _procesarEmision.HandleAsync(tenantId, facturaId, ct);

    public Task EncolarAnulacionAsync(Guid tenantId, Guid facturaId, int codigoMotivo, CancellationToken ct = default) =>
        Task.CompletedTask;
}
