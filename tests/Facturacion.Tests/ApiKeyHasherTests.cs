using Facturacion.Domain.Common;

namespace Facturacion.Tests;

public class ApiKeyHasherTests
{
    [Fact]
    public void Hash_MismaApiKey_ProduceElMismoHash()
    {
        Assert.Equal(ApiKeyHasher.Hash("clave-123"), ApiKeyHasher.Hash("clave-123"));
    }

    [Fact]
    public void Hash_ApiKeysDistintas_ProduceHashesDistintos()
    {
        Assert.NotEqual(ApiKeyHasher.Hash("clave-123"), ApiKeyHasher.Hash("clave-456"));
    }

    [Fact]
    public void Hash_NoDevuelveLaClaveEnTextoPlano()
    {
        Assert.DoesNotContain("clave-123", ApiKeyHasher.Hash("clave-123"));
    }
}
