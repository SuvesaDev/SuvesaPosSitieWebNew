# Motor de plantillas de impresión — Estado y pendientes (Sitio Web)

> **Estado al 29 sep 2026, rama `develop`.** El API
> (`DevSuvesaPosWeb`, `SuvesaPos.Impresion`) resuelve la plantilla, arma los datos
> y renderiza el PDF. El sitio edita plantillas y abre ese PDF. Los pendientes de
> datos (empresa, sucursal, usuario creador) están en el documento del API; aquí
> quedan los pendientes de pantalla.
>
> Documento hermano:
> `../../DevSuvesaPosWeb/docs/MOTOR_PLANTILLAS_IMPRESION_API.md`.
> Hacer los pendientes del API **P1–P6** antes o junto con **W2**: si solo se
> agrega el botón, el PDF sigue saliendo con el membrete equivocado.

Decisiones del 3 sep 2026 que siguen vigentes: el render vive en el API (D1),
toma general y ajuste son dos tipos (D2), el correo adjunta el A4 (D3), y el
editor tiene formato A4 / térmico 80 mm, con rollo de 58 mm (D4).

---

## 1. Qué ya corre

- Pantalla `/parameters/print-templates`, código `PARAMETROS.PLANTILLAS_IMPRESION`.
  Lista por emisor y tipo, alta, edición por zonas (diseño, encabezado, receptor,
  datos del documento, columnas, totales, pie), formato A4 o térmico, serie cuando
  el tipo la usa, predeterminada, desactivar, previsualización PDF.
- El sitio no renderiza documentos de negocio. `IGeneradorPdf.Tabla` sigue solo
  para reportes tabulares y el estado de cuenta.
- El PDF se pide por el sitio en `/documentos/{tipo}/{id}/pdf`, que reenvía al
  API con el token. Atajos: `BotonImprimir` y `VisorPdf`.
- Toma física muestra **Consecutivo N.º {Id:D8}** y tiene botón de imprimir.
- El tiquete emitido desde facturación pide `formato=termico80`.

### Dónde hay botón hoy

| Documento | Dónde se abre el PDF |
|---|---|
| Factura / tiquete recién emitido | `Facturacion.razor` (el tiquete pide térmico) y `Cobrar.razor` (comprobante de la venta) |
| Factura y nota de crédito en consulta | `Bandeja.razor` — ver **W2** |
| Recibo de cobro | `CuentasPorCobrar.razor.cs` y `PanelRecibosFallidas.razor.cs` |
| Recibo de pago | `AbonoPagar.razor` y `RecibosPago.razor` |
| Boleta de trámite | `TramiteCobro.razor.cs` |
| Orden de compra | `OrdenCompra.razor` y `ConsultarPedidos.razor` |
| Toma física | `TomaFisica.razor` |
| Boleta de consignación | `Consignacion/Ajuste.razor` |
| Estado de consignación | `Consignacion/Estados.razor` |

---

## 2. Pendientes de mejora

Los de datos y membrete son **P1–P9** del documento del API. No se resuelven
agregando un botón. Los de esta lista son de sitio.

| Id | Prioridad | Pendiente | Hecho cuando |
|---|---|---|---|
| **W1** | Alta | Botón imprimir, con permiso `Imprimir`, en las pantallas que ya tienen proveedor en el API y no lo abren: toma general (`inventario-toma-general`), ajuste (`inventario-ajuste`), traslado de bodega (`traslado-bodega`), presupuesto (`presupuesto`), devolución interna (`devolucion-interna`) y la pantalla de devolución de venta (`nota-credito`, hoy solo desde la bandeja). | Cada una abre `/documentos/{slug}/{id}/pdf`. |
| **W2** | Alta | La bandeja de ventas pide siempre `factura-electronica`. Un tiquete reimpreso desde ahí usa la plantilla y el título de factura. Pedir `tiquete-electronico` cuando el documento es tiquete, y A4 en la reimpresión de consulta (el térmico queda para el puesto que acaba de cobrar). | El slug coincide con el tipo real del documento. |
| **W3** | Media | `Cobrar.razor` abre el PDF de la venta, no el recibo de cobro. El recibo sí se abre desde cuentas por cobrar. Decidir si Cobrar también ofrece el recibo cuando el cobro generó uno, sin quitar el comprobante de la venta. | El cajero puede imprimir el papel que acaba de producir. |
| **W4** | Media | Cuando exista el tipo del API **P5** (factura de compra del proveedor), botón en la pantalla de compra. No adelantarlo: el slug todavía no existe. | La compra impresa trae empresa y usuario de esa compra (P1–P3). |
| **W5** | Baja | El editor ya ofrece «Atendido por». Cuando el API cierre P2, revisar que el rótulo por defecto pase a «Elaborado por» y que empresa y sucursal aparezcan en la zona de encabezado, no solo como un campo que se puede apagar. | El editor no permite ocultar empresa, sucursal ni creador. |

Cerrado respecto al plan del 3 sep 2026, y no vuelve a abrirse:

- La previsualización es PDF embebido, no HTML.
- Hay varias plantillas por emisor, tipo, serie y formato, con una predeterminada.
- Quien edita plantillas necesita el permiso `PARAMETROS.PLANTILLAS_IMPRESION`.
- El tiquete del puesto de facturación sale por este motor, en térmico.

---

## 3. Pantalla de plantillas

Ruta `/parameters/print-templates`. Archivos
`Views/Parametros/PlantillasImpresion.razor(.cs)`.

Barra: emisor y tipo (los 15, agrupados por vertiente de facturación, operativo
u otro). El formato se elige dentro del editor, no en la barra.

Editor, acordeón: diseño (preset y colores), encabezado, receptor, datos del
documento, columnas del catálogo del tipo, totales, pie, leyendas. Previsualizar
pide el PDF de muestra al API. Guardar manda el `ConfiguracionJson` versión 2.

Proxy `IPlantillasImpresion`: listar, obtener, crear, actualizar, predeterminada,
desactivar, catálogo, previsualizar. Impresión de un documento ya emitido:
`IImpresionDocumentos.Pdf` o el endpoint local `/documentos/...`.

---

## 4. Checklist del plan original

Cerrado en `develop`:

- [x] Proxy `IPlantillasImpresion`, DTOs y registro.
- [x] Pantalla de plantillas con editor por zonas, formato y previsualización PDF.
- [x] Endpoint local `/documentos/{tipo}/{id}/pdf`.
- [x] Botones en facturación, bandeja, recibo de pago, recibo de cobro (desde
      cuentas por cobrar), toma física, orden de compra, boletas de consignación
      y de trámite, y estado de consignación. Los cuatro últimos no estaban en
      el plan de 11 pantallas.
- [x] Toma física muestra el consecutivo de 8 dígitos.
- [x] Menú `PARAMETROS.PLANTILLAS_IMPRESION`.

Sigue abierto — es la sección 2, y depende del API donde se indica:

- [ ] **W1** botones que faltan en pantallas que el API ya imprime.
- [ ] **W2** slug de tiquete en la bandeja.
- [ ] **W3** recibo desde Cobrar, si se confirma que hace falta ahí.
- [ ] **W4** compra de proveedor, después de **P5**.
- [ ] **W5** empresa, sucursal y «Elaborado por» no ocultables, después de **P2**.
