# Bandeja unificada de documentos — Plan de trabajo (Sitio Web)

> Objetivo: **una sola pantalla** que reemplace *Documentos Emitidos* y
> *Bandeja Fiscal V4.4*, con pestañas **Preventas · Facturas · Notas de Crédito ·
> Consignaciones**. Cada pestaña es una tabla con columnas propias; desde
> Facturas se puede lanzar la devolución. Preventas y Consignaciones se ven igual
> pero **sin las columnas de documentos fiscales**. Solo análisis y plan; no toca
> código.

---

## 1. Estado actual

### 1.1 Pantallas y menú
| Pantalla | Ruta | Código menú | Componente | Proxy |
|---|---|---|---|---|
| Documentos Emitidos | `/initial/documents` | `INICIO.DOCUMENTOS_EMITIDOS` | `Views/Documentos/Emitidos.razor` | `IDocumentosEmitidos` (envuelve `IVentaApiCliente`) |
| Bandeja Fiscal V4.4 | `/invoices/fiscal-tray` | `INICIO.BANDEJA_FISCAL_V4_4` | `Views/Facturacion/BandejaFiscal.razor` | `IBandejaFiscal` (HTTP directo a `api/comprobantes-electronicos/v44/...`) |

- **Emitidos.razor**: filtro por fechas o número; rejilla `FacturaDTO`
  (Número, Fecha, Cliente, Tipo, Cajero, Total) + offcanvas de detalle (totales +
  clave con "copiar"). Solo lectura. Hoy `Cliente`, `Tipo`, `Cajero` llegan
  vacíos porque el API no los mapea (`ParseFacturaDTO`).
- **BandejaFiscal.razor**: filtro por clave/estado; tabla paginada
  (`Clave, Tipo, Estado, Intentos`) + acciones **Detalle** (modal con eventos,
  "Ver XML firmado", "Ver respuesta Hacienda") y **Reintentar** (solo
  `ErrorTecnico`, gateada con `Sesion.Puede(Titulo, Modificar)`).

### 1.2 Piezas reutilizables ya existentes
- Devolución (nota de crédito): `Views/Ventas/DevolucionesVenta.razor`
  (`/initial/repayment` y `/sales/repayment`), proxy `IDevolucionesVenta`
  (`BuscarFacturaPorNumero`, `BuscarFacturaPorId`, `BuscarFacturasPorFiltro`,
  `Buscar`) + su `Guardar`. Ya sabe recibir una factura y armar la NC.
- Consignación: `Views/Consignacion/*` + `ConsignacionInvApiCliente`
  (`Prefacturas`, `Prefactura`, `AprobarPrefactura`, `EditarPrefactura`,
  `FacturarPrefactura`, `AnularPrefactura`).
- Preventas: `Facturacion.razor` factura preventas; hay `CargarPreventasActivas`
  en el API.
- Componentes: `AppPantalla`, `AppRejilla` (grid con paginación/orden),
  `AppFiltros`, `HxModal`, `HxOffcanvas`, patrón de pestañas por nav-tabs (ya
  usado en `EmisoresFiscal.razor` modal de alta y en `Inventario/Modulo.razor`).

---

## 2. Diseño propuesto (WEB)

### 2.1 Pantalla única con pestañas
- **Nueva pantalla** `Views/Documentos/Bandeja.razor`, en la ruta **existente**
  `@page "/initial/documents"` (se reutiliza junto con el código de menú
  `INICIO.DOCUMENTOS_EMITIDOS`). Título "Bandeja de documentos".
- `<AppPantalla Pantalla="Bandeja de documentos">` con `nav nav-tabs`:
  `Preventas | Facturas | Notas de Crédito | Consignaciones`. Estado `_pestana`
  (enum). Cada pestaña carga su lista *lazy* (al entrar por primera vez) y tiene
  su propio filtro (rango de fechas por defecto últimos 7 días + texto + estado).
- Cada pestaña usa un `AppRejilla<T>` con `Datos=` server-side (paginado) contra
  el endpoint correspondiente. Rango de fechas obligatorio.

### 2.2 Columnas por pestaña
**Comunes (todas):** Fecha del documento · Consecutivo · Cliente · Sucursal de
facturación · Subtotal · Impuesto · Total · (badge "Anulado" si aplica).

**Solo Facturas y Notas de Crédito (añaden):**
Número factura electrónica · Clave factura electrónica · Estado en Hacienda
(badge con color: verde aceptado / rojo rechazado o error / gris pendiente) ·
Mensaje de rechazo en Hacienda (texto, truncado con tooltip).

