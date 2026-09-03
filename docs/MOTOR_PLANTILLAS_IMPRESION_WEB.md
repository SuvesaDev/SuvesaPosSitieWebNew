# Motor de plantillas de impresión — Análisis y plan de trabajo (Sitio Web)

> Parte de sitio del motor de plantillas. El API (repo `DevSuvesaPosWeb`, rama
> `feature/bonificaciones`) resuelve la plantilla, arma los datos y **renderiza el
> PDF**. La web aporta: (1) una pantalla de **diseño de plantillas** por
> emisor/tipo/serie con previsualización, y (2) los botones **Imprimir /
> Descargar PDF** en las pantallas de los 10 documentos.
>
> Repo web, rama `feature/ola-0-cimientos`. **Solo análisis y plan; no toca código.**
> Documento hermano (API):
> `../DevSuvesaPosWeb/docs/MOTOR_PLANTILLAS_IMPRESION_API.md`.

---

## 1. Estado actual (web)

- **PDF:** `Services/IGeneradorPdf` + `GeneradorPdfQuestPdf` (QuestPDF 2026.8.0)
  con un único método `Tabla(ReporteTabular)` — reportes tabulares A4. El
  comentario de `IGeneradorPdf` dice *“por la decisión 05 no se imprime nada …
  no hay formatos térmicos”*: **ese supuesto queda revertido**. Endpoints
  minimal-API en `Program.cs`: `/reportes/compras/pdf` y cuentas por pagar; se
  descargan con `<a href=…>`.
- **No hay** pantalla de plantillas ni de “configuración de impresión”.
- Pantallas que necesitarán botón imprimir y ya existen:
  | Documento | Pantalla web |
  |---|---|
  | Factura / Tiquete | `Views/Ventas/Facturacion.razor` (post-venta) + `Views/Documentos/Bandeja.razor` |
  | Nota de crédito | `Views/Ventas/DevolucionesVenta.razor` + Bandeja (pestaña NC) |
  | Recibo de cobro | `Views/Ventas/Cobrar.razor` |
  | Recibo de pago | `Views/Compras/CuentasPorPagar.razor` |
  | Presupuesto | pantalla de proformas/cotización (`/sales/budgets/...`) |
  | Consignación boletas | `Views/Consignacion/*` |
  | Inventario / Traslados / Toma física | `Views/Compras/TomaFisica.razor`, traslados de bodega, ajustes |
- Series: `Views/Parametros/SeriesFacturacionFiscal.razor` (catálogo de series por
  emisor/sucursal/terminal/tipo) — su proxy sirve para el `<select>` de serie en
  el editor de plantillas.
- Patrón proxy: `: ProxyBase`, `Ejecutar(async () => await Leer<T>(...))`,
  `CreateClient("SeePosApi")`, `Envelope<T>`, `JsonSerializerOptions(Web)`.
- DTOs generados en `DTOs/Generated/SeePosDtos.cs` son `partial` — **no** se
  regenera NSwag completo; se agregan partials con `[JsonPropertyName]`.
- Menú `Class/MenuSeePos.cs`; `tests/…/Fixtures/seed-seguridad.json` byte-idéntico
  al del API. Tests de menú/semilla (`FiltroMenuTests` conteo **hoy 78**).

---

## 2. Requerimiento (parte web)

3. Plantillas **configurables por el usuario** (encabezado, pie, “y así en lo que
   se necesite”).
4. Plantillas **ligadas a la Serie de facturación** para los documentos de serie.
5. Toma física: mostrar un **consecutivo = `Id` de la toma**.

### 2.bis Decisiones confirmadas (2026-09-03)
- **D1** — el render vive en el API; la web solo edita plantillas y muestra
  botones que abren el PDF del API.
- **D2** — “inventarios” son **dos** tipos: **Inventario · toma general** e
  **Inventario · ajuste**. El `<select>` de tipo tiene **11 opciones**.
- **D3** — el PDF se adjunta al correo desde la fase 1 (no afecta a la web salvo
  que la columna “Correo” de la bandeja ya asume adjunto PDF presente).
- **D4** — **térmico 80 mm dentro del alcance**: el editor tiene selector de
  **Formato (A4 / Térmico 80 mm)** por plantilla y la previsualización renderiza
  ambos.

---

## 3. Pantalla — Diseño de plantillas de impresión

**Ruta** `/parameters/print-templates` · **Código menú**
`PARAMETROS.PLANTILLAS_IMPRESION` · bajo **Parámetros**.
Componentes `Views/Parametros/PlantillasImpresion.razor(.cs)` (+ `.razor.css`).

