using Facturacion.Domain.Entities;

namespace Facturacion.Domain.Ports;

public interface ICredencialSiatRepository
{
    /// <summary>
    /// Busca la credencial de un punto de venta específico, o de la sucursal
    /// (<paramref name="puntoVentaId"/> null) si el CUIS/CUFD se gestiona a
    /// nivel sucursal.
    /// </summary>
    Task<CredencialSiat?> ObtenerAsync(
        Guid tenantId, Guid sucursalId, Guid? puntoVentaId, CancellationToken ct = default);

    Task AgregarAsync(CredencialSiat credencial, CancellationToken ct = default);

    /// <summary>Todas las credenciales registradas, de cualquier tenant — uso interno del job de renovación de CUFD.</summary>
    Task<IReadOnlyList<CredencialSiat>> ListarTodasAsync(CancellationToken ct = default);

    Task GuardarCambiosAsync(CancellationToken ct = default);
}