**Preventas y Consignaciones:** solo las comunes (sin las 4 fiscales).

### 2.3 Acciones por pestaña (columna final)
| Pestaña | Acciones |
|---|---|
| Preventas | Ver detalle · Facturar (→ `Facturacion.razor` con la preventa) · Anular |
| Facturas | Ver detalle · **Realizar devolución** · Ver XML firmado · Ver respuesta Hacienda · Reintentar (si `ErrorTecnico`) · Consultar estado en Hacienda |
| Notas de Crédito | Ver detalle · Ver XML firmado · Ver respuesta Hacienda · Reintentar (si `ErrorTecnico`) · Consultar estado en Hacienda. **Sin anular.** |
| Consignaciones | Ver detalle · Aprobar / Editar / Facturar / Anular **prefactura** (reusando los flujos de consignación). Solo prefacturas de consignación. |

- **Realizar devolución**: navegar a `/sales/repayment?factura={numero}` (o
  `?id={id}`) y que `DevolucionesVenta.razor` autoseleccione esa factura al
  cargar (hoy ya tiene `BuscarFacturaPorNumero` / `BuscarFacturaPorId`; falta
  leer el query param en `OnInitialized`). Alternativa: abrir la devolución en
  un `HxModal` dentro de la bandeja reutilizando el sub-form; más trabajo. Se
  recomienda **navegación con prefill**.
- **Ver XML firmado / respuesta Hacienda / Reintentar / Consultar estado**:
  llamar a los endpoints ya existentes
  `api/comprobantes-electronicos/v44/bandeja/{clave}/...` con `ClaveMh`
  (facturas) o `Clavedgt` (NC). Reusar el modal de detalle fiscal de
  `BandejaFiscal.razor`.

### 2.4 Detalle
- **Ver detalle** (común): `HxOffcanvas` o `HxModal` con cabecera (cliente,
  fecha, sucursal, tipo, cajero), totales y — en facturas/NC — bloque fiscal
  (nº FE, clave con copiar, estado, mensaje de rechazo, botones XML/respuesta).
  Para facturas y NC, listar las líneas (`Detalle`).

---

## 3. Contratos que consume el WEB (del plan API)

Proxy nuevo `IBandejaDocumentos` (patrón `ProxyBase` + cliente generado, o HTTP
directo como `IBandejaFiscal`):

```
Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoBandejaDTO>>>       Preventas(BandejaDocumentosFiltro f)
Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoFiscalBandejaDTO>>> Facturas(BandejaDocumentosFiltro f)
Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoFiscalBandejaDTO>>> NotasCredito(BandejaDocumentosFiltro f)
Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoBandejaDTO>>>       Consignaciones(BandejaDocumentosFiltro f)
Task<ResponseGeneric<FacturaBandejaDetalle>>     DetalleFactura(long id)
Task<ResponseGeneric<NotaCreditoBandejaDetalle>> DetalleNotaCredito(long id)
```

Acciones fiscales: reusar `IBandejaFiscal` tal cual (`XmlFirmado`,
`RespuestaHacienda`, `Reintentar`, y añadir `ConsultarEstado(clave)` →
`POST api/comprobantes-electronicos/v44/emisiones/{clave}/consultar-hacienda`).

DTOs espejo en `DTOs/Fiscal/` (o `DTOs/Bandeja/`), a mano (como el resto de
contratos que divergieron): `BandejaDocumentosFiltro`,
`BandejaDocumentosResultado<T>`, `DocumentoBandejaDTO`,
`DocumentoFiscalBandejaDTO`, detalles.

---

## 4. Trabajo por hacer (WEB) — checklist

- [ ] **§1** Decisiones confirmadas: **Preventas = `Venta.EsPreventa`**;
      **Consignaciones = solo prefacturas de consignación**; **Notas de Crédito sin
      anular**; **Facturas solo "realizar devolución"** (no "anular factura"); la
      pantalla va **bajo "Inicio"** y **reutiliza el código `INICIO.DOCUMENTOS_EMITIDOS`**
      (se retira `INICIO.BANDEJA_FISCAL_V4_4`). Pendiente: si el filtro de sucursal
      se fuerza a la de la sesión.
- [x] **§2** DTOs espejo (`DTOs/Bandeja/BandejaDocumentosDTOs.cs`) +
      `IBandejaDocumentos` + `ProxyClass/BandejaDocumentos` (estilo envelope, como
      `EmisoresFiscales`) + registro en `Program.cs`. `IBandejaFiscal` gana
      `ConsultarEstado(clave)`.