### 3.1 Estructura
- **Barra superior**: `<select>` Emisor · `<select>` Tipo de documento
  (**11 opciones**, incluye *Inventario · toma general* e *Inventario · ajuste*
  — D2) · `<select>` **Formato** (*A4* / *Térmico 80 mm* — D4) · si el tipo
  **usa serie** (`factura`, `tiquete`, `nota-credito`): `<select>` Serie (del
  catálogo de series del emisor). Botón **“Nueva plantilla”**, lista de
  plantillas existentes (`AppRejilla`: Nombre · Serie · Formato · Predeterminada
  · Activa) con acciones Editar / Marcar predeterminada / Desactivar.
- **Editor** (2 columnas):
  - **Izquierda — zonas** (acordeón): Encabezado · Receptor · Meta del documento ·
    Detalle (columnas) · Totales · Pie de página · Leyendas · Márgenes y fuente.
  - **Derecha — previsualización**: `<iframe>` / `<embed>` con el PDF que
    devuelve `POST /api/plantillas-impresion/{id}/previsualizar` (datos de
    muestra). Botón **“Actualizar vista”** (o debounce al cambiar).
- **Guardar** → `PUT /api/plantillas-impresion/{id}` (envía el `ConfiguracionJson`
  armado por el editor). `IManejadorRespuestas.CorrectaAsync` para
  `validationErrors` (el API valida contra el catálogo del tipo).

### 3.2 Editor por zona (mapa al esquema JSON del API)
| Zona | Controles |
|---|---|
| **Encabezado** | switch “Mostrar logo”, alineación (izq/centro/der), alto del logo (mm), subir logo (override; si no, usa el del emisor), switch “Mostrar datos del emisor”, N líneas de texto libre. |
| **Receptor** | switch “Mostrar bloque”; por campo (`nombre`, `identificacion`, `direccion`, `telefono`, `correo`): visible + etiqueta. |
| **Meta** | lista ordenable (drag) de campos (`consecutivo`, `fechaEmision`, `condicionVenta`, `fechaVencimiento`, `medioPago`, `diasCredito`, `claveNumerica`): visible + etiqueta + orden. `claveNumerica` solo aparece en electrónicos. |
| **Detalle** | tabla de columnas **del catálogo del tipo** (`GET /api/plantillas-impresion/catalogo/{tipo}`): visible + etiqueta + ancho relativo + alineación + reordenar. No se agregan columnas fuera del catálogo. |
| **Totales** | filas activables (`subtotal`, `descuentos`, `impuesto`, `total`, extras del tipo): visible + etiqueta. |
| **Pie** | N líneas de texto libre; switch “Mostrar texto de resolución” + textarea; switch “Datos bancarios”; switch “Numerar páginas” (deshabilitado si Formato = Térmico). |
| **Leyendas** | textos “ORIGINAL” / “COPIA”. |
| **Formato y página** | **Formato (A4 / Térmico 80 mm)**; si Térmico: ancho de rollo (58 / 80 mm) y aviso de que se aplica un perfil de columnas reducido; si A4: 4 márgenes (mm). Familia y tamaño de fuente base (en Térmico el API fuerza 7–8 pt). |

El editor **construye/lee el JSON**; no hay campos libres fuera del esquema (V3
del doc API). Al cambiar **Formato** el editor oculta/deshabilita las opciones
que no apliquen y vuelve a pedir previsualización.

### 3.3 Proxy y DTOs
- `IPlantillasImpresion` / `PlantillasImpresion : ProxyBase`:
  `Listar(idEmisor, tipo)`, `Obtener(id)`, `Crear(dto)`, `Actualizar(dto)`,
  `MarcarPredeterminada(id)`, `Desactivar(id)`, `Catalogo(tipo)`,
  `Previsualizar(id, jsonOverride?)` (devuelve `byte[]` → `data:` URL / blob).
- DTOs en `DTOs/Impresion/`: `PlantillaImpresionDTO`,
  `CatalogoCamposImpresionDTO`, `ConfiguracionPlantillaDTO` (espejo del esquema).
- Registrar en `Program.cs` (`AddScoped`).

---

## 4. Botones Imprimir / Descargar PDF en las pantallas de documento

Patrón único: un botón que abre
`GET /api/impresion/{tipo}/{id}/pdf` en pestaña nueva (o descarga). Como el
endpoint exige JWT, se resuelve con un pequeño proxy que trae el `byte[]` y hace
`blob:` + `window.open`, o con un endpoint minimal-API local
`/documentos/{tipo}/{id}/pdf` que reenvía con el token (igual que
`/reportes/compras/pdf`). Recomendado: **minimal-API local** que ya inyecta el
`HttpClient("SeePosApi")` autenticado y hace `stream` del PDF.

