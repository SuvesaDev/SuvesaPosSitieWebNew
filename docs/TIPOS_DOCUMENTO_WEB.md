# Tipos de documento — Análisis y plan de mejora (Sitio Web)

> Pantalla `/parameters/invoice-types` ("Tipos de Factura"). Un tipo de documento
> se asocia a una **serie de facturación** y esta a **facturación / devoluciones**.
> Hoy el formulario tiene checkboxes sueltos (Crédito · Compra · Consignación) y un
> `Código FE` de texto libre; ninguna pantalla sabe distinguir "tipo para notas de
> crédito". Este documento propone rediseñarlo para clasificar el **uso** y elegir
> el **código FE electrónico** con un switch. Solo análisis y plan; no toca código.

---

## 1. Estado actual

### 1.1 Pantalla `TiposFacturaFiscal.razor`
- Proxy `ITiposFactura` → `TipoFactura/ObtenerTipoFacturas|CrearTipoFacturas|ActualizarTipoFacturas`.
- DTO `TipoFacturaFiscalDTO`: `Id, Descripcion, Codigo, Credito, Compra, Consignacion, CodigoFE`.
- **Grid**: Código · Descripción · Código FE · Condición (`Compra` → "Compra";
  `Consignacion` → "Consignación"; `Credito` → "Crédito"; si no, "Contado").
- **Modal**: Código interno · Descripción · **Código FE** (texto libre, "2 dígitos")
  · checkboxes **Crédito** · **Compra** · **Consignación**.

### 1.2 Cómo lo consumen otras pantallas
| Pantalla | Filtro hoy |
|---|---|
| Facturación | `Tipos().Where(!Compra)` + `!Credito \|\| clienteAbierto/Sinrestriccion` |
| Devoluciones de venta | **todos** (filtro "buscar factura") |
| Series de Facturación | `Catalogos()` → `.Where(!Compra)` |

### 1.3 Problemas
1. **No hay marca de "devolución / nota de crédito"** → la pantalla de
   devoluciones muestra tipos que no son de NC, y facturación podría mostrar un
   tipo de NC.
2. **No hay "contado" explícito** (es implícito `!Credito`) ni **"activo"** (no se
   pueden retirar tipos obsoletos sin borrar).
3. **`Código FE` texto libre**: el usuario tiene que saber el número; puede poner
   cualquier cosa; no ve qué códigos ya están tomados (hay índice único: un solo
   tipo por código FE).
4. El grid no comunica el **uso** ni si es electrónico.

---

## 2. Rediseño propuesto — decisiones confirmadas

**Modelo:** columna **`Uso` (enum)** en el API `{1 Facturacion, 2 Devolucion,
3 Compra, 4 Consignacion}` + `Contado` + `Activo`; se dropean los bools `Compra`
y `Consignacion`. `CodigoFE` queda **restringido** al catálogo `{01,02,03,04}`
(sin texto libre). Devolución **no electrónica** permitida (`Uso=Devolucion` +
`CodigoFE` vacío). En Series el `<select>` de tipo se **agrupa** por uso.

### 2.1 Modal "Editar tipo de documento" en secciones

**1 · Identificación**
- **Código interno** (número, único) · **Descripción**.

**2 · Uso** — `<select>` (mapea al enum `Uso` del API):
  **Facturación (venta)** / **Devolución (Nota de crédito)** / **Compra** /
  **Consignación**.
- Si *Facturación* → sub-checkboxes **Contado** y/o **Crédito** (al menos uno);
  ayuda "aparece en Facturación".
- *Devolución* → ayuda "aparece solo en Devoluciones de venta"; oculta
  Contado/Crédito.
- *Compra* → "aparece en Compras"; oculta Contado/Crédito y el switch electrónico
  (FE de compra fuera de alcance).
- *Consignación* → "aparece en las series de consignación".

**3 · Documento electrónico (Hacienda 4.4)** — **switch**
- **OFF** → `CodigoFE = null`. Nota: "documento interno (presupuesto, proforma,
  devolución no electrónica…): no genera comprobante de Hacienda".
- **ON** → `<select>` con las opciones **según el uso**:
  - *Facturación* → `01 Factura electrónica` · `04 Tiquete electrónico`.
  - *Devolución* → `03 Nota de crédito electrónica` (fijo).
  - *Consignación* → `01 Factura electrónica`.
  - Solo se ofrecen las **disponibles** (`CodigosFEDisponibles` del API: las no
    tomadas por otro tipo) + la actual del tipo en edición.

