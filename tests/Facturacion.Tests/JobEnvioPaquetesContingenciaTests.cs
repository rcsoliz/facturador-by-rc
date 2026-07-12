using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Models;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Colas;
using Microsoft.Extensions.Logging.Abstractions;

namespace Facturacion.Tests;

public class JobEnvioPaquetesContingenciaTests
{
    private static (Tenant Tenant, Factura Factura) CrearTenantConFacturaEnContingencia()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. TEST 123", "La Paz", "451010");
        var puntoVenta = sucursal.AgregarPuntoVenta(0, "Caja 1", 1);

        var factura = new Factura(
            tenant.Id, sucursal.Id, puntoVenta.Id,
            1, "REF-1", "Cliente Prueba", 1, "5115889", null, null,
            1, 1, 1, null,
            new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });
        factura.MarcarEnContingencia();

        return (tenant, factura);
    }

    [Fact]
    public async Task EjecutarAsync_SinDisponible_EnviaElPaqueteYMarcaValidadas()
    {
        var (tenant, factura) = CrearTenantConFacturaEnContingencia();
        var proveedor = new ProveedorFiscalFake(disponible: true, exitoso: true);
        var webhook = new NotificadorWebhookFake();
        var job = new JobEnvioPaquetesContingencia(
            new FacturaRepositoryFake(new[] { factura }), new TenantRepositoryFake(tenant), proveedor, webhook,
            NullLogger<JobEnvioPaquetesContingencia>.Instance);

        await job.EjecutarAsync();

        Assert.Equal(EstadoFactura.Validada, factura.Estado);
        Assert.Single(webhook.Notificadas);
    }

    [Fact]
    public async Task EjecutarAsync_SinIndisponible_NoTocaLasFacturas()
    {
        var (tenant, factura) = CrearTenantConFacturaEnContingencia();
        var proveedor = new ProveedorFiscalFake(disponible: false, exitoso: true);
        var job = new JobEnvioPaquetesContingencia(
            new FacturaRepositoryFake(new[] { factura }), new TenantRepositoryFake(tenant), proveedor,
            new NotificadorWebhookFake(), NullLogger<JobEnvioPaquetesContingencia>.Instance);

        await job.EjecutarAsync();

        Assert.Equal(EstadoFactura.EnContingencia, factura.Estado);
        Assert.False(proveedor.PaqueteEnviado);
    }

    [Fact]
    public async Task EjecutarAsync_ElSinRechazaElPaquete_MarcaRechazadas()
    {
        var (tenant, factura) = CrearTenantConFacturaEnContingencia();
        var proveedor = new ProveedorFiscalFake(disponible: true, exitoso: false);
        var job = new JobEnvioPaquetesContingencia(
            new FacturaRepositoryFake(new[] { factura }), new TenantRepositoryFake(tenant), proveedor,
            new NotificadorWebhookFake(), NullLogger<JobEnvioPaquetesContingencia>.Instance);

        await job.EjecutarAsync();

        Assert.Equal(EstadoFactura.Rechazada, factura.Estado);
    }

    private sealed class ProveedorFiscalFake : IProveedorFiscal
    {
        private readonly bool _disponible;
        private readonly bool _exitoso;
        public bool PaqueteEnviado { get; private set; }

        public ProveedorFiscalFake(bool disponible, bool exitoso)
        {
            _disponible = disponible;
            _exitoso = exitoso;
        }

        public Task<bool> ComunicacionDisponibleAsync(CancellationToken ct = default) => Task.FromResult(_disponible);

        public Task<ResultadoFiscal> EnviarPaqueteContingenciaAsync(IReadOnlyList<Factura> facturas, CancellationToken ct = default)
        {
            PaqueteEnviado = true;
            return Task.FromResult(_exitoso
                ? ResultadoFiscal.Ok("REC-PAQUETE", "VALIDADA", "{}")
                : ResultadoFiscal.Fallo(new[] { new ErrorFiscal("1", "paquete inválido") }, "{}"));
        }

        public Task<FacturaGenerada> GenerarDocumentoAsync(Factura factura, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<ResultadoFiscal> EnviarAsync(Factura factura, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<ResultadoFiscal> ConsultarEstadoAsync(Factura factura, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<ResultadoFiscal> AnularAsync(Factura factura, int codigoMotivo, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class FacturaRepositoryFake : IFacturaRepository
    {
        private readonly List<Factura> _facturas;
        public FacturaRepositoryFake(IEnumerable<Factura> facturas) => _facturas = facturas.ToList();

        public Task<IReadOnlyList<Factura>> ListarEnContingenciaAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Factura>>(_facturas.Where(f => f.Estado == EstadoFactura.EnContingencia).ToList());

        public Task GuardarCambiosAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task<Factura?> ObtenerAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Factura?> ObtenerPorReferenciaAsync(Guid tenantId, string referenciaExterna, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<Factura>> ListarPorEstadoAsync(Guid tenantId, EstadoFactura estado, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<long> SiguienteNumeroAsync(Guid puntoVentaId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarAsync(Factura factura, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class TenantRepositoryFake : ITenantRepository
    {
        private readonly Tenant _tenant;
        public TenantRepositoryFake(Tenant tenant) => _tenant = tenant;

        public Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default) =>
            Task.FromResult(_tenant.Id == tenantId ? _tenant : null);
        public Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarAsync(Tenant tenant, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default) => throw new NotSupportedException();
        public Task GuardarCambiosAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class NotificadorWebhookFake : INotificadorWebhook
    {
        public List<Factura> Notificadas { get; } = new();

        public Task NotificarCambioEstadoAsync(Tenant tenant, Factura factura, CancellationToken ct = default)
        {
            Notificadas.Add(factura);
            return Task.CompletedTask;
        }
    }
}
