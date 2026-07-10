namespace Facturacion.Application.Dtos;

/// <summary>Alta de un tenant (endpoint admin de onboarding).</summary>
public sealed record RegistrarTenantRequest(
    string RazonSocial,
    string Nit,
    int Modalidad); // ModalidadFacturacion: 1=ComputarizadaEnLinea, 2=ElectronicaEnLinea

/// <summary>
/// La ApiKey va en texto plano: es la única vez que se muestra, no se persiste
/// (el Tenant solo guarda su hash — ver <see cref="Facturacion.Domain.Common.ApiKeyHasher"/>).
/// </summary>
public sealed record RegistrarTenantResponse(
    Guid TenantId,
    string RazonSocial,
    string ApiKey);
