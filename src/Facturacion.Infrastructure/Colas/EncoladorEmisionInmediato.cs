using Facturacion.Application.Commands.EmitirFactura;

namespace Facturacion.Infrastructure.Colas;

/// <summary>
/// Implementación provisional: no encola nada (el estado queda Pendiente
/// hasta que exista el worker).
/// TODO(claude-code): implementar con Hangfire —
///   BackgroundJob.Enqueue<ProcesarEmisionHandler>(h => h.HandleAsync(...))
/// </summary>
public class EncoladorEmisionInmediato : IEncoladorEmision
{
    public Task EncolarEmisionAsync(Guid facturaId, CancellationToken ct = default) => Task.CompletedTask;
    public Task EncolarAnulacionAsync(Guid facturaId, int codigoMotivo, CancellationToken ct = default) => Task.CompletedTask;
}
