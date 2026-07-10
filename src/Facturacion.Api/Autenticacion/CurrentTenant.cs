using Facturacion.Domain.Entities;

namespace Facturacion.Api.Autenticacion;

public class CurrentTenant : ICurrentTenant
{
    private Tenant? _tenant;

    public Tenant Tenant => _tenant ?? throw new InvalidOperationException(
        "No hay tenant resuelto para esta petición. ¿Falta ApiKeyAuthMiddleware en el pipeline?");

    public Guid TenantId => Tenant.Id;

    public void Establecer(Tenant tenant) => _tenant = tenant;
}
