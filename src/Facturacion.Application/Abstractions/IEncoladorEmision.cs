namespace Facturacion.Application.Commands.EmitirFactura;

/// <summary>
/// Abstracción de la cola de trabajos (implementación: Hangfire en Infrastructure).
/// Application no conoce Hangfire — solo pide "procesá esta factura después".
/// </summary>
public interface IEncoladorEmision
{
    Task EncolarEmisionAsync(Guid facturaId, CancellationToken ct = default);
    Task EncolarAnulacionAsync(Guid facturaId, int codigoMotivo, CancellationToken ct = default);
}
