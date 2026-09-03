# Series de Facturación — Análisis y plan de mejora (Sitio Web)

> La pantalla `/parameters/invoice-series` ("Series de Facturación") configura las
> series/consecutivos por **emisor · sucursal · terminal · tipo de documento** y su
> **emisión electrónica 4.4**. Es una parte compleja (facturas, tiquetes, notas de
> crédito, recibos, consignaciones, presupuestos) resuelta hoy con un formulario
> de campos numéricos sin contexto. Este documento propone rediseñarla para que
> sea entendible y a prueba de errores. Solo análisis y plan; no toca código.

---

## 1. Estado actual

### 1.1 Pantalla
`Views/Parametros/SeriesFacturacionFiscal.razor`, proxy `ISeriesFacturacionFiscales`
→ `SeriesFacturacion/Obtener|Crear|Actualizar`. DTO `SerieFacturacionFiscalDTO`
(espejo de `SerieFacturacionDTO` del API).

- **Grid**: Descripción · Emisor (nº) · Sucursal FE (texto) · Terminal (nº) ·
  Secuencia (nº) · Editar.
- **Modal "Nueva serie"** (una fila de inputs, sin secciones ni ayuda):
  - Descripción (texto)
  - Emisor → `<input type=number>` (¡hay que saber el Id!)
  - Sucursal → `<input type=number>` (¡el Id!)
  - Terminal → `<input type=number>`
  - Tipo de factura → `<input type=number>` (¡el `IdTipoFactura`!)
  - Secuencia → `<input type=number>`
  - ☐ "Habilitar emisión V4.4"

### 1.2 Qué está mal
1. **Todos los FK son campos numéricos**: el usuario tiene que memorizar IDs de
   emisor, sucursal y tipo de factura. Es la causa directa del "está muy sencillo
   pero es muy complejo".
