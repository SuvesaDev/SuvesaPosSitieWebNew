# Plan de mejoras tras el UAT de contabilidad — análisis y orden de trabajo

La misma copia vive en `DevSuvesaPosWeb/docs/` y `SuvesaPosSitieWebNew/docs/`.

Sale de leer `UAT_CONTABILIDAD_EN_VIVO.md` y `PLAN_MEJORAS_UAT_CONTABILIDAD.md` (idénticos en los dos repos) y de **comprobar cada hallazgo contra el código de `feature/modulo-contabilidad`** (2026-11-11). No modifica esos dos archivos: los edita otra persona y tienen cambios sin commit. Este documento los complementa: agrupa los hallazgos por causa, separa lo que ya está hecho de lo que falta y fija un orden.

**No se tocó código para preparar este plan.**

---

## 1. Qué se comprobó contra el código

| Id | Veredicto | Lo que dice el código |
|---|---|---|
| Las 4 correcciones «sin commit» | **Ya están commiteadas** | Commit `f797de9d` «Mejoras UAT». Incluye además 3 líneas en `ServicioCobroCredito` que los documentos no mencionan. Hay que actualizar la frase «todavía no hay commit» |
| 1 Sobrante de toma | Confirmado | `TomaFisicaManager` solo reparte `CostoDiferencia < 0`; la plantilla `AjusteInventario` solo debita pérdidas. El mismo hueco existe en la **rebaja/aumento manual de existencias** |
| 2 Consignación facturada | Confirmado | El `VentaEmitida` de consignación manda solo `Total`, `ImpVenta`, `SubTotal`, `Descuento`: faltan `Costo`, `SubTotalGravado`, `SubTotalExento` e `Inv*`, y la plantilla rechaza un campo ausente. En contado no se emite ningún evento |
| 3 Banco del cobro | Confirmado | La cuenta de banco es el uso fijo `BancoColones` (`1.1.2.1`); no hay mapeo banco → cuenta contable en la forma de pago |
| 4 Abono duplicado | Confirmado | La clave de idempotencia del evento es el **id del abono** (`PagoProveedorAplicado:{IdAbonocpagar}`): dos altas dan dos ids y dos eventos. Nada deduplica el alta |
| 13 Fecha de consignación | Confirmado | `Views/Consignacion/Estados.razor`, fila de la tabla: `@(a ?? b).ToString("dd/MM/yyyy")` cierra el paréntesis antes del `ToString` |
| 14 Porcentajes de compra | Confirmado, más extendido | `Views/Compras/Compra.razor`: `@linea.Descuento:N2%` e `@linea.Impuesto:N2%` en la rejilla y en dos avisos del XML |
| 17 Conciliación de caja | Confirmado | `ObtenerAperturaDeCajaSinCierre` filtra `Estado.Equals("M")` |
| 20 Apertura #0 | No verificado en código | Solo consta en los documentos |
| 23/24 Depósito de caja sin evento | **Causa encontrada** | `SucursalDelInstrumentoAsync` solo mira el cobro y la venta de la aplicación; un depósito de caja no tiene ninguno, devuelve `null`, el resolutor responde «sin libro» y el evento **no se crea, sin error ni aviso**. El dato para resolverlo existe: `PreDeposito.NumApertura` → `Aperturacaja.IdSucursal` |
| 7 Compras #14 | Cambió | Ya no es fecha futura (hoy es 2026-11-11). Queda por reintentar, siempre que la plantilla tenga versión vigente |
| 22/23 Plantillas con versión desde 2026-12-01 | Causa conocida | Es la primera versión atada al corte del inventario por tipo. En este libro se arregló a mano (NC v2, AjusteInventario v2). Las demás plantillas con corte pueden tener lo mismo: revisar |
| Compra sin XML, IVA a `6.1.6.12` (UAT-CR-0511) | Consecuencia de una regla nueva | Una compra sin comprobante electrónico lleva su IVA a gasto no acreditable. Los documentos lo muestran sin comentarlo: confirmar con el contador |

## 2. Errores en los propios documentos

