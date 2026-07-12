using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Siat.Common;
using Facturacion.Infrastructure.Siat.Fake;
using Microsoft.Extensions.Options;

namespace Facturacion.Tests;

public class SiatFakeAdapterTests
{
    private static (SiatFakeAdapter Adapter, Factura Factura) CrearEscenario(
        SiatFakeAdapterOptions? fakeOptions = null, string referenciaExterna = "REF-001")
    {
        var tenant = new Tenant(
            "Mi Empresa SRL", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash-fake");
        var sucursal = tenant.AgregarSucursal(0, "AV. JORGE LOPEZ #123", "La Paz", "451010");
        var puntoVenta = sucursal.AgregarPuntoVenta(0, "Caja 1", 1);

        var detalles = new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m) };
        var factura = new Factura(
            tenant.Id, sucursal.Id, puntoVenta.Id, 1, referenciaExterna,
            "Mi razon social", 1, "5115889", null, null, 1, 1, 1, null, detalles);
        factura.AsignarNumero(1);

        var credencial = new CredencialSiat(tenant.Id, sucursal.Id, puntoVenta.Id, "token-plano-fake");
        var credenciales = new CredencialesService(
            new CredencialSiatRepositoryFake(credencial), new CredencialesClienteFake(), new ProteccionDatosIdentidadFake());

        var adapter = new SiatFakeAdapter(
            new TenantRepositoryFake(tenant),
            credenciales,
            Options.Create(new SiatOptions()),
            Options.Create(fakeOptions ?? new SiatFakeAdapterOptions()));

        return (adapter, factura);
    }

    [Fact]
    public async Task GenerarDocumentoAsync_ProduceCufYXmlValidoContraElXsdOficial()
    {
        var (adapter, factura) = CrearEscenario();

        var generado = await adapter.GenerarDocumentoAsync(factura);

        Assert.False(string.IsNullOrWhiteSpace(generado.Cuf.Valor));
        Assert.Contains("<facturaComputarizadaCompraVenta", generado.Xml);
        Assert.Empty(XmlFacturaBuilder.Validar(generado.Xml));
    }

    [Fact]
    public async Task EnviarAsync_ReferenciaSinPrefijoDeRechazo_DevuelveExitoso()
    {
        var (adapter, factura) = CrearEscenario(referenciaExterna: "REF-OK-001");

        var resultado = await adapter.EnviarAsync(factura);

        Assert.True(resultado.Exitoso);
        Assert.NotNull(resultado.CodigoRecepcion);
    }

    [Fact]
    public async Task EnviarAsync_ReferenciaConPrefijoDeRechazo_DevuelveFallo()
    {
        var (adapter, factura) = CrearEscenario(referenciaExterna: "RECHAZAR-001");

        var resultado = await adapter.EnviarAsync(factura);

        Assert.False(resultado.Exitoso);
        Assert.NotEmpty(resultado.Errores);
    }

    [Fact]
    public async Task ComunicacionDisponibleAsync_ConSimularSinIndisponible_DevuelveFalse()
    {
        var (adapter, _) = CrearEscenario(new SiatFakeAdapterOptions { SimularSinIndisponible = true });

        Assert.False(await adapter.ComunicacionDisponibleAsync());
    }

    [Fact]
    public async Task ComunicacionDisponibleAsync_PorDefecto_DevuelveTrue()
    {
        var (adapter, _) = CrearEscenario();

        Assert.True(await adapter.ComunicacionDisponibleAsync());
    }

    private sealed class TenantRepositoryFake : ITenantRepository
    {
        private readonly Tenant _tenant;
        public TenantRepositoryFake(Tenant tenant) => _tenant = tenant;

        public Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult(tenantId == _tenant.Id ? _tenant : null);

        public Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default) =>
            Task.FromResult<Tenant?>(null);

        public Task AgregarAsync(Tenant tenant, CancellationToken ct = default) => Task.CompletedTask;
        public Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default) => Task.CompletedTask;
        public Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default) => Task.CompletedTask;
        public Task GuardarCambiosAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class CredencialSiatRepositoryFake : ICredencialSiatRepository
    {
        private CredencialSiat? _credencial;
        public CredencialSiatRepositoryFake(CredencialSiat? credencial = null) => _credencial = credencial;

        public Task<CredencialSiat?> ObtenerAsync(
            Guid tenantId, Guid sucursalId, Guid? puntoVentaId, CancellationToken ct = default) =>
            Task.FromResult(_credencial is not null
                && _credencial.TenantId == tenantId && _credencial.SucursalId == sucursalId
                && _credencial.PuntoVentaId == puntoVentaId
                ? _credencial
                : null);

        public Task AgregarAsync(CredencialSiat credencial, CancellationToken ct = default)
        {
            _credencial = credencial;
            return Task.CompletedTask;
        }

        public Task GuardarCambiosAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ProteccionDatosIdentidadFake : IProteccionDatos
    {
        public string Cifrar(string textoPlano) => textoPlano;
        public string Descifrar(string textoCifrado) => textoCifrado;
    }
}
