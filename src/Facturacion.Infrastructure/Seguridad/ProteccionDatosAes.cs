using System.Security.Cryptography;
using Facturacion.Domain.Ports;

namespace Facturacion.Infrastructure.Seguridad;

/// <summary>
/// Cifrado simétrico reversible (AES-256-GCM) para secretos en reposo — v1:
/// token delegado SIAT (<see cref="Facturacion.Domain.Entities.CredencialSiat"/>).
/// Formato del texto cifrado: Base64(nonce[12] + tag[16] + ciphertext).
/// </summary>
public sealed class ProteccionDatosAes : IProteccionDatos
{
    private const int TamanoNonce = 12;
    private const int TamanoTag = 16;

    private readonly byte[] _clave;

    public ProteccionDatosAes(ProteccionDatosOptions opciones)
    {
        _clave = Convert.FromBase64String(opciones.ClaveMaestraBase64);
        if (_clave.Length != 32)
            throw new InvalidOperationException(
                "La clave maestra de ProteccionDatosAes debe ser AES-256 (32 bytes en Base64).");
    }

    public string Cifrar(string textoPlano)
    {
        var datos = System.Text.Encoding.UTF8.GetBytes(textoPlano);
        var nonce = RandomNumberGenerator.GetBytes(TamanoNonce);
        var ciphertext = new byte[datos.Length];
        var tag = new byte[TamanoTag];

        using var aesGcm = new AesGcm(_clave, TamanoTag);
        aesGcm.Encrypt(nonce, datos, ciphertext, tag);

        var salida = new byte[TamanoNonce + TamanoTag + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, salida, 0, TamanoNonce);
        Buffer.BlockCopy(tag, 0, salida, TamanoNonce, TamanoTag);
        Buffer.BlockCopy(ciphertext, 0, salida, TamanoNonce + TamanoTag, ciphertext.Length);
        return Convert.ToBase64String(salida);
    }

    public string Descifrar(string textoCifrado)
    {
        var entrada = Convert.FromBase64String(textoCifrado);
        if (entrada.Length < TamanoNonce + TamanoTag)
            throw new InvalidOperationException("Texto cifrado inválido: longitud insuficiente.");

        var nonce = entrada[..TamanoNonce];
        var tag = entrada[TamanoNonce..(TamanoNonce + TamanoTag)];
        var ciphertext = entrada[(TamanoNonce + TamanoTag)..];
        var datos = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(_clave, TamanoTag);
        aesGcm.Decrypt(nonce, ciphertext, tag, datos);
        return System.Text.Encoding.UTF8.GetString(datos);
    }
}
