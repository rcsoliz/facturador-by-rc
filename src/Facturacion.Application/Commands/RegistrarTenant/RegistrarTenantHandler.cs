using System.Security.Cryptography;
using Facturacion.Application.Dtos;
using Facturacion.Domain.Common;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;

namespace Facturacion.Application.Commands.RegistrarTenant;

/// <summary>
/// Caso de uso: onboarding de un tenant nuevo. Genera la API key, la devuelve
/// en texto plano una única vez y persiste solo su hash.
/// </summary>
public class RegistrarTenantHandler
{
    private readonly ITenantRepository _tenants;

    public RegistrarTenantHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<RegistrarTenantResponse> HandleAsync(
        RegistrarTenantRequest request, CancellationToken ct = default)
    {
        var modalidad = (ModalidadFacturacion)request.Modalidad;
        if (!Enum.IsDefined(modalidad))
            throw new DomainException("MODALIDAD_INVALIDA", $"Modalidad '{request.Modalidad}' no reconocida.");

        var apiKey = GenerarApiKey();
        var tenant = new Tenant(request.RazonSocial, new Nit(request.Nit), modalidad, ApiKeyHasher.Hash(apiKey));

        await _tenants.AgregarAsync(tenant, ct);
        await _tenants.GuardarCambiosAsync(ct);

        return new RegistrarTenantResponse(tenant.Id, tenant.RazonSocial, apiKey);
    }

    private static string GenerarApiKey() =>
        $"fac_{Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant()}";
}