| Pantalla | Acción a agregar |
|---|---|
| `Facturacion.razor` (post-venta OK) | “Imprimir factura/tiquete” → `factura-electronica` o `tiquete-electronico` según la serie usada. |
| `Bandeja.razor` (Facturas / NC) | botón “PDF” por fila. |
| `DevolucionesVenta.razor` | “Imprimir nota de crédito”. |
| `Cobrar.razor` | “Imprimir recibo de cobro” (`recibo-cobro`). |
| `CuentasPorPagar.razor` | “Imprimir recibo de pago” (`recibo-pago`). |
| Proformas/Cotización | “Imprimir presupuesto”. |
| Consignación boletas | “Imprimir boleta”. |
| Inventario · toma general | “Imprimir inventario” → `inventario-toma-general`. |
| Inventario · ajuste | “Imprimir ajuste” → `inventario-ajuste`. |
| Traslados de bodega | “Imprimir traslado”. |
| `TomaFisica.razor` | “Imprimir toma física” + mostrar **“Consecutivo N.º {Id:D8}”** en el encabezado de la pantalla (además del PDF). |

- Para **Factura/Tiquete desde el POS** el botón pide `?formato=termico80`
  (usa la plantilla térmica); la Bandeja y la reimpresión piden `A4`. El correo
  adjunta siempre `A4` (D3/D4).
- Gate por permiso de cada pantalla (`Sesion.Puede(Titulo, Imprimir/Consultar)`).

---

## 5. Ajuste al `IGeneradorPdf` (solo nota, no código aquí)
- Actualizar el comentario XML de `Services/IGeneradorPdf.cs` para reflejar que
  **sí** se imprimen documentos y que los **documentos** ahora se renderizan en
  el API; `IGeneradorPdf.Tabla` queda **solo** para los reportes tabulares
  (`/reportes/compras/pdf`, cuentas por pagar).

---

## 6. Menú y semilla
`Class/MenuSeePos.cs`: **Parámetros → Plantillas de impresión**
(`PARAMETROS.PLANTILLAS_IMPRESION`).
`tests/SuvesaPosSitioAplicacion.Tests/Fixtures/seed-seguridad.json`: añadir la
función (byte-idéntico al seed del API, edición quirúrgica).
Subir el conteo en `FiltroMenuTests`; revisar `MenuCodigosTests`,
`SeedSeguridadTests`.

---

## 7. Checklist Web

- [ ] **1. Proxy** `IPlantillasImpresion` + DTOs `DTOs/Impresion/*` (partials,
      sin regen NSwag) + registro en `Program.cs`.
- [ ] **2. Pantalla** `PlantillasImpresion.razor(.cs/.css)`: barra
      emisor/tipo/**formato**/serie, lista, editor por zonas, previsualización en
      `<iframe>` (renderiza A4 y Térmico — D4).
- [ ] **3. Editor de zonas** que construye/lee el `ConfiguracionJson` según el
      catálogo (`GET .../catalogo/{tipo}`); reordenamiento de campos/columnas;
      opciones condicionadas al **Formato**.
- [ ] **4. Endpoint local** `/documentos/{tipo}/{id}/pdf?formato=` (minimal-API,
      reenvía con token) o proxy `blob:`.
- [ ] **5. Botones Imprimir/PDF** en las **11 pantallas** de la tabla §4
      (gateados); POS pide `termico80`, Bandeja/reimpresión piden `a4`.
- [ ] **6. Toma física**: mostrar “Consecutivo N.º {Id:D8}” en
      `TomaFisica.razor`.
- [ ] **7. Nota** en el comentario de `IGeneradorPdf` (documentos → API).
- [ ] **8. Menú + semilla** (§6) + tests de menú/semilla.
- [ ] **9. Pruebas** `dotnet test tests/SuvesaPosSitioAplicacion.Tests/...`
      (hoy 72) — proxy (deserialización, error), conteo de menú.
- [ ] **10. Build** `dotnet build src/SuvesaPosSitioAplicacion/... -v q`.
- [ ] **11. Docs** — cerrar decisiones aquí; referencia cruzada desde
      `docs/REVISION_PARIDAD_REACT_BLAZOR.md` si aplica.

---

## 8. Preguntas abiertas

**Resueltas (2026-09-03):** render en API (D1) · “inventarios” = toma general +
ajuste, 11 tipos (D2) · PDF al correo desde fase 1 (D3) · térmico 80 mm en
alcance con selector de formato (D4).

Pendientes:
1. ¿Previsualización como **PDF embebido** (más fiel) o **HTML** (más ágil)? El
   plan asume PDF embebido vía `previsualizar`.
2. ¿La descarga/impresión abre **pestaña nueva** con el PDF o fuerza descarga?
   (recomendado: pestaña nueva, `Content-Disposition: inline`).
3. ¿El editor permite **varias plantillas** por (emisor, tipo, serie, formato)
   con una predeterminada, o **una sola**? El plan asume varias + predeterminada.
4. ¿Quién puede editar plantillas: solo administrador o un permiso propio
   (`PARAMETROS.PLANTILLAS_IMPRESION`)? El plan asume permiso propio.
5. Para el POS: ¿el tiquete se imprime por este endpoint (`termico80`) o el POS
   conserva su impresión propia y este motor cubre reimpresión + correo?
