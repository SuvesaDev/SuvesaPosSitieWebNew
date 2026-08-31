# Seguimiento de implementación web — flujo fiscal V4.4

## Objetivo

Incorporar al sitio `SuvesaPosSitieWebNew` los mantenimientos fiscales ya expuestos por la API V4.4 y, después, la bandeja de comprobantes electrónicos. El sitio no debe recibir, mostrar ni conservar secretos fiscales fuera de los formularios de escritura autorizados.

## Estado inicial — 2026-08-31

- [x] Se revisó el sitio y sus contratos generados.
- [x] Se confirmó que no existe cliente, proxy ni pantalla para `/api/comprobantes-electronicos/v44/bandeja`.
- [x] Se confirmó que `Documentos Emitidos` consulta las rutas históricas de ventas; no reemplaza la bandeja fiscal.
- [x] Se identificó que los contratos OpenAPI generados del sitio todavía no incluyen las rutas V4.4 ni los DTOs de los nuevos mantenimientos.
- [x] Se verificó el Swagger local del API: contiene las rutas V4.4 de bandeja y mantenimientos.
- [x] Se detectó que regenerar el cliente OpenAPI completo cambia modelos heredados y rompe las pantallas existentes. Se conservaron los contratos heredados y los contratos V4.4 se incorporarán aislados por módulo.

## Entregas y pendientes

### 1. Catálogos fiscales base

- [x] Tipos de factura: pantalla, menú, permisos existentes por pantalla, alta y edición de código FE y condiciones de venta.
- [x] Tipos de identificación: alta, edición y deshabilitación lógica.
- [x] Impuestos: código, tarifa, porcentaje, auditoría del usuario y deshabilitación lógica.
- [x] Formas de pago, tipos de cobro y tipos de exoneración: altas y edición; los contratos V4.4 se mantienen aislados del cliente OpenAPI heredado.
- [x] Monedas y denominaciones de moneda: pantallas, menú, altas, edición y deshabilitación validadas mediante compilación limpia.
- [x] Configuración de plazos: mantenimiento completo validado mediante compilación limpia.
- [~] Geografía fiscal: bloqueada por contrato del API. Las rutas de crear/editar exigen `CodigoFE` y relaciones padre, pero los listados existentes solo devuelven DTOs históricos sin `CodigoFE`; falta un listado de mantenimiento antes de habilitar edición segura en la WEB.

### 2. Configuración de emisión

- [x] Sucursales: alta, listado y edición V4.4 con número fiscal FE de tres dígitos, validados mediante compilación limpia.
- [x] Emisores: actualización de datos públicos y formulario separado de credenciales/certificado, sin relectura de secretos; validado mediante compilación limpia.
- [x] Series de facturación: mantenimiento validado con emisor, sucursal, terminal, tipo FE, secuencia y habilitación V4.4.

### 3. Bandeja fiscal V4.4

- [x] Cliente, DTOs, interfaz y proxy para listado, detalle, XML, respuesta y reintento; validados mediante compilación limpia.
- [x] Pantalla con filtros, paginación, estado, causa e historial de eventos; validada mediante compilación limpia.
- [x] XML y respuesta quedan bajo `Ver`; reintento bajo `Modificar`, usando el modelo estándar de permisos de pantalla.
- [x] Entrada de menú y permisos de pantalla/acción aplicados.

### 4. Validación

- [x] Pruebas de proxies y permisos: suite local aprobada (69 unitarias y 39 E2E); 13 E2E permanecen omitidas por requerir servicios/datos externos.
- [~] Pruebas de interfaz para altas, edición, deshabilitación y validaciones: compilación y pruebas automatizadas completadas; el recorrido manual autenticado quedó preparado en `PRUEBAS_MANUALES_FISCAL_V44_WEB.md`.
- [ ] Prueba integrada contra API y sandbox cuando los datos fiscales estén configurados.

## Decisiones vigentes

- Se implementa en orden: catálogos base -> emisor/sucursal/series -> bandeja.
- La pantalla histórica `Documentos Emitidos` se conserva; la bandeja V4.4 es una pantalla independiente.
- Los permisos web se registran por pantalla y acción (`Ver`, `Crear`, `Modificar`, `Borrar`). En la bandeja V4.4, XML/respuesta corresponden a `Ver` y reintento a `Modificar`.
- No se habilita una serie V4.4 ni se envían documentos desde el sitio como parte de esta fase de interfaz.
- Los módulos V4.4 usan contratos propios y pequeños sobre el `HttpClient` autenticado del sitio hasta que el OpenAPI pueda regenerarse sin renombrar los modelos del sistema heredado.