2. **`EsCredito / EsRecibo / EsPago / EsConsignacion`** existen en el DTO y se
   round-tripean al editar, pero **no aparecen en el formulario**. Justo lo que el
   usuario dice que se configura ("Facturas, consignaciones, presupuestos,
   recibos…") está invisible.
3. **Sin contexto/ayuda**: nada explica qué es "Terminal", "Secuencia" ni qué
   hace "V4.4". No hay agrupación visual.
4. **Reglas ocultas** que solo se ven al Guardar (error del API):
   - La sucursal debe tener `NumeroSucursalFE` de 3 dígitos.
   - El tipo de factura debe tener `CodigoFE`.
   - "V4.4" solo se puede activar si `CodigoFE ∈ {01, 04, 05}`.
   - No se puede cambiar emisor/sucursal/terminal/tipo si la serie ya tiene
     documentos.
5. **Grid poco legible**: IDs crudos, sin nombre de emisor, sin descripción de
   tipo, sin badge de "uso" ni de V4.4.
6. **Relación con "Tipo de factura"** (lo que se usa al facturar): no se ve. El
   cajero en Facturación elige un Tipo; el API cruza ese tipo + emisor + sucursal
   para hallar la serie y su consecutivo. El usuario no tiene forma de entender
   ese vínculo desde esta pantalla.

---

## 2. Rediseño propuesto

### 2.1 Modal en secciones (o wizard de 2 pasos)
**Sección 1 — Identificación**
- **Emisor**: `<select>` con nombres (de `Catalogos.emisores`). Obligatorio.
- **Sucursal**: `<select>` con nombre comercial; al lado, chip con su
  `NumeroSucursalFE` y, si falta o es inválido, aviso en rojo con enlace a la
  pantalla de Sucursales. Obligatorio.
- **Descripción**: texto, con botón "Sugerir" que arma
  `"<Emisor> · <Sucursal> · <Tipo> · Caja <terminal>"`.

**Sección 2 — Documento**
- **Tipo de documento**: `<select>` mostrando `"<Descripción> — FE <CodigoFE>"`
  (de `Catalogos.tiposFactura`). Al elegirlo:
  - Se muestra para qué sirve (Factura / Tiquete / Nota de crédito / Recibo /
    Presupuesto…), derivado del `CodigoFE`.
  - Habilita/inhabilita el check de V4.4 según `compatibleV44`.
- **Terminal / Caja**: `<input number>` 0–99999 con ayuda ("número de caja; forma
  parte del consecutivo fiscal").
- **Uso** (los flags `Es*`, si el API los mantiene): checkboxes con texto llano
  — "Serie a crédito", "Recibo de dinero", "Nota de pago", "Consignación". Si el
  API los retira, esta parte desaparece y el uso sale del tipo.

**Sección 3 — Numeración**
- **Secuencia actual**: `<input number>`. Ayuda: "próximo número que se asignará
  es Secuencia + 1". En edición no puede bajar (el API lo valida; el input pone
  `min` = valor actual).
- **Previsualización** del consecutivo de 20 dígitos:
  `NumeroSucursalFE(3) · Terminal(5) · CodigoFE(2) · (Secuencia+1)(10)`, resaltando
  cada tramo. (`ProximoConsecutivoEjemplo` del API, o se arma en el cliente.)

**Sección 4 — Facturación electrónica (Hacienda 4.4)**
- ☐ **Emitir comprobante electrónico 4.4 automáticamente**. Deshabilitado con nota
  ("solo para Factura, Tiquete o Nota de Crédito — FE 01/04/05") cuando el tipo no
  es compatible. Muestra el `CodigoFE` del tipo elegido.

### 2.2 Grid
Columnas: **Descripción** · **Emisor** (nombre) · **Sucursal** (nombre + `FE nnn`)
· **Tipo** (descr. + `FE nn`) · **Uso** (badges) · **Terminal** · **Secuencia** ·
**4.4** (badge sí/no) · Editar. Filtro por emisor y por sucursal arriba.

### 2.3 A prueba de errores
- Campos estructurales (emisor, sucursal, terminal, tipo) **de solo lectura** si
  `TieneDocumentos` (con nota "esta serie ya emitió documentos").
- Validación en vivo en el cliente reflejando las reglas del API (sucursal sin FE,
  tipo sin CodigoFE, V4.4 no compatible) para no depender del error de Guardar.
- Mensajería del API (422 + `ValidationErrors`) mostrada como lista.

### 2.4 Textos
- "Emisión V4.4" → **"Emisión electrónica 4.4 (Hacienda)"**.
- "Emisor" con tooltip: "empresa/persona que factura".
- "Terminal" → **"Terminal / Caja"**.
- "Secuencia" con tooltip del formato del consecutivo.

---

## 3. Contratos que consume (del plan API)

- `GET /SeriesFacturacion/Catalogos` → `{ emisores[], sucursales[], tiposFactura[] }`
  (ver `SERIES_FACTURACION_API.md` §3.2).
- `SerieFacturacionDTO` enriquecido: `EmisorNombre`, `SucursalNombre`,
  `SucursalFEValida`, `TipoFacturaDescripcion`, `TipoFacturaCodigo`,
  `UsoDescripcion`, `CompatibleV44`, `TieneDocumentos`,
  `ProximoConsecutivoEjemplo` (§3.1).
- `SerieFacturacionFiscalDTO` (web) gana esos campos de solo lectura (partial a
  mano, como el resto de contratos divergidos).

---

## 4. Checklist (WEB)

- [x] **§1** Decisiones confirmadas (ver `SERIES_FACTURACION_API.md` §2):
      **flags `Es*` se mantienen con uso real** (clasifican la serie y marcan la
      venta) → van en el formulario como sección "Uso"; **serie por
      emisor+sucursal+terminal+tipo** (terminal = caja); **presupuestos se
      modelan aquí** con un tipo sin `CodigoFE` (sección 4 deshabilitada, sin
      exigir FE); `TipoFacturacion` legacy se elimina (no afecta al sitio).
- [ ] **§2** `SerieFacturacionFiscalDTO`: campos derivados nuevos + proxy
      `Catalogos()` en `ISeriesFacturacionFiscales`.
- [ ] **§3** `SeriesFacturacionFiscal.razor`: modal en 4 secciones con `<select>`
      de emisor / sucursal / tipo; ayudas y tooltips; previsualización del
      consecutivo.
- [ ] **§4** Habilitación condicional del check 4.4 según el tipo; nota explicativa.
- [ ] **§5** Campos estructurales de solo lectura cuando `TieneDocumentos`.
- [ ] **§6** Grid rediseñado (nombres + badges de uso/4.4) + filtro emisor/sucursal.
- [ ] **§7** Botón "Sugerir descripción".
- [ ] **§8** Validación en vivo espejo de las reglas del API; render de
      `ValidationErrors`.
- [ ] **§9** Build + `dotnet test` verdes.

### Follow-ups
- Un acceso directo desde Facturación ("¿por qué no encuentra serie para este
  tipo?") que lleve a esta pantalla con emisor/sucursal prefijados.
- Vista de "salud de series": qué combinaciones emisor×sucursal×tipo NO tienen
  serie configurada (para no descubrirlo al facturar).
