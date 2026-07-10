using Facturacion.Domain.Entities;

namespace Facturacion.Api.Autenticacion;

/// <summary>
/// Tenant resuelto para la petición actual (scoped). Lo fija
/// <see cref="ApiKeyAuthMiddleware"/>; los controllers lo consumen en vez de
/// conocer el tenant por otro medio.
/// </summary>
public interface ICurrentTenant
{
    Guid TenantId { get; }
    Tenant Tenant { get; }
    void Establecer(Tenant tenant);
}
