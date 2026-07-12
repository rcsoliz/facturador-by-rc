namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Cliente de las operaciones del SIN para obtener CUIS/CUFD. v1: no hay
/// implementación real (sin acceso al ambiente piloto — ver CLAUDE.md); la
/// única implementación es <see cref="Facturacion.Infrastructure.Siat.Fake.CredencialesClienteFake"/>.
/// Cuando se generen los clientes SOAP (dotnet-svcutil), la implementación real
/// reemplaza al fake solo por configuración de DI, sin tocar
/// <see cref="CredencialesService"/>.
/// </summary>
public interface ISinCredencialesClient
{
    Task<CuisObtenido> ObtenerCuisAsync(SolicitudCredencialSin solicitud, CancellationToken ct = default);

    Task<CufdObtenido> ObtenerCufdAsync(SolicitudCredencialSin solicitud, string cuis, CancellationToken ct = default);
}

/// <param name="Nit">NIT del tenant.</param>
/// <param name="TokenDelegado">Token delegado SIAT, ya descifrado.</param>
/// <param name="CodigoSucursal">Código de sucursal ante el SIN (0 = casa matriz).</param>
/// <param name="CodigoPuntoVenta">Código de punto de venta ante el SIN, si aplica.</param>
public sealed record SolicitudCredencialSin(
    string Nit, string TokenDelegado, int CodigoSucursal, int? CodigoPuntoVenta);

public sealed record CuisObtenido(string Cuis, DateTime VenceUtc);

public sealed record CufdObtenido(string Cufd, string CodigoControl, DateTime VenceUtc);
