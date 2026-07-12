using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;
using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Tests;

public class CredencialesServiceTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SucursalId = Guid.NewGuid();
    private static readonly Guid PuntoVentaId = Guid.NewGuid();

    [Fact]
    public async Task ObtenerCufdVigenteAsync_SinCredencialRegistrada_LanzaExcepcion()
    {
        var servicio = new CredencialesService(
            new CredencialSiatRepositoryFake(), new SinCredencialesClienteFake(), new ProteccionDatosFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.ObtenerCufdVigenteAsync(TenantId, SucursalId, PuntoVentaId, "1003579028", 0, 0));
    }

    [Fact]
    public async Task ObtenerCufdVigenteAsync_CuisYCufdInexistentes_RenuevaAmbosYPersiste()
    {
        var credencial = new CredencialSiat(TenantId, SucursalId, PuntoVentaId, new ProteccionDatosFake().Cifrar("token-plano"));
        var repo = new CredencialSiatRepositoryFake(credencial);
        var cliente = new SinCredencialesClienteFake();
        var servicio = new CredencialesService(repo, cliente, new ProteccionDatosFake());

        var (cufd, codigoControl) = await servicio.ObtenerCufdVigenteAsync(
            TenantId, SucursalId, PuntoVentaId, "1003579028", 0, 0);

        Assert.Equal(1, cliente.LlamadasCuis);
        Assert.Equal(1, cliente.LlamadasCufd);
        Assert.Equal(cliente.UltimoCufdDevuelto!.Cufd, cufd);
        Assert.Equal(cliente.UltimoCufdDevuelto.CodigoControl, codigoControl);
        Assert.True(repo.GuardadoLlamado);
    }

    [Fact]
    public async Task ObtenerCufdVigenteAsync_CuisVigenteCufdVencido_SoloRenuevaCufd()
    {
        var credencial = new CredencialSiat(TenantId, SucursalId, PuntoVentaId, new ProteccionDatosFake().Cifrar("token-plano"));
        credencial.ActualizarCuis("CUIS-VIGENTE", DateTime.UtcNow.AddMonths(6));
        credencial.ActualizarCufd("CUFD-VENCIDO", "000", DateTime.UtcNow.AddHours(-1));

        var repo = new CredencialSiatRepositoryFake(credencial);
        var cliente = new SinCredencialesClienteFake();
        var servicio = new CredencialesService(repo, cliente, new ProteccionDatosFake());

        await servicio.ObtenerCufdVigenteAsync(TenantId, SucursalId, PuntoVentaId, "1003579028", 0, 0);

        Assert.Equal(0, cliente.LlamadasCuis);
        Assert.Equal(1, cliente.LlamadasCufd);
    }

    [Fact]
    public async Task ObtenerCufdVigenteAsync_CuisYCufdVigentes_NoLlamaAlCliente()
    {
        var credencial = new CredencialSiat(TenantId, SucursalId, PuntoVentaId, new ProteccionDatosFake().Cifrar("token-plano"));
        credencial.ActualizarCuis("CUIS-VIGENTE", DateTime.UtcNow.AddMonths(6));
        credencial.ActualizarCufd("CUFD-VIGENTE", "123", DateTime.UtcNow.AddHours(12));

        var repo = new CredencialSiatRepositoryFake(credencial);
        var cliente = new SinCredencialesClienteFake();
        var servicio = new CredencialesService(repo, cliente, new ProteccionDatosFake());

        var (cufd, codigoControl) = await servicio.ObtenerCufdVigenteAsync(
            TenantId, SucursalId, PuntoVentaId, "1003579028", 0, 0);

        Assert.Equal(0, cliente.LlamadasCuis);
        Assert.Equal(0, cliente.LlamadasCufd);
        Assert.Equal("CUFD-VIGENTE", cufd);
        Assert.Equal("123", codigoControl);
    }

    [Fact]
    public async Task RegistrarTokenDelegadoAsync_SinCredencialPrevia_CreaUnaNueva()
    {
        var repo = new CredencialSiatRepositoryFake();
        var proteccion = new ProteccionDatosFake();
        var servicio = new CredencialesService(repo, new SinCredencialesClienteFake(), proteccion);

        await servicio.RegistrarTokenDelegadoAsync(TenantId, SucursalId, PuntoVentaId, "token-plano");

        var credencial = await repo.ObtenerAsync(TenantId, SucursalId, PuntoVentaId);
        Assert.NotNull(credencial);
        Assert.Equal(proteccion.Cifrar("token-plano"), credencial!.TokenDelegadoCifrado);
        Assert.NotEqual("token-plano", credencial.TokenDelegadoCifrado); // nunca en texto plano
    }

    [Fact]
    public async Task RegistrarTokenDelegadoAsync_ConCredencialPrevia_RotaElToken()
    {
        var proteccion = new ProteccionDatosFake();
        var credencial = new CredencialSiat(TenantId, SucursalId, PuntoVentaId, proteccion.Cifrar("token-viejo"));
        var repo = new CredencialSiatRepositoryFake(credencial);
        var servicio = new CredencialesService(repo, new SinCredencialesClienteFake(), proteccion);

        await servicio.RegistrarTokenDelegadoAsync(TenantId, SucursalId, PuntoVentaId, "token-nuevo");

        var actualizada = await repo.ObtenerAsync(TenantId, SucursalId, PuntoVentaId);
        Assert.Equal(proteccion.Cifrar("token-nuevo"), actualizada!.TokenDelegadoCifrado);
        Assert.Same(credencial, actualizada); // misma entidad, no una nueva
    }

    private sealed class CredencialSiatRepositoryFake : ICredencialSiatRepository
    {
        private CredencialSiat? _credencial;
        public bool GuardadoLlamado { get; private set; }

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

        public Task<IReadOnlyList<CredencialSiat>> ListarTodasAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<CredencialSiat>>(_credencial is null ? Array.Empty<CredencialSiat>() : new[] { _credencial });

        public Task GuardarCambiosAsync(CancellationToken ct = default)
        {
            GuardadoLlamado = true;
            return Task.CompletedTask;
        }
    }

    private sealed class SinCredencialesClienteFake : ISinCredencialesClient
    {
        public int LlamadasCuis { get; private set; }
        public int LlamadasCufd { get; private set; }
        public CufdObtenido? UltimoCufdDevuelto { get; private set; }

        public Task<CuisObtenido> ObtenerCuisAsync(SolicitudCredencialSin solicitud, CancellationToken ct = default)
        {
            LlamadasCuis++;
            return Task.FromResult(new CuisObtenido("CUIS-NUEVO", DateTime.UtcNow.AddYears(1)));
        }

        public Task<CufdObtenido> ObtenerCufdAsync(
            SolicitudCredencialSin solicitud, string cuis, CancellationToken ct = default)
        {
            LlamadasCufd++;
            UltimoCufdDevuelto = new CufdObtenido("CUFD-NUEVO", "CTRL-NUEVO", DateTime.UtcNow.AddHours(24));
            return Task.FromResult(UltimoCufdDevuelto);
        }
    }

    private sealed class ProteccionDatosFake : IProteccionDatos
    {
        public string Cifrar(string textoPlano) => $"CIFRADO:{textoPlano}";
        public string Descifrar(string textoCifrado) => textoCifrado.Replace("CIFRADO:", "");
    }
}
