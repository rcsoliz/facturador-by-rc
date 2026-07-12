using Facturacion.Application.Queries;
using Facturacion.Domain.Common;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;

namespace Facturacion.Tests;

public class ObtenerRepresentacionGraficaHandlerTests
{
    private static (Tenant Tenant, Sucursal Sucursal, PuntoVenta PuntoVenta) CrearTenantConSucursal()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. TEST 123", "La Paz", "451010");
        var puntoVenta = sucursal.AgregarPuntoVenta(0, "Caja 1", 1);
        return (tenant, sucursal, puntoVenta);
    }

    private static Factura CrearFactura(Tenant tenant, Sucursal sucursal, PuntoVenta puntoVenta) => new(
        tenant.Id, sucursal.Id, puntoVenta.Id,
        1, "REF-1", "Cliente Prueba", 1, "5115889", null, null,
        1, 1, 1, null,
        new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });

    [Fact]
    public async Task HandleAsync_FacturaInexistente_DevuelveNull()
    {
        var handler = new ObtenerRepresentacionGraficaHandler(
            new FacturaRepositoryFake(), new TenantRepositoryFake(), new GeneradorFake());

        var resultado = await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task HandleAsync_FacturaSinCuf_LanzaDomainException()
    {
        var (tenant, sucursal, puntoVenta) = CrearTenantConSucursal();
        var factura = CrearFactura(tenant, sucursal, puntoVenta);

        var handler = new ObtenerRepresentacionGraficaHandler(
            new FacturaRepositoryFake(factura), new TenantRepositoryFake(tenant), new GeneradorFake());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(tenant.Id, factura.Id));

        Assert.Equal("FACTURA_SIN_CUF", ex.Codigo);
    }

    [Fact]
    public async Task HandleAsync_FacturaValidada_DevuelveLosBytesDelGenerador()
    {
        var (tenant, sucursal, puntoVenta) = CrearTenantConSucursal();
        var factura = CrearFactura(tenant, sucursal, puntoVenta);
        factura.AsignarNumero(1);
        factura.MarcarGenerada(new Cuf("CUF-DE-PRUEBA"), "<xml/>");

        var generador = new GeneradorFake();
        var handler = new ObtenerRepresentacionGraficaHandler(
            new FacturaRepositoryFake(factura), new TenantRepositoryFake(tenant), generador);

        var resultado = await handler.HandleAsync(tenant.Id, factura.Id);

        Assert.Equal(generador.BytesADevolver, resultado);
        Assert.Same(sucursal, generador.UltimaSucursalRecibida);
        Assert.Same(puntoVenta, generador.UltimoPuntoVentaRecibido);
    }

    private sealed class FacturaRepositoryFake : IFacturaRepository
    {
        private readonly Factura? _factura;
        public FacturaRepositoryFake(Factura? factura = null) => _factura = factura;

        public Task<Factura?> ObtenerAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) =>
            Task.FromResult(_factura is not null && _factura.Id == facturaId ? _factura : null);

        public Task<Factura?> ObtenerPorReferenciaAsync(Guid tenantId, string referenciaExterna, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Factura>> ListarPorEstadoAsync(Guid tenantId, EstadoFactura estado, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<long> SiguienteNumeroAsync(Guid puntoVentaId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AgregarAsync(Factura factura, CancellationToken ct = default) => throw new NotSupportedException();
        public Task GuardarCambiosAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class TenantRepositoryFake : ITenantRepository
    {
        private readonly Tenant? _tenant;
        public TenantRepositoryFake(Tenant? tenant = null) => _tenant = tenant;

        public Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult(_tenant is not null && _tenant.Id == tenantId ? _tenant : null);

        public Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AgregarAsync(Tenant tenant, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default) => throw new NotSupportedException();
        public Task GuardarCambiosAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class GeneradorFake : IGeneradorRepresentacionGrafica
    {
        public byte[] BytesADevolver { get; } = { 1, 2, 3 };
        public Sucursal? UltimaSucursalRecibida { get; private set; }
        public PuntoVenta? UltimoPuntoVentaRecibido { get; private set; }

        public Task<byte[]> GenerarPdfAsync(
            Factura factura, Tenant tenant, Sucursal sucursal, PuntoVenta? puntoVenta, CancellationToken ct = default)
        {
            UltimaSucursalRecibida = sucursal;
            UltimoPuntoVentaRecibido = puntoVenta;
            return Task.FromResult(BytesADevolver);
        }
    }
}
