# SiatFakeAdapter — desarrollo local sin el SIN

Implementación de `IProveedorFiscal` solo para desarrollo local. Ver la restricción
"SIN ACCESO AL AMBIENTE PILOTO DEL SIN" en `CLAUDE.md`: mientras no haya NIT ni
credenciales SIAT, todo el desarrollo se prueba contra esto, nunca contra el SIN real.

- Usa `CufCalculator` + `XmlFacturaBuilder` + `FacturaXmlDatosFactory` + `CredencialesService`
  reales (mismo código que usará `SiatComputarizadaAdapter`) para generar un CUF y un
  XML válidos contra el XSD oficial — solo la respuesta de *envío* al SIN es simulada.
  El CUFD/código de control ya no son constantes fijas: vienen de `CredencialesService`,
  que a su vez usa `CredencialesClienteFake` (`ISinCredencialesClient` fake) para
  obtenerlos — no representan un CUFD real, pero ejercitan el flujo completo
  (vigencia, renovación, persistencia cifrada del token delegado) igual que lo hará
  el adaptador real. Requiere que la sucursal/punto de venta tenga un token delegado
  registrado (`POST /api/v1/sucursales/{id}/credencial-siat`) — si no, falla con un
  error explícito en vez de inventar uno.
- Comportamiento configurable vía `SiatFakeAdapterOptions`:
  - `SimularSinIndisponible` — fuerza `ComunicacionDisponibleAsync() == false` (contingencia).
  - `PrefijoParaRechazo` (default `"RECHAZAR-"`) — si `Factura.ReferenciaExterna`
    empieza con ese prefijo, `EnviarAsync` simula un rechazo. Permite disparar el
    escenario de rechazo desde el propio request REST sin tocar configuración.
- Registrado en DI (`Program.cs`) como `IProveedorFiscal` mientras no exista
  `SiatComputarizadaAdapter` real. Cuando exista, la selección deberá ser por
  `ModalidadFacturacion`/ambiente (factory), no un reemplazo directo — este adapter
  debería seguir disponible para desarrollo local incluso después.

Ver `Siat/Common/README.md` para el resto de las piezas de Siat/Common.
