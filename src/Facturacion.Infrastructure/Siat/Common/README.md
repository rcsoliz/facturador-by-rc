# Siat/Common — compartido entre modalidades

Componentes a implementar aquí (TODO claude-code):

- `SoapClients/` — clientes generados con dotnet-svcutil desde los WSDL del SIN:
  sincronización de catálogos, códigos (CUIS/CUFD), recepción de facturas,
  operaciones (eventos significativos, puntos de venta)
- `CufCalculator.cs` — cálculo del CUF: concatenación de campos, módulo 11,
  conversión a base 16, + código de control del CUFD
- `XmlFacturaBuilder.cs` — construcción del XML por documento sector + validación XSD
- `CredencialesService.cs` — obtención/renovación de CUIS (anual) y CUFD (24h)
- `CatalogosService.cs` — sincronización diaria de paramétricas
- `SiatOptions.cs` — endpoints piloto/producción, timeouts, políticas Polly
