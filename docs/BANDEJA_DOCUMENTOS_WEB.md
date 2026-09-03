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
- **Nueva pantalla** `Views/Documentos/Bandeja.razor` (nombre a confirmar), ruta
  nueva (p. ej. `/documents` o `/invoices/tray`).
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

- [ ] **§1** Decisiones confirmadas (doc API §2.1): **Preventas = `Venta.EsPreventa`**,
      **Consignaciones = solo prefacturas de consignación**, **Notas de Crédito sin
      anular**. Pendiente de confirmar: ubicación/código de menú de la pantalla
      nueva, si el filtro de sucursal es forzado a la de la sesión, y si se agrega
      "anular factura" además de "realizar devolución".
- [ ] **§2** DTOs espejo + `IBandejaDocumentos` + implementación de proxy +
      registro en DI (`Program.cs`/módulo de `ApiConexion`).
- [ ] **§3** `Views/Documentos/Bandeja.razor` con `nav-tabs` y estado de pestaña;
      un `AppRejilla<T>` por pestaña con carga server-side y filtro
      (rango fechas + texto + combo estado Hacienda en Facturas/NC).
- [ ] **§4** Pestaña **Preventas**: columnas comunes + acciones (Ver, Facturar,
      Anular). "Facturar" navega a `Facturacion.razor` con la preventa.
- [ ] **§5** Pestaña **Facturas**: columnas comunes + 4 fiscales + badge de estado
      con color; acciones (Ver, Realizar devolución, XML, Respuesta, Reintentar,
      Consultar estado). Gatear acciones de modificación con
      `Sesion.Puede(Titulo, AccionPantalla.Modificar)`.
- [ ] **§6** Pestaña **Notas de Crédito**: columnas comunes + 4 fiscales;
      acciones fiscales iguales a Facturas (sin "Realizar devolución", **sin anular**).
- [ ] **§7** Pestaña **Consignaciones**: columnas comunes; fuente = **prefacturas
      de consignación** (`ConsignacionInvApiCliente.Prefacturas`); reusar sus
      flujos (Aprobar/Editar/Facturar/Anular prefactura) para las acciones.
- [ ] **§8** Modal/offcanvas de **detalle** (común + bloque fiscal + líneas).
      Reusar el patrón de "copiar clave" de `Emitidos.razor` y el modal de
      detalle fiscal de `BandejaFiscal.razor`.
- [ ] **§9** `DevolucionesVenta.razor`: leer query param (`?factura=` / `?id=`)
      en `OnInitializedAsync` y autoseleccionar la factura (usa
      `BuscarFacturaPorNumero` / `BuscarFacturaPorId` ya existentes).
- [ ] **§10** Menú (`Class/MenuSeePos.cs`): quitar **Documentos Emitidos** y
      **Bandeja Fiscal V4.4**, agregar **Bandeja de documentos** (una sola
      entrada; ubicación a decidir — Inicio o Ventas). Ajustar
      `FiltroMenuTests` (conteo de nodos) y `PantallasMigradasTests` (rutas).
- [ ] **§11** Semilla de seguridad: `tests/.../Fixtures/seed-seguridad.json` debe
      quedar igual que la del API (nueva función, viejas fuera). `MenuCodigosTests`
      exige que todo código del menú exista en la semilla.
- [ ] **§12** Borrar `Views/Documentos/Emitidos.razor`,
      `Views/Facturacion/BandejaFiscal.razor` y sus proxies/DTOs si quedan sin
      uso (o dejarlos un ciclo y borrarlos después). `IBandejaFiscal` se
      **conserva** (lo reusa la bandeja nueva para XML/respuesta/reintento).
- [ ] **§13** `PantallasMigradasTests` (E2E): actualizar la ruta y el título.
- [ ] **§14** Build + `dotnet test` (unit) verdes; revisar `FiltroMenuTests`,
      `MenuCodigosTests`, `FiltroMenuTests.ElMenuRealSeCargoCompleto`.

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
