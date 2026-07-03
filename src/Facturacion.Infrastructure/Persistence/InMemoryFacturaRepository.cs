using System.Collections.Concurrent;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;

namespace Facturacion.Infrastructure.Persistence;

/// <summary>
/// Repositorio en memoria para desarrollo temprano de la API sin DB.
/// TODO(claude-code): reemplazar por EF Core + Npgsql (FacturacionDbContext,
/// migraciones, secuencias por punto de venta para el correlativo).
/// </summary>
public class InMemoryFacturaRepository : IFacturaRepository
{
    private readonly ConcurrentDictionary<Guid, Factura> _store = new();
    private readonly ConcurrentDictionary<Guid, long> _correlativos = new();

    public Task<Factura?> ObtenerAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) =>
        Task.FromResult(_store.TryGetValue(facturaId, out var f) && f.TenantId == tenantId ? f : null);

    public Task<Factura?> ObtenerPorReferenciaAsync(Guid tenantId, string referenciaExterna, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(f =>
            f.TenantId == tenantId && f.ReferenciaExterna == referenciaExterna));

    public Task<IReadOnlyList<Factura>> ListarPorEstadoAsync(Guid tenantId, EstadoFactura estado, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Factura>>(
            _store.Values.Where(f => f.TenantId == tenantId && f.Estado == estado).ToList());

    public Task<long> SiguienteNumeroAsync(Guid puntoVentaId, CancellationToken ct = default) =>
        Task.FromResult(_correlativos.AddOrUpdate(puntoVentaId, 1, (_, n) => n + 1));

    public Task AgregarAsync(Factura factura, CancellationToken ct = default)
    {
        _store[factura.Id] = factura;
        return Task.CompletedTask;
    }

    public Task GuardarCambiosAsync(CancellationToken ct = default) => Task.CompletedTask;
}
