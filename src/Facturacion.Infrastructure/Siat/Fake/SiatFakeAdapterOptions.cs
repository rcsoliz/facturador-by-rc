namespace Facturacion.Infrastructure.Siat.Fake;

/// <summary>
/// Configuración de <see cref="SiatFakeAdapter"/> — solo para desarrollo local,
/// nunca se usa en el adaptador real. Ver Siat/Common/README.md y la restricción
/// "SIN ACCESO AL AMBIENTE PILOTO DEL SIN" en CLAUDE.md.
/// </summary>
public sealed class SiatFakeAdapterOptions
{
    public const string SeccionConfiguracion = "SiatFake";

    /// <summary>Si es true, ComunicacionDisponibleAsync devuelve false (simula SIN caído → contingencia).</summary>
    public bool SimularSinIndisponible { get; set; } = false;

    /// <summary>
    /// Si <c>Factura.ReferenciaExterna</c> empieza con este prefijo (sin distinguir
    /// mayúsculas/minúsculas), <see cref="SiatFakeAdapter.EnviarAsync"/> simula un
    /// rechazo. Permite disparar el escenario de rechazo desde el propio request
    /// REST, sin tocar configuración global.
    /// </summary>
    public string PrefijoParaRechazo { get; set; } = "RECHAZAR-";
}
