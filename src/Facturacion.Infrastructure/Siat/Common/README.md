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

Pendientes (TODO claude-code):

- `SoapClients/` — clientes generados con dotnet-svcutil desde los WSDL del SIN:
  sincronización de catálogos, códigos (CUIS/CUFD), recepción de facturas,
  operaciones (eventos significativos, puntos de venta)
- `XmlFacturaBuilder.cs` — construcción del XML por documento sector + validación XSD
- `CredencialesService.cs` — obtención/renovación de CUIS (anual) y CUFD (24h)
- `CatalogosService.cs` — sincronización diaria de paramétricas
- `SiatOptions.cs` — endpoints piloto/producción, timeouts, políticas Polly
