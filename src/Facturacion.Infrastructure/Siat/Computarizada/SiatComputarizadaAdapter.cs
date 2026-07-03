using Facturacion.Domain.Entities;
using Facturacion.Domain.Models;
using Facturacion.Domain.Ports;

namespace Facturacion.Infrastructure.Siat.Computarizada;

/// <summary>
/// Adaptador v1 — Facturación Computarizada en Línea (SIAT Bolivia).
///
/// Flujo por factura:
///   1. Obtener CUFD vigente (cache Redis/DB; renovación programada en Workers)
///   2. Construir XML del documento sector (XmlFacturaBuilder) y validar contra XSD
///   3. Calcular CUF (CufCalculator: módulo 11 + base 16 + código de control del CUFD)
///   4. Comprimir gzip + hash SHA256 => hashArchivo (huella, SIN firma XMLDSig)
///   5. Enviar vía SOAP (recepcionFactura) con Polly: reintentos + circuit breaker
///
/// TODO(claude-code):
///   - Generar clientes SOAP desde los WSDL del SIN con dotnet-svcutil (ambiente piloto)
///   - Implementar CufCalculator + XmlFacturaBuilder en Siat/Common
///   - Servicio de sincronización de catálogos/paramétricas (diaria)
///   - Gestión CUIS/CUFD (obtención y renovación) en Siat/Common/CredencialesService
/// </summary>
public class SiatComputarizadaAdapter : IProveedorFiscal
{
    public Task<FacturaGenerada> GenerarDocumentoAsync(Factura factura, CancellationToken ct = default)
        => throw new NotImplementedException("v1: XmlFacturaBuilder + CufCalculator pendientes.");

    public Task<ResultadoFiscal> EnviarAsync(Factura factura, CancellationToken ct = default)
        => throw new NotImplementedException("v1: cliente SOAP recepcionFactura pendiente.");

    public Task<ResultadoFiscal> ConsultarEstadoAsync(Factura factura, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<ResultadoFiscal> AnularAsync(Factura factura, int codigoMotivo, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<ResultadoFiscal> EnviarPaqueteContingenciaAsync(IReadOnlyList<Factura> facturas, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<bool> ComunicacionDisponibleAsync(CancellationToken ct = default)
        => throw new NotImplementedException("v1: verificarComunicacion SOAP pendiente.");
}
