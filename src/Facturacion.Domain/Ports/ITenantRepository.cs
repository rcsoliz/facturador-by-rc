using Facturacion.Domain.Entities;

namespace Facturacion.Domain.Ports;

public interface ITenantRepository
{
    Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default);
    Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default);
    Task AgregarAsync(Tenant tenant, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
