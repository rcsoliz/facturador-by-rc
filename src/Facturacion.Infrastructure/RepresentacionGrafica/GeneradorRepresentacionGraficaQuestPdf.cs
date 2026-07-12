using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;
using Facturacion.Infrastructure.Siat.Common;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Infrastructure.RepresentacionGrafica;

/// <summary>
/// Representación gráfica (PDF + QR) de una factura validada, vía QuestPDF.
/// Requiere <see cref="Factura.Cuf"/> — solo tiene sentido para facturas ya
/// validadas por el SIN (o por <c>SiatFakeAdapter</c> en desarrollo).
/// </summary>
public class GeneradorRepresentacionGraficaQuestPdf : IGeneradorRepresentacionGrafica
{
    private readonly SiatOptions _opciones;

    static GeneradorRepresentacionGraficaQuestPdf()
    {
        // Se fija acá (no solo en Program.cs) para que también aplique cuando
        // esta clase se usa directo desde tests, sin pasar por el host de la API.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public GeneradorRepresentacionGraficaQuestPdf(IOptions<SiatOptions> opciones) => _opciones = opciones.Value;

    public Task<byte[]> GenerarPdfAsync(
        Factura factura, Tenant tenant, Sucursal sucursal, PuntoVenta? puntoVenta, CancellationToken ct = default)
    {
        var datos = FacturaPdfDatosFactory.Crear(factura, tenant, sucursal, puntoVenta, _opciones);
        var qrPng = GeneradorQr.GenerarPng(datos.UrlVerificacionQr);

        var pdf = Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(9));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text(datos.RazonSocialEmisor).FontSize(14).Bold();
                    columna.Item().Text($"NIT: {datos.NitEmisor}");
                    columna.Item().Text($"{datos.Direccion} — {datos.Municipio}");
                    columna.Item().PaddingTop(10).Text("FACTURA").FontSize(12).Bold();
                    columna.Item().Text(
                        $"N° {datos.NumeroFactura} — Sucursal {datos.CodigoSucursal}" +
                        (datos.CodigoPuntoVenta is { } cpv ? $" — Punto de venta {cpv}" : ""));
                    columna.Item().Text($"Fecha de emisión: {datos.FechaEmision:dd/MM/yyyy HH:mm:ss}");
                    columna.Item().PaddingBottom(5).LineHorizontal(0.5f);
                });

                pagina.Content().Column(columna =>
                {
                    columna.Item().PaddingBottom(5).Text(texto =>
                    {
                        texto.Span("Cliente: ").Bold();
                        texto.Span(datos.NombreRazonSocialComprador);
                    });
                    columna.Item().PaddingBottom(10).Text(texto =>
                    {
                        texto.Span("NIT/CI: ").Bold();
                        texto.Span(datos.NumeroDocumentoComprador + (datos.Complemento is null ? "" : $"-{datos.Complemento}"));
                    });

                    columna.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(4);
                            columnas.RelativeColumn(1);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                            columnas.RelativeColumn(2);
                        });

                        tabla.Header(encabezado =>
                        {
                            encabezado.Cell().Text("Código").Bold();
                            encabezado.Cell().Text("Descripción").Bold();
                            encabezado.Cell().Text("Cant.").Bold();
                            encabezado.Cell().Text("P. Unitario").Bold();
                            encabezado.Cell().Text("Descuento").Bold();
                            encabezado.Cell().Text("Subtotal").Bold();
                            encabezado.Cell().ColumnSpan(6).PaddingTop(2).LineHorizontal(0.5f);
                        });

                        foreach (var detalle in datos.Detalles)
                        {
                            tabla.Cell().Text(detalle.CodigoProducto);
                            tabla.Cell().Text(detalle.Descripcion);
                            tabla.Cell().Text(detalle.Cantidad.ToString("0.##"));
                            tabla.Cell().Text(detalle.PrecioUnitario.ToString("0.00"));
                            tabla.Cell().Text(detalle.MontoDescuento.ToString("0.00"));
                            tabla.Cell().Text(detalle.SubTotal.ToString("0.00"));
                        }
                    });

                    columna.Item().PaddingTop(10).AlignRight().Text($"MONTO TOTAL: {datos.MontoTotal:0.00}").FontSize(11).Bold();
                });

                pagina.Footer().Column(columna =>
                {
                    columna.Item().PaddingTop(10).Row(fila =>
                    {
                        fila.ConstantItem(80).Image(qrPng);
                        fila.RelativeItem().PaddingLeft(10).Column(texto =>
                        {
                            texto.Item().Text($"CUF: {datos.Cuf}").FontSize(7);
                            texto.Item().Text(
                                "Este documento es la representación gráfica de un documento fiscal digital, " +
                                "emitido dentro de la modalidad de Facturación en Línea.").FontSize(7);
                            texto.Item().Text(datos.Leyenda).FontSize(7);
                        });
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdf);
    }
}
