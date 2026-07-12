# RepresentacionGrafica — PDF + QR de la factura

Implementación de `Domain/Ports/IGeneradorRepresentacionGrafica`, consumida por
`GET /api/v1/facturas/{id}/pdf` vía `Application/Queries/ObtenerRepresentacionGraficaHandler.cs`.

- `GeneradorRepresentacionGraficaQuestPdf.cs` — genera el PDF con QuestPDF
  (fluent API). Requiere `Factura.Cuf` no nulo — solo tiene sentido para
  facturas ya generadas (`MarcarGenerada` ya corrió, sea el resultado final
  Validada o Rechazada: el CUF se calcula localmente ANTES de enviar al SIN,
  así que una factura rechazada también tiene CUF y también puede mostrar su
  representación gráfica). El handler devuelve `DomainException("FACTURA_SIN_CUF", ...)`
  si todavía está `Pendiente`/`EnContingencia` (400), o `null` si la factura
  no existe (404).
- `FacturaPdfDatos.cs` + `FacturaPdfDatosFactory.cs` — mismo patrón que
  `Siat/Common/FacturaXmlDatosFactory.cs`: combina `Factura` + `Tenant` +
  `Sucursal` + `PuntoVenta` + `SiatOptions`, pero solo con los campos que la
  representación gráfica necesita mostrar (no los que existen únicamente para
  el XSD del XML).
- `GeneradorQr.cs` — wrapper sobre QRCoder. Usa `PngByteQRCode` (no `QRCode`/
  `System.Drawing.Common`, que no es multiplataforma) — relevante si el
  servicio termina corriendo en un contenedor Linux.
- **Contenido del QR**: `{SiatOptions.UrlBaseVerificacionQr}?nit={nit}&cuf={cuf}&numero={numeroFactura}`.
  Formato tomado de la documentación pública del SIN (siatinfo.impuestos.gob.bo,
  "Código Respuesta Rápida (QR)"): `https://pilotosiat.impuestos.gob.bo/consulta/QR?nit=valorNit&cuf=valorCuf&numero=valorNroFactura&t=valorTamaño`
  — implementado sin el parámetro `t` (tamaño) porque su semántica exacta no
  está confirmada; revisar/ajustar cuando haya acceso real al SIN (mismo
  principio que el resto del proyecto: solo cambio de configuración en
  `SiatOptions`, no de código). **No se hizo ninguna conexión al SIN para
  esto** — es solo investigación de documentación pública, no acceso al
  ambiente piloto.
- **Licencia QuestPDF**: se fija `QuestPDF.Settings.License = LicenseType.Community`
  en el constructor estático de `GeneradorRepresentacionGraficaQuestPdf` (no
  solo en `Program.cs`) para que también aplique cuando la clase se usa
  directo desde tests.

Verificado end-to-end con curl real (API + Postgres reales, sin el SIN):
factura validada → PDF descargado (200, `Content-Type: application/pdf`,
71KB, cabecera `%PDF-1.4` real); factura rechazada → también genera PDF
(200, tiene CUF); factura inexistente → 404.

Pendiente (TODO claude-code): confirmar el parámetro `t` del QR y la URL de
producción cuando haya acceso real al SIN; hoy el layout es funcional pero
básico (A4, sin logo ni monto en letras) — si Roberto pide un formato más
cercano al ticket/factura impresa tradicional, es un ajuste de layout en
`GeneradorRepresentacionGraficaQuestPdf.cs`, no de arquitectura.
