# SiatElectronicaAdapter — v1.5

Modalidad Electrónica en Línea. Reutiliza todo Siat/Common (XML builder, CUF, CUFD,
catálogos, SOAP) y agrega:

- Firma XMLDSig del XML con `System.Security.Cryptography.Xml.SignedXml`
- Gestión de certificados digitales por tenant (cifrados en reposo)

No implementar hasta cerrar v1.
