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

## 2. Rediseño propuesto

### 2.1 Modal "Editar tipo de documento" en secciones

**1 · Identificación**
- **Código interno** (número, único) · **Descripción**.

**2 · ¿Para qué se usa?**
- Radio "Uso principal": **Venta** / **Devolución (Nota de crédito)** / **Compra**
  / **Consignación**.
- Si *Venta* → sub-checkboxes **Contado** y/o **Crédito** (al menos uno).
- *Devolución* / *Compra* deshabilitan Contado/Crédito.
- Texto de ayuda por opción:
  - *Venta* → "aparece en Facturación".
  - *Devolución* → "aparece solo en Devoluciones de venta".
  - *Compra* → "aparece en Compras".

**3 · Documento electrónico (Hacienda 4.4)** — **switch**
- **OFF** → `CodigoFE = null`. Nota: "documento interno (presupuesto, proforma,
  devolución no electrónica…): no genera comprobante de Hacienda".
- **ON** → `<select>` con **01 Factura electrónica · 02 Nota de débito electrónica
  · 03 Nota de crédito electrónica · 04 Tiquete electrónico**, mostrando solo las
  **disponibles** (`CodigosFEDisponibles` del API: las no tomadas por otro tipo)
  + la actual. Si el uso es *Devolución*, el combo se limita a **03**.

**4 · Estado**
- Switch **Activo** (default ON). En OFF, aviso si el tipo tiene series/ventas
  (el API lo rechaza).

- **Avisos en vivo** (espejo de la validación del API): "Un tipo de devolución
  solo admite FE 03", "El código FE 01 ya lo usa «Factura contado»",
  "Un tipo de venta necesita Contado o Crédito".

### 2.2 Grid
Columnas: **Descripción** · **Uso** (badge: Venta / Devolución / Compra /
Consignación) · **Condición** (Contado / Crédito / —) · **Electrónico**
(badge `FE 01` … o "No") · **Activo**. Filtro por uso arriba.

### 2.3 Consumidores — pasar a filtro server-side
| Pantalla | Cambio |
|---|---|
| **Facturación** | `Api.Tipos()` → `Api.TiposPorContexto("facturacion")`. Se mantiene la regla contado/crédito por cliente en el cliente. |
| **Devoluciones de venta** | usar `TiposPorContexto("devolucion")` en el filtro de "buscar factura" (hoy muestra todos). |
| **Series de Facturación** | `Catalogos()` ya trae `tiposFactura`; agregar `uso`/`contado`/`credito` a cada fila y **agrupar** el `<select>` por uso (optgroup Facturación / Devoluciones / Compra / No fiscal). El formulario de serie muestra el uso del tipo elegido. |

### 2.4 Contratos que consume (del plan API)
- `TipoFacturaFiscalDTO` gana `EsDevolucion`, `Contado`, `Activo`.
- `ITiposFactura` gana:
  - `TiposPorContexto(string contexto)` → `GET TipoFactura/PorContexto?contexto=…`
  - `CodigosFEDisponibles()` → `GET TipoFactura/CodigosFEDisponibles`
- `SerieCatalogoTipoFacturaFiscalDTO` (series) gana `Uso`, `Contado`, `Credito`.
- En Facturación / Devoluciones: la DTO generada `TipoFactura` (NSwag) **no**
  tiene los campos nuevos → o se usa el proxy fiscal `ITiposFactura` /
  `TiposPorContexto` (recomendado), o se agrega un partial. Preferible cambiar
  esas pantallas al endpoint filtrado y no arrastrar flags al cliente.

---

## 3. Checklist (WEB) — pendiente

- [ ] **§1** Confirmar decisiones (ver `TIPOS_DOCUMENTO_API.md` §4).
- [ ] **§2** `TipoFacturaFiscalDTO` + `ITiposFactura` (`TiposPorContexto`,
      `CodigosFEDisponibles`) + proxy.
- [ ] **§3** `TiposFacturaFiscal.razor`: modal en 4 secciones (identificación ·
      uso · documento electrónico con switch + combo · estado); avisos en vivo;
      grid con badges de uso / condición / electrónico / activo + filtro por uso.
- [ ] **§4** Facturación → `TiposPorContexto("facturacion")`; Devoluciones →
      `TiposPorContexto("devolucion")`.
- [ ] **§5** Series de Facturación: `<select>` de tipo agrupado por uso; mostrar
      el uso del tipo elegido en el formulario de serie.
- [ ] **§6** Build + `dotnet test` verdes.

### Follow-ups
- Presupuestos / Proformas: quedan como tipo *Venta* + switch electrónico OFF
  (ya cubierto por el plan de Series).
- Un "asistente de códigos FE" que, dado el catálogo de Hacienda, sugiera el tipo
  a crear si falta (p. ej. "no tiene un tipo con FE 04 Tiquete").
