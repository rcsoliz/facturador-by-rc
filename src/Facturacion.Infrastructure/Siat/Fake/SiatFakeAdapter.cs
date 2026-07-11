using Facturacion.Domain.Entities;
using Facturacion.Domain.Models;
using Facturacion.Domain.Ports;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.Siat.Common;
using Microsoft.Extensions.Options;

namespace Facturacion.Infrastructure.Siat.Fake;

/// <summary>
/// Implementación de <see cref="IProveedorFiscal"/> para desarrollo local, sin tocar
/// el SIN (ver restricción "SIN ACCESO AL AMBIENTE PILOTO DEL SIN" en CLAUDE.md).
/// Genera un XML y CUF reales (mismo <see cref="CufCalculator"/> y
/// <see cref="XmlFacturaBuilder"/> que usará el adaptador real) pero simula la
/// respuesta de envío al SIN de forma configurable — ver <see cref="SiatFakeAdapterOptions"/>.
/// Permite probar el flujo completo API → cola → worker → webhook end-to-end.
/// </summary>
public class SiatFakeAdapter : IProveedorFiscal
{
    // Códigos SIN (no el enum de dominio ModalidadFacturacion — ver discrepancia
    // documentada en CufDatos): 2 = Computarizada.
    private const int ModalidadComputarizadaSin = 2;
    private const int TipoEmisionOnline = 1;
    private const int CodigoDocumentoFiscalFactura = 1;

    // CUFD/código de control fijos: CredencialesService (CUIS/CUFD) todavía no
    // existe (roadmap v1, pendiente). Cuando exista, este adaptador seguirá siendo
    // fake pero podría consumirlo igual que el real — no es una razón para bloquear
    // esto hasta entonces.
    private const string CufdFake = "CUFD-FAKE-PENDIENTE-CREDENCIALES-SERVICE";
    private const string CodigoControlCufdFake = "000000000000000";

    private readonly ITenantRepository _tenants;
    private readonly SiatOptions _siatOptions;
    private readonly SiatFakeAdapterOptions _fakeOptions;

    public SiatFakeAdapter(
        ITenantRepository tenants, IOptions<SiatOptions> siatOptions, IOptions<SiatFakeAdapterOptions> fakeOptions)
    {
        _tenants = tenants;
        _siatOptions = siatOptions.Value;
        _fakeOptions = fakeOptions.Value;
    }

    public async Task<FacturaGenerada> GenerarDocumentoAsync(Factura factura, CancellationToken ct = default)
    {
        var tenant = await _tenants.ObtenerAsync(factura.TenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {factura.TenantId} no encontrado.");
        var sucursal = tenant.Sucursales.SingleOrDefault(s => s.Id == factura.SucursalId)
            ?? throw new InvalidOperationException(
                $"Sucursal {factura.SucursalId} no encontrada para el tenant {factura.TenantId}.");
        var puntoVenta = sucursal.PuntosVenta.SingleOrDefault(p => p.Id == factura.PuntoVentaId);

        var cufDatos = new CufDatos(
            Nit: tenant.Nit.Valor,
            FechaHoraEmision: factura.FechaEmision,
            CodigoSucursal: sucursal.CodigoSiat,
            Modalidad: ModalidadComputarizadaSin,
            TipoEmision: TipoEmisionOnline,
            CodigoDocumentoFiscal: CodigoDocumentoFiscalFactura,
            CodigoDocumentoSector: factura.CodigoDocumentoSector,
            NumeroFactura: factura.NumeroFactura,
            PuntoVenta: puntoVenta?.CodigoSiat ?? 0,
            CodigoControlCufd: CodigoControlCufdFake);
        var cufValor = CufCalculator.Calcular(cufDatos);

        var datosXml = FacturaXmlDatosFactory.Crear(
            factura, tenant, sucursal, puntoVenta, cufValor, CufdFake, _siatOptions);
        var xml = XmlFacturaBuilder.Construir(datosXml);

        var errores = XmlFacturaBuilder.Validar(xml);
        if (errores.Count > 0)
            throw new InvalidOperationException(
                $"El XML generado no cumple el XSD del SIN: {string.Join("; ", errores)}");

        return new FacturaGenerada(new Cuf(cufValor), xml);
    }

    public Task<ResultadoFiscal> EnviarAsync(Factura factura, CancellationToken ct = default)
    {
        if (factura.ReferenciaExterna.StartsWith(_fakeOptions.PrefijoParaRechazo, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ResultadoFiscal.Fallo(
                new[] { new ErrorFiscal("905", "Rechazo simulado por SiatFakeAdapter (fines de prueba).") },
                "{\"simulado\":true,\"proveedor\":\"SiatFakeAdapter\",\"resultado\":\"RECHAZADA\"}"));
        }

        return Task.FromResult(ResultadoFiscal.Ok(
            codigoRecepcion: $"FAKE-{Guid.NewGuid():N}",
            codigoEstado: "VALIDADA",
            raw: "{\"simulado\":true,\"proveedor\":\"SiatFakeAdapter\",\"resultado\":\"VALIDADA\"}"));
    }

    public Task<ResultadoFiscal> ConsultarEstadoAsync(Factura factura, CancellationToken ct = default) =>
        Task.FromResult(ResultadoFiscal.Ok(
            factura.CodigoRecepcionSin ?? $"FAKE-{Guid.NewGuid():N}",
            "VALIDADA",
            "{\"simulado\":true,\"proveedor\":\"SiatFakeAdapter\"}"));

    public Task<ResultadoFiscal> AnularAsync(Factura factura, int codigoMotivo, CancellationToken ct = default) =>
        Task.FromResult(ResultadoFiscal.Ok(
            $"FAKE-ANULACION-{Guid.NewGuid():N}",
            "ANULADA",
            "{\"simulado\":true,\"proveedor\":\"SiatFakeAdapter\"}"));

    public Task<ResultadoFiscal> EnviarPaqueteContingenciaAsync(
        IReadOnlyList<Factura> facturas, CancellationToken ct = default) =>
        Task.FromResult(ResultadoFiscal.Ok(
            $"FAKE-PAQUETE-{Guid.NewGuid():N}",
            "VALIDADA",
            "{\"simulado\":true,\"proveedor\":\"SiatFakeAdapter\"}"));

    public Task<bool> ComunicacionDisponibleAsync(CancellationToken ct = default) =>
        Task.FromResult(!_fakeOptions.SimularSinIndisponible);
}