**4 · Estado**
- Switch **Activo** (default ON). En OFF, aviso si el tipo tiene series/ventas
  (el API lo rechaza).

- **Avisos en vivo** (espejo de la validación del API): "Un tipo de devolución
  solo admite FE 03", "El código FE 01 ya lo usa «Factura contado»",
  "Un tipo de venta necesita Contado o Crédito".

### 2.2 Grid
Columnas: **Descripción** · **Uso** (badge: Facturación / Devolución / Compra /
Consignación) · **Condición** (Contado / Crédito / —) · **Electrónico**
(badge `FE 01` … o "No") · **Activo**. Filtro por uso arriba.

### 2.3 Consumidores — pasar a filtro server-side
| Pantalla | Cambio |
|---|---|
| **Facturación** | `Api.Tipos()` → `Api.TiposPorContexto("facturacion")`. Se mantiene la regla contado/crédito por cliente en el cliente. |
| **Devoluciones de venta** | usar `TiposPorContexto("devolucion")` en el filtro de "buscar factura" (hoy muestra todos). |
| **Series de Facturación** | `Catalogos()` ya trae `tiposFactura`; agregar `uso`/`contado`/`credito` a cada fila y **agrupar** el `<select>` por uso (optgroup Facturación / Devoluciones / Compra / No fiscal). El formulario de serie muestra el uso del tipo elegido. |

### 2.4 Contratos que consume (del plan API)
- `TipoFacturaFiscalDTO`: `Compra`/`Consignacion` (bools) → **`Uso` (int/enum)**;
  gana `Contado`, `Activo`.
- `ITiposFactura` gana:
  - `TiposPorContexto(string contexto)` → `GET TipoFactura/PorContexto?contexto=…`
  - `CodigosFEDisponibles()` → `GET TipoFactura/CodigosFEDisponibles`
- `SerieCatalogoTipoFacturaFiscalDTO` (series) gana `Uso`, `Contado`, `Credito`.
- En Facturación / Devoluciones: la DTO generada `TipoFactura` (NSwag) tiene
  `Compra`/`Consignacion` bool → esas pantallas pasan a llamar `TiposPorContexto`
  (`ITiposFactura`) y dejan de leer flags. `Facturacion.razor` línea 1080
  (`_tipos.First(t => t.Codigo == _tipoFactura)`) y `_tipos.Where(!t.Compra)` se
  ajustan.

---

## 3. Checklist (WEB) — pendiente

- [x] **§1** Decisiones confirmadas (ver `TIPOS_DOCUMENTO_API.md` §4): enum `Uso`;
      `CodigoFE` cerrado `{01,02,03,04}`; devolución no electrónica permitida;
      `<select>` de tipo en series **agrupado** por uso.
- [ ] **§2** `TipoFacturaFiscalDTO` (`Uso`, `Contado`, `Activo`; fuera bools
      `Compra`/`Consignacion`) + `ITiposFactura` (`TiposPorContexto`,
      `CodigosFEDisponibles`) + proxy.
- [ ] **§3** `TiposFacturaFiscal.razor`: modal en 4 secciones (identificación ·
      `<select>` de uso con sub-checkboxes contado/crédito · switch electrónico +
      combo FE según uso · switch activo); avisos en vivo; grid con badges de
      uso / condición / electrónico / activo + filtro por uso.
- [ ] **§4** Facturación → `TiposPorContexto("facturacion")` (y ajustar los
      `_tipos.Where(!Compra)` / `_tipos.First(...)`); Devoluciones →
      `TiposPorContexto("devolucion")`.
- [ ] **§5** Series de Facturación: `<select>` de tipo con `<optgroup>` por uso;
      mostrar el uso del tipo elegido en el formulario de serie.
- [ ] **§6** Build + `dotnet test` verdes.

### Follow-ups
- Presupuestos / Proformas: quedan como tipo *Venta* + switch electrónico OFF
  (ya cubierto por el plan de Series).
- Un "asistente de códigos FE" que, dado el catálogo de Hacienda, sugiera el tipo
  a crear si falta (p. ej. "no tiene un tipo con FE 04 Tiquete").
