using Facturacion.Infrastructure.Seguridad;

namespace Facturacion.Tests;

public class ProteccionDatosAesTests
{
    // AES-256 de prueba (32 bytes en Base64) — nunca usar en producción.
    private const string ClaveMaestraPrueba = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=";

    private static ProteccionDatosAes CrearServicio() =>
        new(new ProteccionDatosOptions(ClaveMaestraPrueba));

    [Fact]
    public void CifrarYDescifrar_DevuelveElTextoOriginal()
    {
        var servicio = CrearServicio();
        var textoPlano = "token-delegado-super-secreto-123";

        var cifrado = servicio.Cifrar(textoPlano);
        var descifrado = servicio.Descifrar(cifrado);

        Assert.Equal(textoPlano, descifrado);
    }

    [Fact]
    public void Cifrar_MismoTextoPlanoDosVeces_ProduceCifradosDistintos()
    {
        var servicio = CrearServicio();
        var textoPlano = "token-delegado-super-secreto-123";

        var cifrado1 = servicio.Cifrar(textoPlano);
        var cifrado2 = servicio.Cifrar(textoPlano);

        Assert.NotEqual(cifrado1, cifrado2); // nonce aleatorio por operación
    }

    [Fact]
    public void Cifrar_NuncaDevuelveElTextoPlanoEnClaro()
    {
        var servicio = CrearServicio();
        var textoPlano = "token-delegado-super-secreto-123";

        var cifrado = servicio.Cifrar(textoPlano);

        Assert.DoesNotContain(textoPlano, cifrado);
    }

    [Fact]
    public void Descifrar_TextoCifradoAlterado_LanzaExcepcion()
    {
        var servicio = CrearServicio();
        var cifrado = servicio.Cifrar("token-delegado-super-secreto-123");
        var bytes = Convert.FromBase64String(cifrado);
        bytes[^1] ^= 0xFF; // corrompe el último byte (parte del ciphertext)
        var alterado = Convert.ToBase64String(bytes);

        Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(
            () => servicio.Descifrar(alterado));
    }

    [Fact]
    public void Constructor_ConClaveDeLongitudInvalida_LanzaExcepcion()
    {
        var claveCorta = Convert.ToBase64String(new byte[16]); // AES-128, no AES-256

        Assert.Throws<InvalidOperationException>(
            () => new ProteccionDatosAes(new ProteccionDatosOptions(claveCorta)));
    }
}
