namespace Facturacion.Domain.Ports;

/// <summary>
/// Puerto que expone a Application la única operación de gestión de credenciales
/// SIAT que necesita conocer: registrar/rotar el token delegado de una sucursal
/// o punto de venta. El resto de <c>CredencialesService</c> (obtención/renovación
/// de CUFD) es un detalle de Infrastructure, consumido solo por los adaptadores
/// de <see cref="IProveedorFiscal"/> — Application/Domain nunca conocen CUFD.
/// </summary>
public interface IGestorCredencialesSiat
{
    Task RegistrarTokenDelegadoAsync(
        Guid tenantId, Guid sucursalId, Guid? puntoVentaId, string tokenDelegadoPlano, CancellationToken ct = default);
}
