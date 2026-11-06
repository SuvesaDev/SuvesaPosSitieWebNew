# Plan de mejoras — UAT de contabilidad en vivo

La misma copia vive en:

- `DevSuvesaPosWeb/docs/PLAN_MEJORAS_UAT_CONTABILIDAD.md`
- `SuvesaPosSitieWebNew/docs/PLAN_MEJORAS_UAT_CONTABILIDAD.md`

Sale de las pruebas del 2026-11-05 y del 2026-11-06 en CostaPets (contabilidad encendida, libro del emisor Rafael Alberto Martinez Quesada). La evidencia de cada caso, con números de documento y póliza, está en `docs/UAT_CONTABILIDAD_EN_VIVO.md`. Este archivo es solo lo que hay que cambiar.

No sustituye a `docs/PLAN_MEJORAS_PENDIENTES.md` (impresión, facturación e importaciones, 29 sep 2026) ni a `docs/MEJORAS_CONTABILIDAD_API_Y_WEB.md` (activación por sucursal y libro por emisor).

**Fecha:** 2026-11-06.

## Ya está en el código de trabajo, sin commit

Estos cuatro cambios están en el árbol del API. Falta commit y falta ejercitar el anular de la toma.

| Cambio | Dónde | Queda hecho cuando |
|---|---|---|
| Aprobar una toma registra `AjusteInventario` | `TomaFisicaManager.Aprobar` | Aprobar un faltante deja evento. Hoy solo `Guardar` lo hacía, y la pantalla aprueba |
| Aprobar pulsa el worker | `TomaFisicaController.Aprobar` | El evento no espera al barrido periódico |
| Nombre de cajero | `ServicioFacturarPreventaContado` | Si el nombre enviado no coincide con la apertura, reintenta con `c.Usuario` |
| Descripción de línea vacía | `ContabilidadManager` | Una descripción nula se guarda en blanco y no revienta el asiento |

`Anular` de la toma ya reversa con `AjusteInventarioAnulado`. No se probó en vivo.

## Verificado: no es una mejora

El ciclo de caja del 2026-11-06 confirmó la regla. No hay cambio de código aquí.

