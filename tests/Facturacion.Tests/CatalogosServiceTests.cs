using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Tests;

public class CatalogosServiceTests
{
    [Fact]
    public async Task SincronizarAsync_ReemplazaCatalogosPorTipo_ConLoQueDevuelveElCliente()
    {
        var repo = new CatalogoRepositoryFake();
        var cliente = new SinCatalogosClienteFake();
        var servicio = new CatalogosService(repo, cliente);

        await servicio.SincronizarAsync();

        Assert.True(await servicio.ExisteYActivoAsync(TipoCatalogo.ProductosServicios, "P1"));
        Assert.True(await servicio.ExisteYActivoAsync(TipoCatalogo.ActividadesEconomicas, "A1"));
        Assert.False(await servicio.ExisteYActivoAsync(TipoCatalogo.ProductosServicios, "INEXISTENTE"));
    }

    [Fact]
    public async Task SincronizarAsync_ItemInactivoEnElOrigen_QuedaComoInactivo()
    {
        var repo = new CatalogoRepositoryFake();
        var cliente = new SinCatalogosClienteFake();
        var servicio = new CatalogosService(repo, cliente);

        await servicio.SincronizarAsync();

        Assert.False(await servicio.ExisteYActivoAsync(TipoCatalogo.ProductosServicios, "P2-INACTIVO"));
    }

    [Fact]
    public async Task SincronizarAsync_EjecutadoDosVeces_NoDuplicaItems()
    {
        var repo = new CatalogoRepositoryFake();
        var cliente = new SinCatalogosClienteFake();
        var servicio = new CatalogosService(repo, cliente);

        await servicio.SincronizarAsync();
        await servicio.SincronizarAsync();

        Assert.Equal(2, repo.CantidadItems(TipoCatalogo.ProductosServicios));
    }

    [Fact]
    public async Task SincronizarAsync_UnCodigoDejaDeVenirDelSin_YaNoExisteTrasSincronizar()
    {
        var repo = new CatalogoRepositoryFake();
        var cliente = new SinCatalogosClienteFake();
        var servicio = new CatalogosService(repo, cliente);
        await servicio.SincronizarAsync();

        cliente.Productos = cliente.Productos.Where(p => p.Codigo != "P1").ToList();
        await servicio.SincronizarAsync();

        Assert.False(await servicio.ExisteYActivoAsync(TipoCatalogo.ProductosServicios, "P1"));
    }

    [Fact]
    public async Task ExisteYActivoAsync_CodigoNoSincronizado_DevuelveFalse()
    {
        var servicio = new CatalogosService(new CatalogoRepositoryFake(), new SinCatalogosClienteFake());

        Assert.False(await servicio.ExisteYActivoAsync(TipoCatalogo.ProductosServicios, "LO-QUE-SEA"));
    }

    private sealed class CatalogoRepositoryFake : ICatalogoRepository
    {
        private readonly List<ItemCatalogo> _items = new();

        public Task<ItemCatalogo?> ObtenerAsync(TipoCatalogo tipo, string codigo, CancellationToken ct = default) =>
            Task.FromResult(_items.SingleOrDefault(i => i.Tipo == tipo && i.Codigo == codigo));

        public Task ReemplazarAsync(TipoCatalogo tipo, IReadOnlyList<ItemCatalogo> items, CancellationToken ct = default)
        {
            _items.RemoveAll(i => i.Tipo == tipo);
            _items.AddRange(items);
            return Task.CompletedTask;
        }

        public int CantidadItems(TipoCatalogo tipo) => _items.Count(i => i.Tipo == tipo);
    }

    private sealed class SinCatalogosClienteFake : ISinCatalogosClient
    {
        public IReadOnlyList<ItemCatalogoSin> Productos { get; set; } = new[]
        {
            new ItemCatalogoSin("P1", "Producto 1", true),
            new ItemCatalogoSin("P2-INACTIVO", "Producto 2", false),
        };

        public IReadOnlyList<ItemCatalogoSin> Actividades { get; set; } = new[]
        {
            new ItemCatalogoSin("A1", "Actividad 1", true),
        };

        public Task<IReadOnlyList<ItemCatalogoSin>> ObtenerProductosServiciosAsync(CancellationToken ct = default) =>
            Task.FromResult(Productos);

        public Task<IReadOnlyList<ItemCatalogoSin>> ObtenerActividadesEconomicasAsync(CancellationToken ct = default) =>
            Task.FromResult(Actividades);
    }
}
