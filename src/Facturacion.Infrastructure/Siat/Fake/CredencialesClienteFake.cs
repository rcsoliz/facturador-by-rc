using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Infrastructure.Siat.Fake;

/// <summary>
/// Implementación de <see cref="ISinCredencialesClient"/> para desarrollo local,
/// sin tocar el SIN (ver restricción "SIN ACCESO AL AMBIENTE PILOTO DEL SIN" en
/// CLAUDE.md). Genera CUIS/CUFD con vigencia realista (1 año / 24h) pero valores
/// que no representan credenciales reales del SIN.
/// </summary>
public sealed class CredencialesClienteFake : ISinCredencialesClient
{
    public Task<CuisObtenido> ObtenerCuisAsync(SolicitudCredencialSin solicitud, CancellationToken ct = default) =>
        Task.FromResult(new CuisObtenido(
            $"FAKE-CUIS-{Guid.NewGuid():N}", DateTime.UtcNow.AddYears(1)));

    public Task<CufdObtenido> ObtenerCufdAsync(
        SolicitudCredencialSin solicitud, string cuis, CancellationToken ct = default) =>
        Task.FromResult(new CufdObtenido(
            $"FAKE-CUFD-{Guid.NewGuid():N}",
            Guid.NewGuid().ToString("N")[..16],
            DateTime.UtcNow.AddHours(24)));
}
