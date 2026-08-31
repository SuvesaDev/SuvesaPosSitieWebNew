# Ruta de implementación web: facturación electrónica V4.4

## Relación con el API

La fuente de verdad fiscal es el API y su base de datos. El sitio Blazor no genera claves, consecutivos, XML ni firma documentos; consulta la bandeja fiscal, muestra estados y administra los catálogos que el API autorice.

La especificación completa de datos, reserva de numeración y worker está en el repositorio API, en `docs/RUTA_IMPLEMENTACION_FLUJO_FISCAL_V44.md`. Este documento define el trabajo exclusivo del sitio.

## Objetivo

Incorporar una bandeja para las emisiones originadas en `Venta` y `DevolucionesVenta`, además de las pantallas de mantenimiento de catálogos fiscales. La prioridad es trazabilidad operativa; el PDF queda después de la emisión, respuesta de Hacienda y XML.

## Principios obligatorios

- El sitio llama al API mediante los proxies existentes; no abre conexiones directas a la base de datos.
- Ninguna pantalla, DTO, log ni llamada al navegador contiene usuario de Hacienda, clave, ruta del certificado o contraseña del certificado.
- No exponer XML firmado o respuesta de Hacienda sin el permiso fiscal correspondiente.
- El estado mostrado se refresca desde la API; el navegador no interpreta ni cambia el estado fiscal.
- Los mantenimientos no borran catálogos usados por ventas, series o emisiones; el API es quien aplica esta regla.

## Fase W1 — Contratos y permisos

1. Regenerar contratos del API cuando estén disponibles los endpoints V4.4 de bandeja y mantenimientos.
2. Crear DTOs/ViewModels de lectura para `BandejaFiscalItem`, `DetalleEmisionFiscal` y `EventoFiscal` si no son generados automáticamente.
3. Definir permisos separados:
   - `FacturacionElectronica.VerBandeja`
   - `FacturacionElectronica.VerXml`
   - `FacturacionElectronica.Reintentar`
   - `ParametrosFiscales.Administrar`
   - `Emisores.AdministrarCredenciales`
4. Ocultar botones y rutas sin el permiso, pero confiar siempre en la autorización del API.

## Fase W2 — Bandeja fiscal

Ubicación recomendada: `Views/Documentos/FacturacionElectronica/`.

Pantalla principal `/documentos/facturacion-electronica`:

- Filtros: estado, tipo de documento, origen (Venta/Devolución), emisor, sucursal y período.
- Tabla paginada: fecha, tipo, clave, consecutivo, venta/devolución origen, emisor, sucursal, total, estado, intentos y causa resumida.
- Indicadores claros para `Pendiente`, `En proceso`, `Aceptado`, `Rechazado` y `Error técnico`.
- Enlace al detalle de venta o devolución sólo si el usuario tiene acceso a ese módulo.

Pantalla detalle `/documentos/facturacion-electronica/{clave}`:

- Línea de tiempo de eventos del API.
- Datos del origen y del emisor/sucursal sin credenciales.
- Causa de rechazo/error, respuesta de Hacienda y fecha del último intento.
- Descarga/visualización de XML firmado y respuesta únicamente con `FacturacionElectronica.VerXml`.
- Botón “Reintentar” sólo para `ErrorTecnico` o `ReintentoPendiente`; pide confirmación, llama una sola vez al API y vuelve a consultar el estado. Nunca permite editar clave ni consecutivo.

No añadir todavía generación de PDF: sólo mostrarlo cuando el API informe que existe una ruta/artefacto válido.

## Fase W3 — Flujo visual de ventas y devoluciones

En las pantallas de venta y devolución existentes:

1. Al completar la operación, mostrar que la emisión fue **encolada**, no prometer “aceptada” hasta que Hacienda responda.
2. Mostrar clave y consecutivo cuando el API ya los haya reservado.
3. Ofrecer un acceso a la bandeja filtrado por el documento origen.
4. Para preventas, no ofrecer acciones fiscales de envío.
5. Para tiquete, el flujo debe indicar que requiere opciones de pago registradas; la validación final sigue siendo del API.
6. Para venta a crédito, mostrar plazo y condición de venta proporcionados por el API.

La interfaz no calcula impuestos ni decide si un tipo es FE/TE/NC; consume el resultado del API, que se basa en `TiposFactura.CodigoFE`.

## Fase W4 — Mantenimientos fiscales

Agregar opciones al módulo `Views/Parametros/`, usando una pantalla de lista y formulario consistente con el sitio. Mantenimientos requeridos:

- Tipos de factura: incluir `CodigoFE` y advertir que cambia el tipo fiscal que emite el API.
- Tipos de identificación.
- Emisores: datos públicos y datos fiscales; los campos sensibles nunca se precargan ni se devuelven. El formulario de actualización de credenciales debe mostrar sólo “configurado/no configurado”.
- Sucursales: incluir `NumeroSucursalFE` de tres dígitos.
- Series de facturación: emisor, sucursal, terminal de cinco dígitos, tipo de factura, secuencia, estado y uso (crédito/pago/recibo). No permitir editar una secuencia en uso sin una operación administrativa explícita del API.
- Cajas: presentar el mantenimiento existente `Cajas_Cantidad` bajo el nombre “Cajas”, sin renombrar físicamente tablas desde la web.
- Provincia, cantón y distrito con selección encadenada.
- Configuración de plazos, denominaciones de moneda, impuestos, monedas, formas de pago, tipos de exoneración y tipos de cobro.

Validaciones visuales mínimas:

- `CodigoFE`: dos dígitos del catálogo autorizado.
- Número de sucursal: tres dígitos.
- Terminal: cinco dígitos.
- Identificación y códigos de impuesto/tarifa: formato indicado por el API.
- No mostrar contraseñas, certificados ni valores secretos tras guardarlos.

## Fase W5 — Pruebas

1. Pruebas de componentes para filtros, estados, permisos y confirmación de reintento.
2. Pruebas de integración con el API simulado para paginación, detalle, enlaces al origen y manejo de 403/422.
3. E2E con datos de prueba: venta FE, venta TE con pagos, venta a crédito, devolución/NC, rechazo y error técnico recuperable.
4. Prueba de seguridad: un usuario sin permiso no ve ni obtiene XML, respuesta ni acciones de reintento.

## Criterios de terminado web

- Un usuario autorizado puede encontrar una venta o devolución, entender su estado y llegar a la trazabilidad fiscal desde una sola pantalla.
- La web no crea ni modifica clave/consecutivo, ni reenvía rechazos, ni revela secretos.
- Los mantenimientos entregan datos completos y validados para que el API pueda reservar serie y emitir.
- La bandeja permanece utilizable con errores de Hacienda y muestra el siguiente paso permitido.