1. **Numeración distinta.** En el UAT, el 22 es el ciclo de caja y el 23 la NC; en el plan, el 22 es la NC y el 23 el depósito. El hallazgo 25 (pre-cierre bloqueado) no está en el plan. Conviene una sola numeración.
2. «Todavía no hay commit» ya no es cierto (ver arriba).
3. «Los ₡40.000 de la #7 y los ₡10.000 de las demás cajas no están en `1.1.1.2`» describe una decisión pendiente (punto 21), no un error: dígalo así.
4. La póliza manual quedó sin comprobar a mano (la automatización no actualizó el estado Blazor). Sigue abierto.

## 3. Causas comunes (lo que de verdad hay que arreglar)

**A. Omisiones silenciosas.** Los hallazgos 24, 2, 1 y 5 tienen la misma forma: el sistema no falla, simplemente no deja rastro. El control «documentos sin contabilizar» cubre hoy ventas, compras y notas de crédito. No cubre depósitos de caja, cobros, prefacturas de consignación, tomas ni ajustes. Además, la toma #6 quedó **Contabilizada con una póliza de 0 líneas**: el motor no debería marcar así un asiento sin importes.

**B. Cuentas fijas donde hace falta el dato del pago.** El hallazgo 3 (banco). Aplica igual a los abonos a proveedor.

**C. Altas que no son idempotentes.** El hallazgo 4. Es un patrón: el sistema ya tiene `IServicioIdempotencia` para los comandos de facturación, pero el alta de abonos, cobros y depósitos no lo usa.

**D. Plantillas con vigencia mal puesta.** Los hallazgos 22/23. Hoy corregirlo exige capturar a mano todas las líneas de una «Nueva versión».

**E. Presentación Razor.** Los hallazgos 13, 14, 15 y 17 a 20. Los de formato se repiten con el mismo patrón y se pueden prevenir con una prueba.

**F. Pre-cierre bloqueado (hallazgo 25).** Es consecuencia de lo anterior más datos de arranque (ver fase 0).

## 4. Plan por fases

Esfuerzo: **S** (menos de un día), **M** (un día o dos), **L** (más).

### Fase 0 — Sin código, hoy

