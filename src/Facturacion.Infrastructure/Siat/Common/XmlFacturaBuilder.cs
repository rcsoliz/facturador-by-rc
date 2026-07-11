using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Construye y valida el XML del documento sector 1 (factura de compra-venta,
/// modalidad computarizada) contra el XSD oficial del SIN
/// (<c>facturaComputarizadaCompraVenta.xsd</c>, descargado de siatinfo.impuestos.gob.bo
/// y embebido como recurso). El orden de los elementos sigue exactamente la
/// <c>xs:sequence</c> del XSD — no reordenar sin revisar el esquema, porque XSD
/// valida orden estricto.
/// </summary>
public static class XmlFacturaBuilder
{
    private const string RecursoXsd =
        "Facturacion.Infrastructure.Siat.Common.Xsd.facturaComputarizadaCompraVenta.xsd";

    private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    public static string Construir(FacturaXmlDatos datos)
    {
        var cabecera = new XElement("cabecera",
            new XElement("nitEmisor", datos.NitEmisor),
            new XElement("razonSocialEmisor", datos.RazonSocialEmisor),
            new XElement("municipio", datos.Municipio),
            CampoNillable("telefono", datos.Telefono),
            new XElement("numeroFactura", datos.NumeroFactura),
            new XElement("cuf", datos.Cuf),
            new XElement("cufd", datos.Cufd),
            new XElement("codigoSucursal", datos.CodigoSucursal),
            new XElement("direccion", datos.Direccion),
            CampoNillable("codigoPuntoVenta", FormatearEntero(datos.CodigoPuntoVenta)),
            new XElement("fechaEmision", FormatearFecha(datos.FechaEmision)),
            CampoNillable("nombreRazonSocial", datos.NombreRazonSocial),
            new XElement("codigoTipoDocumentoIdentidad", datos.CodigoTipoDocumentoIdentidad),
            new XElement("numeroDocumento", datos.NumeroDocumento),
            CampoNillable("complemento", datos.Complemento),
            new XElement("codigoCliente", datos.CodigoCliente),
            new XElement("codigoMetodoPago", datos.CodigoMetodoPago),
            CampoNillable("numeroTarjeta", FormatearEntero(datos.NumeroTarjeta)),
            new XElement("montoTotal", FormatearMonto(datos.MontoTotal)),
            new XElement("montoTotalSujetoIva", FormatearMonto(datos.MontoTotalSujetoIva)),
            new XElement("codigoMoneda", datos.CodigoMoneda),
            new XElement("tipoCambio", FormatearMonto(datos.TipoCambio)),
            new XElement("montoTotalMoneda", FormatearMonto(datos.MontoTotalMoneda)),
            CampoNillable("montoGiftCard", FormatearMonto(datos.MontoGiftCard)),
            CampoNillable("descuentoAdicional", FormatearMonto(datos.DescuentoAdicional)),
            CampoNillable("codigoExcepcion", FormatearEntero(datos.CodigoExcepcion)),
            CampoNillable("cafc", datos.Cafc),
            new XElement("leyenda", datos.Leyenda),
            new XElement("usuario", datos.Usuario),
            new XElement("codigoDocumentoSector", datos.CodigoDocumentoSector));

        var raiz = new XElement("facturaComputarizadaCompraVenta",
            new XAttribute(XNamespace.Xmlns + "xsi", Xsi.NamespaceName),
            new XAttribute(Xsi + "noNamespaceSchemaLocation", "facturaComputarizadaCompraVenta.xsd"),
            cabecera);

        foreach (var detalle in datos.Detalles)
        {
            raiz.Add(new XElement("detalle",
                new XElement("actividadEconomica", detalle.ActividadEconomica),
                new XElement("codigoProductoSin", detalle.CodigoProductoSin),
                new XElement("codigoProducto", detalle.CodigoProducto),
                new XElement("descripcion", detalle.Descripcion),
                new XElement("cantidad", FormatearMonto(detalle.Cantidad)),
                new XElement("unidadMedida", detalle.UnidadMedida),
                new XElement("precioUnitario", FormatearMonto(detalle.PrecioUnitario)),
                new XElement("montoDescuento", FormatearMonto(detalle.MontoDescuento)),
                new XElement("subTotal", FormatearMonto(detalle.SubTotal)),
                CampoNillable("numeroSerie", detalle.NumeroSerie),
                CampoNillable("numeroImei", detalle.NumeroImei)));
        }

        var documento = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), raiz);

        using var writer = new Utf8StringWriter();
        documento.Save(writer);
        return writer.ToString();
    }

    /// <summary>
    /// Valida el XML contra el XSD oficial embebido. Devuelve la lista de errores
    /// (vacía si es válido) en vez de lanzar, para que el llamador decida qué hacer
    /// (loggear, reintentar generación, marcar la factura con un motivo de rechazo interno).
    /// </summary>
    public static IReadOnlyList<string> Validar(string xml)
    {
        var errores = new List<string>();
        var schemas = new XmlSchemaSet();
        using (var esquema = ObtenerXsdEmbebido())
            schemas.Add(null, XmlReader.Create(esquema));

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
            XmlResolver = null,
        };
        settings.ValidationEventHandler += (_, e) => errores.Add(e.Message);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, settings);
        while (xmlReader.Read())
        {
        }

        return errores;
    }

    private static Stream ObtenerXsdEmbebido() =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream(RecursoXsd)
        ?? throw new InvalidOperationException($"No se encontró el recurso embebido '{RecursoXsd}'.");

    private static XElement CampoNillable(string nombre, string? valor) =>
        valor is null
            ? new XElement(nombre, new XAttribute(Xsi + "nil", "true"))
            : new XElement(nombre, valor);

    private static string? FormatearMonto(decimal? valor) =>
        valor?.ToString("F2", CultureInfo.InvariantCulture);

    private static string FormatearMonto(decimal valor) =>
        valor.ToString("F2", CultureInfo.InvariantCulture);

    private static string? FormatearEntero(long? valor) =>
        valor?.ToString(CultureInfo.InvariantCulture);

    private static string? FormatearEntero(int? valor) =>
        valor?.ToString(CultureInfo.InvariantCulture);

    private static string FormatearFecha(DateTime fecha) =>
        fecha.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