| Caso | Resultado |
|---|---|
| Apertura | No publica evento. El fondo no entra a `1.1.1.2` |
| Cierre en cuadre (apertura #15, cierre #3) | Diferencia 0, sin evento |
| Faltante hasta ₡5.000 (apertura #16, ₡500) | D `6.1.6.9` / C `1.1.1.2` |
| Sobrante (apertura #17, ₡200) | D `1.1.1.2` / C `4.2.1.1` |
| Faltante sobre el tope (aperturas #18 y #19) | D `1.1.5.1` / C `1.1.1.2`, estado por cobrar al cajero |
| Rebajo por planilla (#18, ₡6.000) | D `2.1.2.1` / C `1.1.5.1` |
| Castigo (#19, ₡7.000) | D `6.1.6.9` / C `1.1.5.1` |

Si el contador decide que el fondo sí debe asentarse al abrir, eso es el punto 21.

## Pendiente — API, contabilidad

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| 1 | El sobrante de toma no tiene asiento. `RegistrarEventoAjusteInventario` solo reparte `CostoDiferencia < 0` y la plantilla `AjusteInventario` solo debita `CostoPerdidas`. La toma 00000006 (ganancia ₡1.502,41) quedó en Inventario #6 con póliza de 0 líneas. Mandar también el costo ganado y agregar en la plantilla el par inventario / sobrante. Una toma mixta parte faltante y sobrante en la misma póliza. | Una toma con unidades de más deja débito de inventario y crédito de sobrante (o reversión de merma), por el costo de esas unidades. |
| 2 | La unidad facturada de una consignación no llega al libro. La boleta de ingreso a la bodega del cliente no es venta: si central y cliente están en la misma sucursal, `PolizaTrasladoIntrasucursal` sale sin evento. `FacturarPrefactura` en contado crea el recibo y no llama a `VentaEmitida` ni a `CobroAplicado`. En crédito sí llama, pero el payload trae solo total, subtotal, impuesto y descuento: faltan `Costo`, `SubTotalGravado`, `SubTotalExento` y el reparto `Inv*` que manda `VentasManager`, y la fórmula rechaza un campo ausente. La prefactura no pasa por `addEnvoiceEntry`. | Facturar una prefactura publica el mismo `VentaEmitida` que una venta normal y, si es contado, el `CobroAplicado`. |
| 3 | El banco del cobro no es el banco del asiento. Un depósito pagado con BANCO NACIONAL debitó `1.1.2.1` BAC colones, la cuenta fija de `CobroAplicado` para `MontoBanco`. Igual en los abonos a proveedor. El payload debe llevar la cuenta bancaria elegida. Si no hay mapeo, el evento queda en error de configuración. | La póliza usa la cuenta del banco del pago. Sin mapeo, no usa la cuenta de otro banco. |
| 4 | El abono se puede confirmar dos veces. UAT-AB-0511 generó Compras #1 y Compras #2, ambos Contabilizado, y CxP quedó debitada ₡22.600 contra una factura de ₡11.300. El alta debe ser idempotente por referencia y factura, o la pantalla debe impedir el segundo guardado. El recibo duplicado se anula en el entorno; no se reintenta. | Un segundo guardado del mismo abono no crea otro recibo ni otra póliza. |
| 5 | El traslado a la bodega de consignación de la misma sucursal no deja rastro. `PolizaTrasladoIntrasucursal` sale si las sucursales son iguales o el costo es 0. Decidir con el contador: documentar que es solo operativo, o agregar dimensión de bodega para ver central contra consignado sin cambiar el saldo de la cuenta. | Queda escrito en la pantalla y en este plan cuál de las dos vías se eligió, y el código hace esa vía. |
| 6 | El artículo 002 (tipo 3) se fue a producto terminado `1.1.6.1.3`, no a mercadería `1.1.6.1.4`. `InventarioPorTipo` manda el tipo 3 a terminado. Revisar el tipo del 002 y del resto del catálogo Costa Pets antes de cerrar un período. | El tipo de cada artículo de la sucursal coincide con la subcuenta que el contador espera. |
| 7 | Compras #14, `CompraRegistrada`, fecha 2026-11-11, sigue en error porque la fecha era posterior al reloj del sistema (2026-11-05). No volver a registrar esa compra. Cuando el reloj pase esa fecha, reintentar desde la bandeja. | El evento #14 pasa a Contabilizado con un reintento, sin un alta nueva. |
| 8 | `EsDeducible` existe en la semilla y en el API. El modal de editar cuenta no la muestra. Hace falta verla y editarla con permiso. | La ficha de la cuenta muestra la marca y, con permiso, la guarda. |
| 9 | La cuenta `1.1.6.1` admite movimiento y tiene subcuentas. El sync del catálogo ya lo avisó. Solo las hojas `1.1.6.1.1` a `1.1.6.1.4` deben moverse. | Un asiento contra `1.1.6.1` se rechaza. |

## Pendiente — API, consignación y comisiones

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| 10 | La liquidación muestra facturado 0. El conteo #5 vendió 1 y la prefactura se facturó, pero el estado muestra factura 0 y facturado 0,000. `Facturado` solo sube si la reserva coincide en artículo y lote. La línea guardó artículo «4» y lote «0», así que no casó con 002 / Test1. La existencia de la bodega del cliente sí bajó. Guardar código de artículo e id real del lote, y acumular `Facturado` con esa llave. | El estado de la liquidación muestra la factura y la cantidad facturada, y el saldo final las resta. |
| 11 | El corte de comisiones liquida todo lo pendiente, cobrado y sin corte de la sucursal entre las dos fechas. El corte #2 se llevó también una comisión anterior. El diálogo «Si, continuar» debe decir cuántos movimientos, de qué beneficiarios y por qué importe entran. Un segundo corte con el mismo rango y estado Confirmado ya se rechaza. | Antes de confirmar se ve el detalle que va a entrar. |
| 12 | El log repite `IX_ComisionMovimientos_IdVentaDetalle_TipoBeneficiario` al tomar la foto de la venta. No bloqueó el guardado. Dejar de registrarlo como error cuando la fila ya existe. | Una venta que ya tiene su foto no escribe ese error. |

## Pendiente — Web

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| 13 | La fecha del estado de consignación se imprime como código. En `Views/Consignacion/Estados.razor` el `@()` cierra antes de `.ToString(...)`. En pantalla salió `5/11/2026 5:40:16 p. m..ToString("dd/MM/yyyy")`. | La celda muestra `dd/MM/yyyy`. |
| 14 | En `Views/Compras/Compra.razor` el formato `:N2%` quedó fuera de la expresión Razor. Se ve `0:N2%`. El mismo patrón está en la fecha de vencimiento del lote de esa rejilla si el `ToString` queda fuera del `@()`. | Descuento, impuesto y vencimiento del lote se ven como importe y fecha. |
| 15 | Comisiones y la prefactura de consignación pintan el id interno («4», «1 — …») en vez del código 002 y el lote Test1. El DTO de la grilla debe traer `CodArticulo` y el número de lote. | La columna muestra código y lote. |
| 16 | Un pago con forma DEPOSITO sobre una factura de crédito contabiliza como `CobroAplicado` solo si queda confirmado. `DepositoConfirmado` es el traslado de efectivo de la caja al banco. El estado negativo del depósito de factura es «No confirmado»; «Rechazado» es el del cheque. Decirlo en la pantalla de depósitos. | La pantalla nombra el evento que va a buscarse en la bandeja. |
| 17 | La conciliación no lista la caja abierta. `ObtenerAperturaDeCajaSinCierre` filtra estado `M`. Una apertura en `A` no aparece hasta el arqueo, y el texto dice «solo aperturas sin cerrar». Con la #7 abierta la lista salió vacía. Listar `A` y `M`, y dejar Cerrar deshabilitado mientras no haya arqueo, con el motivo en la fila. | Una caja recién abierta se ve en la conciliación y no se puede cerrar hasta el arqueo. |
| 18 | El arqueo no muestra el efectivo que el sistema espera. Solo enseña tarjetas y pre-depósitos. Mostrar, como referencia, fondo + efectivo neto al lado del total declarado. | El cajero ve el efectivo esperado mientras cuenta. |
| 19 | El cierre confirma el total de sistema (₡148.966,80 en la #7) y no la diferencia que publica `DiferenciaCaja` (faltante ₡506,59). El texto debe incluir contado, esperado y diferencia, y si cae a gasto o al cajero. | El diálogo nombra la diferencia y la cuenta a la que va. |
| 20 | La apertura responde número 0. `CrearAperturaCaja` guarda el id y devuelve el `request`, que nunca recibió `Napertura`. El aviso dijo «Apertura #0» en la #14 y en la #15 a la #20. Devolver el id asignado. | El aviso muestra el número real. |
| 21 | El fondo no entra al libro. Abrir no publica evento y al cerrar solo se contabiliza la diferencia. Los ₡40.000 de la #7 y los ₡10.000 de las cajas de esta prueba no están en `1.1.1.2`. Decidir con el contador: asentar el fondo al abrir (caja contra el origen del fondo) o dejarlo fuera y documentarlo. | La decisión queda escrita y el código hace esa vía. |

## Orden sugerido

1. Commit de las cuatro correcciones que ya están en el árbol del API.
2. Puntos 2 y 10 juntos: la consignación facturada tiene que salir del inventario y el estado tiene que mostrar lo facturado.
3. Punto 1: sobrante de toma, misma plantilla que el faltante que ya funciona.
4. Punto 3: banco del pago, antes de cerrar un período con cobros reales.
5. Punto 4: anular el abono duplicado en el entorno y cerrar el doble guardado.
6. Puntos 13, 14, 15 y 20: son de presentación y el usuario ya los vio mal.
7. Puntos 17, 18 y 19: la caja ya contabiliza bien; la pantalla es la que confunde.
8. Puntos 5, 6 y 21: decisión del contador, no un cambio a ciegas.
9. Punto 7 cuando el reloj del sistema pase el 2026-11-11. Puntos 8, 9, 11, 12 y 16 en el mismo corte de pulido.