| Paso | Qué | Quién |
|---|---|---|
| 0.1 | Bandeja → Integridad → Plantillas: listar qué plantillas tienen vacío de vigencia (por la misma causa de NC y AjusteInventario). Corregir antes de seguir probando | Contador / TI |
| 0.2 | Reintentar Compras #14 | Contador |
| 0.3 | Anular el recibo duplicado de abono (Compras #2) y comprobar que CxP queda con ₡11.300 | Contador |
| 0.4 | Bandeja → Integridad → Documentos sin contabilizar: regularizar lo que aparezca | Contador |
| 0.5 | Diagnosticar las **15 diferencias de conciliación de CxC** una por una. Hipótesis a descartar primero: son **saldos de clientes anteriores a encender la contabilidad** y el libro no tiene saldos iniciales por tercero. Si es así, lo que corresponde es cargarlos (Puesta en marcha / Saldos iniciales), no tocar código | Contador |
| 0.6 | Actualizar los dos documentos de UAT: numeración, commit, decisión sobre el fondo de caja | Quien los edita |

### Fase 1 — Lo que impide cerrar un período (API)

| Orden | Hallazgo | Trabajo | Esfuerzo | Queda hecho cuando |
|---|---|---|---|---|
| 1.1 | 24 | Resolver la sucursal de un depósito de caja por `PreDeposito.NumApertura` → apertura. Si no se puede resolver, que quede **en error visible**, no en silencio | S | Confirmar un depósito de caja deja `DepositoConfirmado` Contabilizado, y uno sin sucursal resoluble aparece en la bandeja como error |
| 1.2 | 2 y 10 | Facturar una prefactura de consignación emite el mismo `VentaEmitida` que una venta normal (costo, base gravada y exenta, reparto por tipo de inventario) y, en contado, el `CobroAplicado`. Guardar código de artículo e id de lote reales en la prefactura para que `Facturado` sume | L | La prefactura #63 aparece en la bandeja con póliza y la liquidación muestra lo facturado |
| 1.3 | 1 | Sobrante: nueva cuenta de ingreso por sobrante de inventario y plantilla (par inventario / sobrante). Una toma mixta parte faltante y sobrante en una póliza. Incluye el **aumento manual de existencias** | M | Una toma con unidades de más deja débito a inventario y crédito al sobrante por su costo. **Depende de la decisión 5.1** |
| 1.4 | Póliza vacía | Un asiento sin importes no se marca Contabilizado: queda `NoAplicable` con motivo, o error | S | La toma #6 deja de aparecer como Contabilizada con 0 líneas |
| 1.5 | 4 | Idempotencia del alta de abono (referencia + factura) y botón deshabilitado mientras guarda | S | Un segundo guardado devuelve el recibo existente |
| 1.6 | 3 | Banco por pago: mapeo cuenta bancaria → cuenta contable; el evento lleva la cuenta elegida; sin mapeo queda en error de configuración | M | El cobro con Banco Nacional debita la cuenta de ese banco, y uno sin mapeo no cae en la de otro |

Sugerencia de orden: 1.1, 1.4 y 1.5 primero (pequeños, quitan silencios y duplicados); 1.2 en paralelo por ser el más grande; 1.3 y 1.6 cuando el contador decida.

### Fase 2 — Que no vuelva a pasar en silencio (API)

| Trabajo | Esfuerzo | Queda hecho cuando |
|---|---|---|
| Ampliar «documentos sin contabilizar» a cobros, depósitos de caja confirmados, prefacturas de consignación, tomas aprobadas y abonos | M | Cada uno de esos documentos sin evento aparece en Integridad y bloquea el cierre |
| Prueba de contrato: por cada flujo de dinero o inventario, un documento de ejemplo debe producir evento | M | Una prueba falla si un flujo nuevo no emite |
| «Duplicar versión de plantilla con otra vigencia» en la pantalla de Plantillas | M | Corregir una vigencia no obliga a capturar las líneas |

### Fase 3 — Presentación (Web)

| Hallazgos | Trabajo | Esfuerzo |
|---|---|---|
| 13, 14 | Corregir las expresiones y agregar **una prueba que busque en los `.razor` el patrón `@x:N2` y `@(...).ToString(` mal cerrado** | S |
| 15 | La grilla de comisiones y la de prefactura muestran código y lote, no el id interno | S |
| 17, 18, 19, 20 | Conciliación lista cajas `A` y `M`; arqueo muestra fondo + efectivo esperado; el cierre dice contado, esperado, diferencia y destino; la apertura devuelve su número real | M |
| 8, 9 | `EsDeducible` visible y editable; `1.1.6.1` no admite movimiento | S |
| 16 | La pantalla de depósitos nombra el evento que debe buscarse | S |

### Fase 4 — Decisiones del contador y pulido

| Decisión | Qué hay que decidir |
|---|---|
| 5.1 | Cuenta del sobrante de inventario |
| 5 / 6 / 21 | Traslado entre bodegas de la misma sucursal (solo operativo o con dimensión de bodega); tipo del artículo 002 y del catálogo Costa Pets; fondo de caja al abrir |
| 5.2 | Compra sin comprobante electrónico: ¿su IVA va a gasto no acreditable? |
| 11, 12 | Corte de comisiones con el detalle antes de confirmar; ruido de clave duplicada en el log |

## 5. Orden de ejecución

1. **Fase 0** hoy: desbloquea el pre-cierre sin escribir código.
2. **Fase 1**: 1.1, 1.4, 1.5 (rápidos), 1.2 (largo) en paralelo.
3. **Fase 3** en cuanto haya manos libres: no depende de nada.
4. **Fase 1**: 1.3 y 1.6 cuando llegue la decisión.
5. **Fase 2** después, para blindar.
6. **Fase 4** al final.

## 6. Cómo re-probar cada fase

No repetir lo ya cerrado en el UAT. Re-probar solo el caso que se arregló:

- 1.1: confirmar un pre-depósito de caja y verificar `DepositoConfirmado`.
- 1.2: facturar una prefactura de consignación de contado y otra de crédito; revisar `VentaEmitida` (con costo) y `CobroAplicado`.
- 1.3: toma con sobrante y toma mixta.
- 1.4: la póliza sin importes no queda Contabilizada.
- 1.5: guardar el mismo abono dos veces (doble clic y dos llamadas).
- 1.6: cobro con un banco distinto al de la cuenta por defecto, y otro sin mapeo.
- Cierre: pre-cierre del mes sin bloqueos.

## 7. Datos que el UAT dejó en el entorno

Siguen vigentes los de la sección «Datos dejados en el entorno» del UAT: no borrarlos ni volver a registrarlos.
