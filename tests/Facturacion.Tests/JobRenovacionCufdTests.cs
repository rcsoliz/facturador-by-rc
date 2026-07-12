using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Seguridad;
using Facturacion.Infrastructure.Siat.Common;
using Facturacion.Infrastructure.Siat.Fake;
using Microsoft.Extensions.Logging.Abstractions;

namespace Facturacion.Tests;

public class JobRenovacionCufdTests
{
    [Fact]
    public async Task EjecutarAsync_CredencialConCufdVencido_LaRenueva()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. TEST 123", "La Paz", "451010");

        var proteccion = new ProteccionDatosAes(new ProteccionDatosOptions("MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="));
        var credencial = new CredencialSiat(tenant.Id, sucursal.Id, null, proteccion.Cifrar("token-plano"));
        credencial.ActualizarCuis("CUIS-VIGENTE", DateTime.UtcNow.AddMonths(6));
        credencial.ActualizarCufd("CUFD-VENCIDO", "000", DateTime.UtcNow.AddHours(-1));

        var repoCredenciales = new CredencialSiatRepositoryFake(new[] { credencial });
        var repoTenants = new TenantRepositoryFake(tenant);
        var servicio = new CredencialesService(repoCredenciales, new CredencialesClienteFake(), proteccion);
        var job = new JobRenovacionCufd(repoCredenciales, repoTenants, servicio, NullLogger<JobRenovacionCufd>.Instance);

        await job.EjecutarAsync();

        Assert.NotEqual("CUFD-VENCIDO", credencial.Cufd);
        Assert.True(credencial.CufdVigente(DateTime.UtcNow));
    }

    [Fact]
    public async Task EjecutarAsync_UnaCredencialFalla_NoDetieneLasDemas()
    {
        var tenant = new Tenant("Empresa Prueba", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. TEST 123", "La Paz", "451010");
        var proteccion = new ProteccionDatosAes(new ProteccionDatosOptions("MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="));

        var credencialHuerfana = new CredencialSiat(Guid.NewGuid(), Guid.NewGuid(), null, proteccion.Cifrar("token"));
        var credencialValida = new CredencialSiat(tenant.Id, sucursal.Id, null, proteccion.Cifrar("token-plano"));
        credencialValida.ActualizarCuis("CUIS-VIGENTE", DateTime.UtcNow.AddMonths(6));
        credencialValida.ActualizarCufd("CUFD-VENCIDO", "000", DateTime.UtcNow.AddHours(-1));

        var repoCredenciales = new CredencialSiatRepositoryFake(new[] { credencialHuerfana, credencialValida });
        var repoTenants = new TenantRepositoryFake(tenant);
        var servicio = new CredencialesService(repoCredenciales, new CredencialesClienteFake(), proteccion);
        var job = new JobRenovacionCufd(repoCredenciales, repoTenants, servicio, NullLogger<JobRenovacionCufd>.Instance);

        await job.EjecutarAsync();

        Assert.True(credencialValida.CufdVigente(DateTime.UtcNow));
    }

    private sealed class CredencialSiatRepositoryFake : ICredencialSiatRepository
    {
        private readonly List<CredencialSiat> _credenciales;
        public CredencialSiatRepositoryFake(IEnumerable<CredencialSiat> credenciales) => _credenciales = credenciales.ToList();

        public Task<CredencialSiat?> ObtenerAsync(Guid tenantId, Guid sucursalId, Guid? puntoVentaId, CancellationToken ct = default) =>
            Task.FromResult(_credenciales.SingleOrDefault(
                c => c.TenantId == tenantId && c.SucursalId == sucursalId && c.PuntoVentaId == puntoVentaId));

        public Task AgregarAsync(CredencialSiat credencial, CancellationToken ct = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<CredencialSiat>> ListarTodasAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<CredencialSiat>>(_credenciales);

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
}
