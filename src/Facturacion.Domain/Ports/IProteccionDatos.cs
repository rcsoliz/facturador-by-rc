namespace Facturacion.Domain.Ports;

/// <summary>
/// Cifrado simétrico reversible para secretos en reposo (tokens delegados SIAT,
/// futuros certificados) — ver regla de CLAUDE.md: nunca texto plano en la base.
/// </summary>
public interface IProteccionDatos
{
    string Cifrar(string textoPlano);

    string Descifrar(string textoCifrado);
}
