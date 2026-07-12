namespace Facturacion.Infrastructure.Seguridad;

/// <summary>
/// Clave maestra para <see cref="ProteccionDatosAes"/>, leída de variable de
/// entorno (nunca de appsettings ni del repo — ver regla de CLAUDE.md).
/// </summary>
/// <param name="ClaveMaestraBase64">32 bytes (AES-256) codificados en Base64.</param>
public sealed record ProteccionDatosOptions(string ClaveMaestraBase64);
