using Facturacion.Infrastructure.Webhooks;

namespace Facturacion.Tests;

public class FirmaWebhookTests
{
    [Fact]
    public void Calcular_VectorDePrueba_CoincideConHmacSha256CalculadoIndependientemente()
    {
        // Vector calculado independientemente con Python (hmac + hashlib), no con el
        // propio código bajo prueba — evita el error de "probar la implementación contra sí misma".
        var firma = FirmaWebhook.Calcular(
            "secreto-de-prueba", "1700000000", "{\"a\":1,\"b\":\"texto\"}");

        Assert.Equal("95f6cc840398727f75422590ee37a1ae86b98a2b266a7e3a555c6de1f45bb210", firma);
    }

    [Fact]
    public void Calcular_EsDeterministico()
    {
        var firma1 = FirmaWebhook.Calcular("secreto", "123", "{}");
        var firma2 = FirmaWebhook.Calcular("secreto", "123", "{}");

        Assert.Equal(firma1, firma2);
    }

    [Fact]
    public void Calcular_SecretoDistinto_DaFirmaDistinta()
    {
        var firma1 = FirmaWebhook.Calcular("secreto-a", "123", "{}");
        var firma2 = FirmaWebhook.Calcular("secreto-b", "123", "{}");

        Assert.NotEqual(firma1, firma2);
    }

    [Fact]
    public void Calcular_TimestampDistinto_DaFirmaDistinta()
    {
        var firma1 = FirmaWebhook.Calcular("secreto", "111", "{}");
        var firma2 = FirmaWebhook.Calcular("secreto", "222", "{}");

        Assert.NotEqual(firma1, firma2);
    }

    [Fact]
    public void Calcular_CuerpoDistinto_DaFirmaDistinta()
    {
        var firma1 = FirmaWebhook.Calcular("secreto", "123", "{\"x\":1}");
        var firma2 = FirmaWebhook.Calcular("secreto", "123", "{\"x\":2}");

        Assert.NotEqual(firma1, firma2);
    }
}
