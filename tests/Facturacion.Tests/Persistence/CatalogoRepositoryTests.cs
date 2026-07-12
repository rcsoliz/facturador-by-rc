using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Infrastructure.Persistence;
using Xunit;

namespace Facturacion.Tests.Persistence;

[Collection("Postgres")]
public class CatalogoRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public CatalogoRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ReemplazarAsync_SegundaSincronizacion_ReemplazaLosItemsDelMismoTipo()
    {
        await using (var db = _fixture.NuevoContexto())
        {
            var repo = new EfCatalogoRepository(db);
            await repo.ReemplazarAsync(TipoCatalogo.ProductosServicios, new[]
            {
                new ItemCatalogo(TipoCatalogo.ProductosServicios, "P1", "Producto 1", true),
                new ItemCatalogo(TipoCatalogo.ProductosServicios, "P2", "Producto 2", true),
            });
        }

        await using (var db = _fixture.NuevoContexto())
        {
            var repo = new EfCatalogoRepository(db);
            await repo.ReemplazarAsync(TipoCatalogo.ProductosServicios, new[]
            {
                new ItemCatalogo(TipoCatalogo.ProductosServicios, "P1", "Producto 1 actualizado", true),
            });
        }

        await using var dbVerificacion = _fixture.NuevoContexto();
        var repoVerificacion = new EfCatalogoRepository(dbVerificacion);

        var p1 = await repoVerificacion.ObtenerAsync(TipoCatalogo.ProductosServicios, "P1");
        var p2 = await repoVerificacion.ObtenerAsync(TipoCatalogo.ProductosServicios, "P2");

        Assert.NotNull(p1);
        Assert.Equal("Producto 1 actualizado", p1!.Descripcion);
        Assert.Null(p2);
    }

    [Fact]
    public async Task ReemplazarAsync_NoAfectaOtroTipoDeCatalogo()
    {
        await using (var db = _fixture.NuevoContexto())
        {
            var repo = new EfCatalogoRepository(db);
            await repo.ReemplazarAsync(TipoCatalogo.ActividadesEconomicas, new[]
            {
                new ItemCatalogo(TipoCatalogo.ActividadesEconomicas, "A1", "Actividad 1", true),
            });
            await repo.ReemplazarAsync(TipoCatalogo.ProductosServicios, new[]
            {
                new ItemCatalogo(TipoCatalogo.ProductosServicios, "P9", "Producto 9", true),
            });
        }

        await using var dbVerificacion = _fixture.NuevoContexto();
        var repoVerificacion = new EfCatalogoRepository(dbVerificacion);

        Assert.NotNull(await repoVerificacion.ObtenerAsync(TipoCatalogo.ActividadesEconomicas, "A1"));
        Assert.NotNull(await repoVerificacion.ObtenerAsync(TipoCatalogo.ProductosServicios, "P9"));
    }
}