- [x] **§3** `Views/Documentos/Bandeja.razor` (+ `.razor.cs`) en `@page "/initial/documents"`
      con `nav-tabs`, filtro común (rango fechas + texto + estado Hacienda en
      Facturas/NC + "incluir anulados") y tabla + paginador Anterior/Siguiente
      (server-side), como `BandejaFiscal`.
- [x] **§4** Pestaña **Preventas**: columnas comunes; acción **Ver** (offcanvas con
      los datos de la fila). "Facturar / Anular preventa" → **follow-up** (flujo
      aparte, no incluido en v1).
- [x] **§5** Pestaña **Facturas**: columnas comunes + 4 fiscales + badge de estado
      con color (`ClaseEstado`); acciones **Ver** (detalle con líneas), **Devolución**
      (navega a `/sales/repayment?factura=<nº>`), **Fiscal** (modal: XML firmado,
      respuesta Hacienda, consultar estado, reintentar emisión).
- [x] **§6** Pestaña **Notas de Crédito**: columnas comunes + 4 fiscales; acciones
      **Ver** + **Fiscal** (sin devolución, sin anular).
- [x] **§7** Pestaña **Consignaciones**: columnas comunes + `EstadoDescripcion`;
      acción **Ver**. Aprobar/Editar/Facturar/Anular prefactura se siguen haciendo
      desde las pantallas de consignación → **follow-up**.
- [x] **§8** Offcanvas de **detalle** (común + bloque fiscal + líneas) para
      factura y NC; para preventa/consignación muestra los datos de la fila.
      Modal de acciones fiscales aparte.
- [x] **§9** `DevolucionesVenta.razor`: `OnInitialized` lee `?factura=<nº>`
      (`QueryHelpers`), prefija `_numeroFactura` y, tras desbloquear, autocarga
      la factura con `BuscarFacturaPorNumero`.
- [x] **§10** Menú: "Documentos Emitidos" → **"Bandeja de documentos"**
      (`INICIO.DOCUMENTOS_EMITIDOS` / `/initial/documents` intactos); quitada
      "Bandeja Fiscal V4.4". `FiltroMenuTests` 78 → 77. `PantallasMigradasTests`
      (E2E) título actualizado (nunca tuvo fila de `/invoices/fiscal-tray`).
- [x] **§11** Semilla: `INICIO.BANDEJA_FISCAL_V4_4` retirada de
      `SecuritySystem/Seed/seed-seguridad.json` y de la copia de test del sitio
      (idénticas); `INICIO.DOCUMENTOS_EMITIDOS` conservada con `nombre` nuevo.
      `SeedSeguridadTests` umbral 70 → 60.
- [x] **§12** Borradas `Views/Documentos/Emitidos.razor`,
      `Views/Facturacion/BandejaFiscal.razor`, `IDocumentosEmitidos` +
      `DocumentosEmitidos` (y su registro DI). `IBandejaFiscal` **conservada**.
- [x] **§13** `PantallasMigradasTests` (E2E) actualizado.
- [x] **§14** Build web limpio (0 warnings). Unit `dotnet test` 72/72
      (`FiltroMenuTests`, `MenuCodigosTests` incluidos). API smoke 418/418.

### Follow-ups (no en v1)
- Preventas: acción "Facturar" (→ `Facturacion.razor`) y "Anular preventa".
- Consignaciones: acciones de prefactura (aprobar/editar/facturar/anular) desde
  la propia bandeja en vez de las pantallas de consignación.
- Combo de estados de Hacienda con vocabulario normalizado (hoy es texto libre).
- Vista responsive en tarjetas para móvil (hoy solo tabla con scroll).

---

## 5. Notas / riesgos
- **No ampliar `FacturaDTO`** generado con campos fiscales: usar los DTOs nuevos
  de bandeja (mismo criterio que Consignación y Bonificación).
- El badge "Estado en Hacienda" necesita el vocabulario real de `EstadoMh` /
  `Estadodgt` (lo define el proyector fiscal en el API); pedir al API un set
  normalizado o mapearlo en el cliente.
- "Realizar devolución" por navegación con prefill es lo más barato y no duplica
  el formulario de devolución (que es grande: bonificaciones, lotes, validación
  de clave interna).
- Consignaciones: no reimplementar prefacturas; la pestaña es una **vista** que
  enlaza a los flujos existentes.
- Mantener la pantalla responsive (móvil = tarjetas, escritorio = rejilla), como
  hace hoy `Emitidos.razor`.
