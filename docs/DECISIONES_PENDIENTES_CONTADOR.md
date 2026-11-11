# Decisiones del contador — módulo de contabilidad (resueltas)

Salen del UAT en vivo (2026-11-05/06) y del plan de mejoras (`PLAN_MEJORAS_UAT_ANALISIS.md`, Fase 4). El sistema ya funciona con la opción marcada como **«hoy»**; cada punto dice qué cambiaría si se elige otra. Ninguna requiere una migración. **El contador las resolvió el 2026-11-11: la resolución está en la sección siguiente y ninguna exige cambios de código.**

| # | Decisión | Qué hace hoy el sistema | Opciones | Qué cambia en el código |
|---|---|---|---|---|
| 1 | **Cuenta del sobrante de inventario** (toma física y aumento manual) | Ingreso `4.2.1.2 Sobrantes de inventario`, creada por el catálogo. Es un supuesto de quien programó | a) dejarla así; b) otra cuenta de ingreso; c) reversar contra la misma cuenta de merma `6.1.6.8` | b) y c): cambiar el código de la cuenta en el catálogo del libro y en la línea 8 de la plantilla `AjusteInventario`. No toca el código |
| 2 | **Compra sin comprobante electrónico: IVA** | El IVA va a `6.1.6.12 IVA soportado no acreditable` (gasto) | a) hoy; b) acreditarlo igual (no procede fiscalmente sin Mensaje Receptor); c) dejarlo pendiente hasta que llegue el comprobante | c) exige un estado «pendiente» en `ClasificacionIvaCompra` y un reproceso al llegar el XML |
| 3 | **Traslado entre bodegas de la misma sucursal** | No genera póliza: el inventario se mueve pero el libro no lo ve | a) documentar que es solo operativo; b) agregar una dimensión de bodega a las líneas de inventario para ver central contra consignado sin cambiar el saldo de la cuenta | b) es trabajo nuevo: dimensión por línea y reportes por bodega |
| 4 | **Tipo de artículo `002` y del catálogo Costa Pets** | El tipo 3 se contabiliza en `1.1.6.1.3 Producto terminado`; el faltante y el costo de las ventas de `002` cayeron ahí y no en mercadería `1.1.6.1.4` | Revisar el tipo de cada artículo antes del cierre: 2 materia prima, 3 producto terminado, el resto mercadería | Ninguno si el tipo está bien; si estaba mal, corregirlo en el artículo y reclasificar con la herramienta de reclasificación de inventario |
| 5 | **Fondo de caja al abrir** | Abrir no genera póliza; al cerrar solo se contabiliza la diferencia. Los fondos (₡40.000 de la caja #7, ₡10.000 de la #14) no están en `1.1.1.2` | a) asentar el fondo al abrir (caja contra el origen del fondo: banco, caja general o cuenta por pagar al dueño); b) dejarlo fuera y documentarlo | a) es un evento nuevo `AperturaCaja` con su plantilla y la cuenta de origen configurable |
| 6 | **Saldos iniciales del libro** | El libro empieza en cero el 2026-09-30 y las conciliaciones muestran diferencias estructurales: cuentas por cobrar −148.471,36 contra 27.630,73; por pagar 1.434.790,76 contra 0; inventario 451.592,66 contra −13.832,05 | Cargar los saldos al 2026-09-30 con el asiento de apertura o el importador de día cero | Ninguno: se hace con las herramientas existentes |
| 7 | **Auxiliar de cuentas por cobrar negativo** | El auxiliar suma −148.471,36 (hay clientes con saldo a favor o cobros sin factura) | Investigar por cliente antes de cargar saldos | Ninguno hasta saber la causa |
| 8 | **Recibo de abono duplicado (Compras #2)** | Existe en el entorno de pruebas; la idempotencia nueva evita que se repita | Anularlo (deshace un recibo real) o dejarlo documentado | Ninguno |

## Resolución del contador (2026-11-11)

| # | Decisión | Qué hacer | Quién |
|---|---|---|---|
| 1 | Cuenta del sobrante de inventario | **Opción A.** Se queda `4.2.1.2 Sobrantes de inventario` (puede renombrarse a «Ajustes por sobrantes de inventario»). No se netea contra la merma `6.1.6.8`, para no ocultar ineficiencias ni robos (NIC 2). Validar la cuenta en el catálogo | Contador |
| 2 | IVA de compra sin comprobante electrónico | **Opción A.** Va a `6.1.6.12` como gasto no acreditable; no se deja como activo pendiente. Si el XML llega meses después, asiento manual de reclasificación de `6.1.6.12` a `1.1.7.1` | Contador (procedimiento) |
| 3 | Traslado entre bodegas de la misma sucursal | **Opción A.** Es solo operativo: la bodega vive en el auxiliar de inventario y el mayor refleja el saldo total de `1.1.6.1.x` de la sucursal. Documentarlo en el manual de usuario | Documentación |
| 4 | Tipo del artículo `002` y del catálogo Costa Pets | Corregir el tipo en el maestro de artículos y luego ejecutar la herramienta de reclasificación de inventario (Caso 17 del UAT) para mover `1.1.6.1.3` a `1.1.6.1.4` | Equipo de datos, luego contador |
| 5 | Fondo de caja al abrir | **Opción A.** Asentar el fondo con un asiento manual de apertura o la carga de Día Cero: débito `1.1.1.2`, crédito a la cuenta de origen (`1.1.2.1` o aportes de socios). El evento automático `AperturaCaja` se hizo en la Fase 5 (entrega al abrir y devolución al cerrar, con la cuenta de origen configurable); si el contador prefiere el fondo fijo con asiento único, basta no activar esas dos plantillas | Contador |
| 6 | Saldos iniciales | Cargar el balance de comprobación al **2026-09-30** con la herramienta de importación de saldos iniciales, ya | Contador |
| 7 | Auxiliar de cuentas por cobrar negativo | Antes del Día Cero: sacar la antigüedad de saldos, identificar los clientes con saldo a favor o pagos sin aplicar y reclasificar esos montos a `2.1.3.4` (ingresos diferidos y anticipos de clientes) | Equipo de cartera |
| 8 | Recibo de abono duplicado (Compras #2) | Anularlo en el entorno de pruebas y dejar en el acta del UAT: «Caso de prueba de idempotencia exitoso. Registro duplicado de prueba anulado para limpiar el entorno» | QA |

### Orden sugerido
1. Datos: corregir el tipo de artículo (4) y cartera: reclasificar los saldos a favor (7).
2. Contador: cargar los saldos iniciales (6) y el asiento de apertura del fondo (5).
3. Correr el pre-cierre de septiembre-noviembre y revisar la lista de «documentos sin contabilizar».

Esta tabla es lo que el contador indicó; el sistema no ejecutó ninguna de estas acciones (en particular, no se anuló el abono duplicado ni se cargaron saldos).

## Qué ya quedó resuelto por defecto y conviene confirmar

- Sobrante en `4.2.1.2` (punto 1).
- Faltante de caja: hasta el tope de la empresa va a `6.1.6.9` (gasto no deducible); lo que pasa el tope se cobra al cajero (`1.1.5.1`) y luego se rebaja por planilla o se castiga.
- El depósito de un cliente se contabiliza como `CobroAplicado` cuando el banco lo confirma; `DepositoConfirmado` es solo el traslado de efectivo de la caja al banco.
- Un banco sin cuenta contable asignada deja el cobro o el pago en error de configuración (nunca cae en la cuenta de otro banco).
