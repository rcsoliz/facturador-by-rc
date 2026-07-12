using Facturacion.Application.Commands.AnularFactura;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Models;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;

namespace Facturacion.Tests;

public class ProcesarAnulacionHandlerTests
{
    private static (Tenant Tenant, Sucursal Sucursal, PuntoVenta PuntoVenta) CrearTenantConSucursal()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. TEST 123", "La Paz", "451010");
        var puntoVenta = sucursal.AgregarPuntoVenta(0, "Caja 1", 1);
        return (tenant, sucursal, puntoVenta);
    }

    private static Factura CrearFacturaValidada(Tenant tenant, Sucursal sucursal, PuntoVenta puntoVenta)
    {
        var factura = new Factura(
            tenant.Id, sucursal.Id, puntoVenta.Id,
            1, "REF-1", "Cliente Prueba", 1, "5115889", null, null,
            1, 1, 1, null,
            new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });
        factura.AsignarNumero(1);
        factura.MarcarGenerada(new Cuf("CUF-DE-PRUEBA"), "<xml/>");
        factura.MarcarEnviada();
        factura.MarcarValidada("REC-1", "{}");
        return factura;
    }

    [Fact]
    public async Task HandleAsync_AnulacionExitosa_MarcaAnuladaYNotifica()
    {
        var (tenant, sucursal, puntoVenta) = CrearTenantConSucursal();
        var factura = CrearFacturaValidada(tenant, sucursal, puntoVenta);
        var proveedor = new ProveedorFiscalFake(exitoso: true);
        var webhook = new NotificadorWebhookFake();
        var handler = new ProcesarAnulacionHandler(
            new FacturaRepositoryFake(factura), new TenantRepositoryFake(tenant), proveedor, webhook);

        await handler.HandleAsync(tenant.Id, factura.Id, 3);

        Assert.Equal(EstadoFactura.Anulada, factura.Estado);
        Assert.Equal(3, factura.CodigoMotivoAnulacion);
        Assert.Same(factura, webhook.UltimaFacturaNotificada);
    }

    [Fact]
    public async Task HandleAsync_ElSinRechazaLaAnulacion_Lanza()
    {
        var (tenant, sucursal, puntoVenta) = CrearTenantConSucursal();
        var factura = CrearFacturaValidada(tenant, sucursal, puntoVenta);
        var proveedor = new ProveedorFiscalFake(exitoso: false);
        var handler = new ProcesarAnulacionHandler(
            new FacturaRepositoryFake(factura), new TenantRepositoryFake(tenant), proveedor, new NotificadorWebhookFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(tenant.Id, factura.Id, 3));

        Assert.Equal(EstadoFactura.Validada, factura.Estado); // no cambia si el SIN rechaza
    }

    private sealed class ProveedorFiscalFake : IProveedorFiscal
    {
        private readonly bool _exitoso;
        public ProveedorFiscalFake(bool exitoso) => _exitoso = exitoso;

        public Task<ResultadoFiscal> AnularAsync(Factura factura, int codigoMotivo, CancellationToken ct = default) =>
            Task.FromResult(_exitoso
                ? ResultadoFiscal.Ok("REC-ANULACION", "ANULADA", "{}")
                : ResultadoFiscal.Fallo(new[] { new ErrorFiscal("1", "motivo inválido") }, "{}"));

        public Task<FacturaGenerada> GenerarDocumentoAsync(Factura factura, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<ResultadoFiscal> EnviarAsync(Factura factura, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<ResultadoFiscal> ConsultarEstadoAsync(Factura factura, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<ResultadoFiscal> EnviarPaqueteContingenciaAsync(IReadOnlyList<Factura> facturas, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<bool> ComunicacionDisponibleAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class FacturaRepositoryFake : IFacturaRepository
    {
        private readonly Factura _factura;
        public FacturaRepositoryFake(Factura factura) => _factura = factura;

        public Task<Factura?> ObtenerAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) =>
            Task.FromResult(_factura.Id == facturaId ? _factura : null);

        public Task<Factura?> ObtenerPorReferenciaAsync(Guid tenantId, string referenciaExterna, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<IReadOnlyList<Factura>> ListarPorEstadoAsync(Guid tenantId, EstadoFactura estado, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<IReadOnlyList<Factura>> ListarEnContingenciaAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<long> SiguienteNumeroAsync(Guid puntoVentaId, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task AgregarAsync(Factura factura, CancellationToken ct = default) => throw new NotSupportedException();
        public Task GuardarCambiosAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class TenantRepositoryFake : ITenantRepository
    {
        private readonly Tenant _tenant;
        public TenantRepositoryFake(Tenant tenant) => _tenant = tenant;

        public Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult(_tenant.Id == tenantId ? _tenant : null);
        public Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task AgregarAsync(Tenant tenant, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default) => throw new NotSupportedException();
        public Task GuardarCambiosAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class NotificadorWebhookFake : INotificadorWebhook
    {
        public Factura? UltimaFacturaNotificada { get; private set; }

        public Task NotificarCambioEstadoAsync(Tenant tenant, Factura factura, CancellationToken ct = default)
        {
            UltimaFacturaNotificada = factura;
            return Task.CompletedTask;
        }
    }
}
