# Siat/Common — compartido entre modalidades

- [x] `CufCalculator.cs` — cálculo del CUF: concatenación de 52 dígitos, dígito
      auto-verificador módulo 11, codificación base 16, + código de control
      del CUFD. Algoritmo verificado contra el ejemplo oficial del SIN
      ("Generación CUF", www.impuestos.gob.bo, 21/11/2018) — ver
      `CufCalculatorTests`. **Ojo**: `CufDatos.Modalidad` usa los códigos SIN
      (1=Electrónica, 2=Computarizada, 3=Manual), que NO coinciden con el
      enum de dominio `ModalidadFacturacion` (ComputarizadaEnLinea=1,
      ElectronicaEnLinea=2) — quien construya `CufDatos` debe mapear
      explícitamente entre ambos, no castear el enum directo.

- [x] `XmlFacturaBuilder.cs` — construcción del XML de `facturaComputarizadaCompraVenta`
      (documento sector 1) + validación contra el XSD oficial del SIN, embebido en
      `Xsd/facturaComputarizadaCompraVenta.xsd` (descargado de siatinfo.impuestos.gob.bo,
      paquete "Factura de Compra y Venta"). Recibe un `FacturaXmlDatos` explícito
      (no lee `Factura` directamente). Fuente de cada campo que el XSD exige y no
      es un dato directo de `Factura` (decidido con Roberto):
        - `codigoMetodoPago` / `numeroTarjeta` → `Factura.CodigoMetodoPago` /
          `Factura.NumeroTarjeta` (nuevos campos de dominio, ver `EmitirFacturaRequest`).
        - `actividadEconomica` (por ítem) → `Sucursal.ActividadEconomica`, un único
          valor por sucursal repetido en todos los `detalle` de la factura (no hay
          override por ítem en v1).
        - `leyenda` / `usuario` → `SiatOptions.LeyendaFacturaDefault` /
          `SiatOptions.UsuarioSistema` (config fija por instalación, no por tenant).
        - `codigoCliente` → se resuelve en el adaptador como
          `Factura.NumeroDocumentoComprador` (sin campo nuevo).
      Ver `XmlFacturaBuilderTests`: 7/7 en verde, incluyendo un caso que reproduce
      byte a byte los valores del ejemplo oficial del SIN y valida contra el XSD real.
- [x] `SiatOptions.cs` — v1: solo `LeyendaFacturaDefault` y `UsuarioSistema` (lo que
      `XmlFacturaBuilder` necesita hoy). Endpoints piloto/producción, timeouts y
      políticas Polly se agregan cuando se implementen los clientes SOAP.
- [x] `FacturaXmlDatosFactory.cs` — arma `FacturaXmlDatos` combinando `Factura` +
      `Tenant` + `Sucursal` + `PuntoVenta` + CUF/CUFD + `SiatOptions`. Compartido
      entre `Siat/Fake/SiatFakeAdapter` (ya lo usa) y el futuro
      `SiatComputarizadaAdapter`, para no duplicar este mapeo.

Ver también `Siat/Fake/README.md`: `SiatFakeAdapter`, la implementación de
`IProveedorFiscal` para desarrollo local (sin el SIN) que ya ejercita todo lo de
arriba end-to-end — ver `SiatFakeAdapterTests` y `EmisionEndToEndTests`.

Pendientes (TODO claude-code):

- `SoapClients/` — clientes generados con dotnet-svcutil desde los WSDL del SIN:
  sincronización de catálogos, códigos (CUIS/CUFD), recepción de facturas,
  operaciones (eventos significativos, puntos de venta)
- `CredencialesService.cs` — obtención/renovación de CUIS (anual) y CUFD (24h)
- `CatalogosService.cs` — sincronización diaria de paramétricas
