# Plan de mejoras pendientes

> **29 sep 2026, rama `develop`.** Lo que ya está en producción no se reescribe.
> Este archivo junta tres revisiones: plantillas de impresión, facturación /
> inventario / producción, importaciones (caso DUA 005-2026-593304) y compras
> locales con XML. La misma copia está en `DevSuvesaPosWeb/docs/PLAN_MEJORAS_PENDIENTES.md`.
>
> Detalle de impresión ya escrito en
> `docs/MOTOR_PLANTILLAS_IMPRESION_WEB.md` y, en el API,
> `docs/MOTOR_PLANTILLAS_IMPRESION_API.md`.

Orden entre proyectos: los pasos de datos del API (**P1–P6** y **F1–F5**) van
antes que el botón o la pantalla que los muestra. Un botón nuevo sobre el
membrete o el stock equivocado no cierra el hueco.

---

## Proyecto API — DevSuvesaPosWeb

### Impresión

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| P1 | Al guardar la transacción, congelar nombre del creador, empresa (id y razón social) y sucursal (id y nombre). La reimpresión lee esa foto. | Un cambio posterior de catálogo o de usuario no altera un PDF ya emitido. |
| P2 | Todo PDF, A4 y térmico, muestra empresa, sucursal y «Elaborado por», aunque la plantilla guardada las haya ocultado. | Salen en factura, tiquete, recibos, boletas, compras, traslados, tomas y consignaciones. |
| P3 | Si el documento no trae empresa, no usar el primer emisor. Resolver por la sucursal de la transacción, o rechazar el PDF. | Recibo de pago, recibo legado, presupuesto, devolución interna, toma general, ajuste, traslado y toma física dejan de salir con otra empresa. |
| P4 | Sucursal, bodegas, formas de pago, motivo y saldo entran al catálogo con clave estable y el render los pinta. | Esos datos, que hoy se calculan y se descartan, aparecen en el PDF. |
| P5 | Nuevo tipo de impresión para la factura de compra del proveedor (`Compra`), con el usuario y la empresa de esa compra. | Existe proveedor y slug. El botón del sitio es W4. |
| P6 | Reimpresión fiel: condición y medio de pago salen de la venta guardada; el saldo del recibo es el de ese cobro; el QR apunta al comprobante de esa empresa, no a `https://costapets.com/`. | Reimprimir no cambia título, condición, saldo ni destino del QR. |
| P7 | Toma general no tiene usuario. El presupuesto no guarda creador. En el alta nueva, guardarlo. En lo histórico, imprimir «No consta». | Esos PDF traen una línea de responsable. |
| P8 | Si no viene `formato`, usar el de la plantilla predeterminada. Hoy el controlador fuerza A4. | Un tiquete reimpreso sin query usa su plantilla térmica cuando esa es la predeterminada. |
| P9 | Bitácora de reimpresión (quién, cuándo, documento, motivo). `BitacoraReimpresiones` existe y nadie escribe en ella. | Cada reimpresión deja rastro. |

### Facturación, inventario y producción

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| F1 | Si el artículo es servicio, guardar la línea de venta, compra o devolución y no mover existencia. Hoy el kardex rechaza el servicio y la línea igual pide el movimiento, así que el documento completo falla. | Una factura con un servicio se emite y el stock de ese ítem no cambia. |
| F2 | Exigir bodega en venta, compra y preventa (`Inventario:ExigirBodegaEnTransaccion` hoy no está en la configuración y queda apagado). Asignar sucursal a las bodegas que siguen globales. El chequeo de disponible y el descuento usan la misma bodega. | Una venta de una sucursal no descuenta la bodega de otra ni la primera bodega CostaPets por omisión. |
| F3 | Encender `Ventas:ExigirCobroContado` cuando la caja nueva cuadre. Hoy está en `false`: la ruta vieja `CrearFactura` y la consignación de contado pueden emitir sin pago. El tiquete por los comandos nuevos ya exige el 100 %. | Un documento de contado no se emite si el pago no cubre el total. |
| F4 | Pasar edición y borrado de compra por el mismo kardex del alta, con lote y sin lote. `EnvoiceEditNew` sigue en el camino anterior. | Corregir una compra no descuadra existencia ni lotes. |
| F5 | Al convertir una producción, actualizar el costo del terminado con los insumos consumidos. La cantidad y el lote ya se mueven en la bodega elegida; `Inventario.Costo` del terminado no. | El costo del artículo producido coincide con la suma de insumos de esa conversión. |
| F6 | Antes de producir, el terminado es tipo 3 y la materia prima tipo 2, ambos con lote. Un artículo normal sin lote no entra en la calculadora; si se fuerza el tipo sin lotes reales, el ingreso y la venta no pegan en el mismo saldo. | Solo se convierte una fórmula cuyos artículos ya están clasificados y con lote. |
| F7 | Migración de consignación vieja: correr primero en simulación. No crear ingresos si el stock tipo 2 ya está en la bodega del cliente. La bonificación de la prefactura la arma el API, no solo el sitio. | El saldo consignado no se duplica y la prefactura trae las bonificaciones. |
| F8 | `CrearMuchasFacturas` devuelve qué documento se guardó y cuál falló, con el motivo. Hoy omite los fallos en silencio. | Quien llama ve la lista de rechazados. |
| F9 | No reabrir el motor de factura electrónica 4.4. Seguir solo con series habilitadas, sin cambiar la clave en un reintento y sin reenviar solos un rechazo de Hacienda. | Se vigila; no es un rediseño. |
| F10 | Poder modificar una preventa pendiente (líneas, cantidades, cliente) antes de cobrarla o facturarla. Hoy `venta/EditarFactura` no comprueba que siga siendo preventa, mueve stock por el camino viejo y no respeta el lote. El sitio no llama ese endpoint. La excepción que sí edita es la prefactura de consignación, solo en estado Editable. | Desde la pantalla se cambian líneas de una preventa no anulada y no facturada, y el inventario (reserva o descuento, lote y bodega) queda igual al de haberla creado de nuevo. |

### Importaciones

Medido contra el DUA 005-2026-593304 (RD 135382, levante TICA y facturas BPCR).
El módulo ya guarda documentos con hash, ratea por valor, entra lote a bodega
y puede enviar mensaje receptor de aceptación total. El costo de inventario de
ese expediente, sin capitalizar el IVA, es **₡5,023,520.93**.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| I1 | Separar, por línea, el IVA aduanal acreditable y la Ley 6946. Solo la Ley 6946 entra al costo. Hoy `TributoDuaUnitario` se describe como «Ventas 13% + Ley 6946» y el cierre lo suma entero al costo nacionalizado. En este DUA el IVA es **₡489,877.11** y la Ley 6946 es **₡37,309.76**. | El inventario no incluye el 13%. Ese IVA queda como crédito fiscal. |
| I2 | Cada costo en moneda extranjera guarda su tipo de cambio, la moneda y el monto original. La mercancía de este caso usa **453.89** (el de la liquidación). El flete BPCR usa **452.02**. El levante usa **453.50** y no se le suma otra vez el flete de la factura. | No hay un solo `TipoCambioDua` convirtiendo toda la importación. |
| I3 | No asociar un XML a la importación si el DUA citado en la observación es otro. La factura de EBBA de mayo (clave `506050526003101723070001000010100000079054159808793`) cita el DUA **005-2026-305686**, movimiento **300676**, no este embarque. | Ese XML no puede colgarse del DUA 005-2026-593304. |
| I4 | El cierre no mueve inventario mientras un XML de esta importación no tenga mensaje receptor aceptado, o deja el crédito fiscal en suspenso hasta la aceptación. Hoy se puede cerrar con el XML en «Pendiente». | No se toma crédito de una factura que Hacienda no aceptó. |
| I5 | Al cerrar, el costo del artículo es promedio ponderado: `(existencia anterior × costo anterior + cantidad entrada × costo nacionalizado) / existencia nueva`. Hoy se reemplaza `Inventario.Costo` por el de esta entrada. | Una segunda importación no borra el costo de la primera. |
| I6 | Mensaje receptor con aceptación total (1), rechazo (2) y aceptación parcial (3). Hoy el envío va fijo en mensaje 1. El consecutivo no cambia si Hacienda rechaza el mensaje. | El usuario puede rechazar o aceptar parcial, y el impuesto aceptado es el que queda como crédito. |
| I7 | La proforma del recinto (EBBA 156394, movimiento 317736, **₡266,769.58** sin IVA) se guarda como estimado y se sustituye cuando llegue la factura electrónica de ese mismo movimiento. | El costo del recinto no queda cerrado sobre una proforma. |
| I8 | PROCOMER (**₡1,361.67**) y timbres (**₡72.00**) se ratean por valor de mercancía, igual que flete y honorarios. La Ley 6946 no se ratea: ya viene exacta por línea. No se suma el flete teórico del CIF (USD 412.77) además de la factura de flete. | El costo de las 4,400 unidades cuadra con **₡5,023,520.93** cuando el recinto siga en el estimado de la proforma. |
| I9 | Certificado de origen y registro sanitario por producto, con alerta seis meses antes del vencimiento. Los lotes de esta importación vencen en 2028. | El DAI en cero queda documentado y el registro sanitario no se vence en silencio. |
| I10 | Reporte de IVA acreditable (aduana y facturas locales aceptadas) separado de Ley 6946, PROCOMER y timbres, para la D-104. | El contador no rearma el expediente a mano. |

### Compras locales (XML, lotes y Hacienda)

La compra nueva ya asocia cada línea a un artículo, exige lote si el artículo lo maneja, rechaza la misma clave de Hacienda dos veces y el sitio ofrece aceptar, aceptar parcial o rechazar desde Documentos aceptados. El inventario se mueve al guardar, antes de esa aceptación.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| C1 | Guardar la compra en una sola transacción. Hoy se graba el encabezado y cada línea por separado: si un lote falla a mitad, queda una compra incompleta y la clave ya no se puede volver a importar. | Si una línea falla, no queda encabezado, stock ni clave registrados. |
| C2 | Si el artículo es servicio, guardar la línea y no mover existencia. Hoy el kardex lo rechaza y tumba toda la factura. | Una compra con un servicio se registra y ese ítem no cambia el stock. |
| C3 | Actualizar el costo del artículo con promedio ponderado al recibir la compra. Hoy la cantidad entra al kardex y `Inventario.Costo` no se recalcula en `InsertarLineaCompra`. | El costo del catálogo refleja esta entrada y la existencia anterior. |
| C4 | La edición de una compra ya está en producción: el sitio abre la factura, cambia líneas y llama a `EnvoiceEditNew`. Ese camino no sirve. Siempre manda el lote con id 0, así que no entra al ajuste por lote: descuenta la cantidad nueva del saldo general, no devuelve la cantidad original, y el segundo movimiento deja el acumulado en el saldo ya rebajado. Una línea que se quita en pantalla no viaja en el guardado, así que sigue en la compra y en el inventario. Hay que revertir el movimiento original (cantidad, bodega y lote) y aplicar las líneas nuevas por el mismo kardex del alta, en una sola transacción. Si el mensaje receptor de esa clave ya fue aceptado, no se pueden cambiar cantidades, impuesto ni total. | Editar o quitar una línea deja el kardex igual a haber registrado la compra corregida desde el inicio, y una factura ya aceptada por Hacienda no cambia sus montos. |
| C5 | No tomar el crédito de IVA de la compra mientras el mensaje receptor no esté aceptado por Hacienda. La pantalla ya manda los códigos correctos: 1 aceptado, 2 parcial, 3 rechazado. El inventario puede entrar al guardar; el crédito no. | Una compra pendiente no suma crédito en la D-104. |
| C6 | Avisar las compras con clave electrónica que siguen sin mensaje receptor. No hay alerta por antigüedad. | La bandeja de pendientes muestra desde cuándo espera cada factura. |

### Orden de compra manual

La orden ya numera por serie operativa, distingue nacional e internacional, imprime PDF y lo manda por correo. No mueve inventario. El seguimiento solo marca entregada, cancelada, dada de baja o facturada.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| O1 | Mientras la orden esté pendiente y no se haya enviado, se puede corregir cantidad, precio, artículo y condición. Hoy solo se crea: no hay edición. | Un cambio de la orden pendiente queda en el mismo consecutivo, y una orden ya enviada o entregada no se reescribe. |
| O2 | Recibir por línea: cantidad recibida, cantidad pendiente y recepción parcial. Hoy «Marcar como entregada» cierra toda la orden de un golpe y no compara con lo que llegó. | Una línea puede quedar parcial y la orden no pasa a entregada hasta completar o cerrar la diferencia. |
| O3 | Generar la factura de compra desde la orden, con el proveedor, las cantidades recibidas, el precio y la bodega de la sucursal. El enlace actual pide un id interno y no revisa que esa compra exista, sea del mismo proveedor ni cubra las cantidades. La pantalla lo rotula como «N.º de factura». | La compra nace de la orden y el inventario entra una sola vez, por el kardex de compra, con lote si el artículo lo maneja. |
| O4 | No marcar facturada una orden cuya compra no existe. No volver a marcar entregada una orden ya facturada. Hoy las dos transiciones lo permiten. | El estado sigue el orden pendiente, entregada, facturada, y no se puede saltar ni deshacer a mano. |
| O5 | Si no viene emisor, no usar el primero de la base. El tipo de cambio de una orden internacional no puede quedar en 1 por omisión. | La orden sale con la empresa de la sucursal y el tipo de cambio que se capturó. |
| O6 | Enviar el correo deja constancia (fecha y destino) y pasa la orden a enviada. Hoy el envío no cambia el estado. | Se distingue una orden pendiente de una ya mandada al proveedor. |

### Traslados de bodega

El traslado ya sale de origen y entra a destino en la misma transacción, no deja negativo en origen, rechaza bodegas de consignación y, al anular, devuelve el lote al origen si el destino todavía tiene la cantidad. El inventario pasa de una bodega a la otra en el mismo instante: no hay mercadería en tránsito.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| T1 | Si el artículo maneja lote, el traslado exige el lote y que esa cantidad exista en la bodega de origen. Hoy la pantalla deja agregar «Sin lote» y el API rechaza todo el traslado al final. El selector de lote lista lotes del artículo en cualquier bodega. | No se puede registrar un traslado de un artículo con lote si el lote no tiene saldo en el origen. |
| T2 | Un servicio no se puede trasladar. Hoy el kardex lo rechaza a mitad del documento y deshace el traslado. | El servicio ni se ofrece en la lista. |
| T3 | Las dos bodegas tienen que ser del centro de la sesión. Hoy una bodega sin sucursal (global) se acepta contra cualquier centro, y el listado de un centro incluye traslados de bodegas globales. | Un traslado no mueve stock de una sucursal a otra por una bodega huérfana. |
| T4 | El número interno sale de la serie de la sucursal y del emisor de esa sucursal. Hoy se toma el primer emisor y, si la serie falla, el traslado se guarda igual sin número. | Todo traslado aplicado tiene consecutivo, o no se guarda. |
| T5 | El costo de la línea es el costo del artículo en ese momento, no un costo distinto por lote. Se conserva como valor del documento, y el costo del catálogo no cambia por el traslado. | El papel del traslado cuadra con el costo vigente y una anulación no lo altera. |

### Toma física

La toma ya excluye servicios, abre una fila por lote activo, fija el saldo contado en esa bodega y deja el consecutivo igual al id. Solo ajusta las líneas que el usuario contó: un campo en blanco no pone el artículo en cero. El texto de la pantalla dice que resetea toda la bodega, y eso no es lo que hace el guardado.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| TF1 | Una toma completa exige todas las filas de esa bodega. Una toma parcial solo ajusta lo contado y queda marcada como parcial. Hoy el aviso dice que se resetea la bodega, y guardar solo manda las líneas con cantidad. | Un conteo a medias no se archiva como si se hubiera contado todo el almacén. |
| TF2 | El conteo no puede ser negativo. Hoy el ajuste absoluto lo permite y deja existencia bajo cero. | Un contado menor que cero se rechaza. |
| TF3 | Un artículo que maneja lote y tiene saldo sin lote, o no tiene lotes activos, aparece en la toma. Hoy esas existencias no salen en la lista. | Ese saldo se puede contar y ajustar. |
| TF4 | La bodega es obligatoria y tiene que ser del centro. Hoy, si llega en cero, se usa la primera bodega operativa que no sea de consignación. | La toma no cae en otra sucursal. |
| TF5 | Mientras la toma está abierta, esa bodega no acepta otro conteo encima, y el guardado compara la existencia de sistema que se mostró. Si una venta la cambió, se avisa y no se pisa en silencio. | Dos conteos simultáneos no se borran uno al otro. |
| TF6 | Anular una toma revierte cada ajuste, devuelve el saldo anterior y marca el documento anulado. Hoy un conteo mal guardado solo se corrige con otra toma. | Anular deja el kardex como antes de esa toma. |
| TF7 | El reporte valora tanto las unidades ganadas como las perdidas, con el costo del artículo. Hoy solo costea las pérdidas. | El costo de la diferencia, a favor y en contra, queda en el reporte. |

### Correo y alertas

El motor ya encola el comprobante cuando Hacienda lo acepta, reintenta con espera creciente, adjunta XML firmado, respuesta y PDF, y abre una alerta si el rechazo o el correo agotan los intentos. La campana del sitio lista esas alertas. El cheque y el depósito sin confirmar también generan alerta.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| E1 | Si el comprobante aceptado no tiene correo de destinatario, se crea alerta. Hoy queda «Omitido sin destinatario» y nadie se entera. | Una factura aceptada sin correo aparece en la campana el mismo ciclo. |
| E2 | El correo de la orden de compra, el del estado de consignación y cualquier envío suelto usan la misma cola, los mismos reintentos y la misma alerta. Hoy la orden llama al SMTP directo y un fallo no queda en la bandeja. | Un correo que no salió se ve en envíos y se reintenta solo. |
| E3 | La alerta de SMTP inválido es una por emisor, no una por cada comprobante. Hoy se abre una por clave y el comprobante sigue reintentando cada 30 minutos. | El administrador ve un aviso del emisor, y los comprobantes esperan sin repetir la misma alerta. |
| E4 | Las alertas de instrumento sin confirmar y de comprobante rechazado pueden avisarse al correo del administrador del emisor, no solo a la campana. Hoy la campana es el único canal. | Quien no tiene la pantalla abierta recibe el aviso. |
| E5 | Alertas de operación que el motor todavía no cubre: mensaje receptor de compra pendiente, lote por vencer y existencia bajo el mínimo. Cada una queda en la misma tabla de alertas, sin un worker distinto. | Esas tres situaciones aparecen en la campana, con su tipo propio. |

### Caja, cobros, abonos y bancos

Apertura, arqueo, conciliación, cobrar, predepósito, generar depósito y consulta de depósitos ya existen. La conciliación nueva lee `MovimientoCaja`. El cobro de contado estricto sigue apagado (`Ventas:ExigirCobroContado` en false), así que una venta vieja puede emitirse sin entrar a esa caja. Abono Cobrar y Cobrar cobran preventas y facturas de crédito; el recibo sale de Cobrar y de cuentas por cobrar, no siempre del mismo lugar. El predepósito saca el efectivo de la apertura y deja el cheque o el depósito pendiente de confirmar en la cuenta bancaria.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| CA1 | Apertura, tiquete, factura de contado, abono a cobrar y compra de contado escriben el mismo `MovimientoCaja` de esa apertura. Depende de **F3**: con el cobro estricto apagado, la conciliación no ve todo el efectivo. | El esperado del arqueo coincide con la suma de movimientos de esa apertura. |
| CA2 | El cierre usa solo la conciliación nueva. El cierre viejo, que lee formas de pago sueltas, no puede cerrar la misma apertura otra vez. | Una apertura tiene un solo cierre y una sola diferencia. |
| CA3 | El predepósito exige una apertura abierta del cajero y una cuenta bancaria de la empresa de esa sucursal. El efectivo sale una sola vez, al predepositar. Generar el depósito bancario agrupa predepósitos y no vuelve a restar caja. Si se juntan varios, se conserva el número de cada apertura. Hoy, con más de un predepósito, ese número se pierde. | El banco aumenta y la caja no baja dos veces. Cada depósito se puede rastrear hasta su apertura. |
| CA4 | Confirmar un cheque o un depósito en consulta de depósitos actualiza el instrumento una sola vez y no vuelve a mover la caja ni el saldo del cliente. | Un depósito confirmado no duplica el abono en cuentas por cobrar. |
| CA5 | Un abono a cobrar, sea desde Cobrar o desde Abono Cobrar, aplica facturas, baja el saldo, emite el mismo recibo y entra a la caja de la apertura vigente. Una caja ya cerrada no recibe ese abono. | El saldo del cliente, el recibo y el movimiento de caja son los mismos en las dos pantallas. |
| CA6 | La devolución en efectivo de una venta de esa apertura sale de `MovimientoCaja`. Hoy, con el cobro estricto apagado, esa salida puede no registrarse. | El arqueo descuenta la devolución en efectivo de la misma apertura. |

### Devoluciones

La devolución de venta ya devuelve la cantidad al lote vendido, no deja devolver más de lo facturado y arma la nota de crédito con la serie electrónica de esa sucursal. La de compra saca el producto al proveedor y exige lote si el artículo lo maneja. El reintegro en efectivo de la venta solo se anota en caja si el cobro de contado estricto está encendido, y en ese caso se asume efectivo aunque la factura original haya sido de crédito.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| DV1 | El reintegro sigue la forma de la venta. Contado en efectivo sale de la apertura. Crédito baja el saldo de cuentas por cobrar y no toca la caja. Depósito o anticipo no se convierten en efectivo. Hoy, con **F3** apagado, la caja no se entera; con **F3** encendido, toda devolución se trata como efectivo. | El arqueo y el saldo del cliente cambian según cómo se cobró la factura. |
| DV2 | La devolución de compra descuenta el saldo pendiente con el proveedor y no deja devolver más de lo comprado ni de otro lote. Las líneas de una segunda devolución de la misma factura quedan en esa devolución. Hoy `idDevolucionCompra` toma la primera devolución de esa compra y las líneas nuevas se le cuelgan. | Cada devolución de compra tiene sus líneas, y la cuenta por pagar baja solo por lo devuelto. |
| DV3 | Si una línea de la devolución de venta falla, no queda la nota de crédito a medias. Hoy el encabezado se guarda antes de terminar todas las líneas. | O se guarda la devolución completa o no queda ninguna. |
| DV4 | Un servicio en la factura se devuelve sin mover inventario. Hoy el kardex lo rechaza y puede dejar la devolución a medias. | La nota de crédito del servicio se emite y el stock no cambia. |
| DV5 | Anular una devolución revierte el inventario, el reintegro y, si la nota de crédito electrónica ya se emitió, no se borra: se exige el documento que Hacienda acepte. | Anular no deja el producto en bodega y el dinero fuera de caja a la vez. |

### Reportes

Hay 28 reportes de operación con filtro, gráfica, Excel y PDF. El tope es de 2.000 filas. El inventario, cuando se filtra por bodega o sucursal, sale del kardex y no del total global del artículo. El filtro de empresa y sucursal lo manda la pantalla; el API no lo ata a las sucursales del usuario.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| RP1 | Los totales y los indicadores del periodo se calculan sobre todas las filas, no sobre las primeras 2.000. La tabla puede seguir paginada. Hoy un mes cargado muestra un total incompleto. | El panel y el Excel del periodo cuadran con la facturación, aunque haya más de 2.000 documentos. |
| RP2 | El usuario solo consulta las empresas y sucursales que su sesión puede ver. Hoy, si se quita el filtro, el reporte trae todas. | Un usuario de una sucursal no ve las ventas ni la caja de otra. |
| RP3 | Recuperación de cuentas por cobrar, caja, arqueo y depósitos leen el cobro, el movimiento de caja y el depósito nuevos. La recuperación hoy arma el recibo desde el abono legado. | Esos reportes coinciden con Cobrar, la conciliación y consulta de depósitos. |
| RP4 | La rentabilidad usa el costo que quedó en la línea al vender, no el costo actual del catálogo. | Cambiar el costo hoy no reescribe el margen de una venta ya hecha. |
| RP5 | El inventario de una sucursal incluye las bodegas de ese centro. Una bodega sin sucursal no se suma en silencio ni se esconde sin aviso. | El encargado ve el stock de su centro y sabe si hay existencia en una bodega global. |

### Seguridad, usuarios, roles y perfiles

El sitio ya filtra el menú por código de función y el perfil SUPER_ADMIN ve todo. El mantenimiento de roles y perfiles en el API exige perfil con gestión de usuarios para leer y superadministrador para escribir. El resto de los controladores de negocio (ventas, compras, caja, inventario) solo exigen un JWT válido.

| Id | Mejora | Queda hecho cuando |
|---|---|---|
| SG1 | Cada endpoint de negocio comprueba la misma acción (ver, crear, editar, borrar) que el menú. Un token de un cajero no puede llamar facturación, compras ni seguridad aunque conozca la ruta. | Quitar el botón en pantalla y llamar el API con ese usuario da el mismo rechazo. |
| SG2 | `ValidarClaveInternaSinUsuario` deja de aceptar cualquier clave interna de la base. Solo valida la del usuario de la sesión, igual que el cambio de la propia clave. | No se puede probar claves ajenas con un token cualquiera. |
| SG3 | La clave de entrada y la clave interna se guardan con hash. El login compara el hash. Hoy el alta copia la clave que manda el cliente. | Un respaldo de la base no muestra las contraseñas. |
| SG4 | No se puede quitar el último superadministrador ni el propio rol de quien está editando, sin otra cuenta superadmin activa. | La instalación no se queda sin quien administre usuarios. |
| SG5 | Al desactivar un usuario o cambiarle rol o perfil, los tickets ya emitidos dejan de servir en el siguiente llamado. Hoy el JWT sigue válido hasta que vence. | Un usuario dado de baja no opera con la sesión que ya tenía abierta. |

---

## Proyecto sitio — SuvesaPosSitieWebNew

### Impresión

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| W1 | Botón imprimir, con permiso Imprimir, en toma general, ajuste, traslado de bodega, presupuesto, devolución interna y en la pantalla de devolución de venta (hoy la nota de crédito solo sale desde la bandeja). | — | Cada pantalla abre `/documentos/{slug}/{id}/pdf`. |
| W2 | La bandeja de ventas pide siempre factura electrónica. Pedir tiquete cuando el documento es tiquete. La reimpresión de consulta va en A4; el térmico queda para el puesto que acaba de cobrar. | — | El slug coincide con el tipo real. |
| W3 | En Cobrar, ofrecer el recibo de cobro cuando el cobro generó uno, sin quitar el comprobante de la venta. El recibo hoy se abre desde cuentas por cobrar. | — | El cajero imprime el papel que acaba de producir. |
| W4 | Botón de la factura de compra del proveedor. | P5 | El PDF trae la empresa y el usuario de esa compra. |
| W5 | En el editor de plantillas, empresa, sucursal y «Elaborado por» no se pueden ocultar. El rótulo por defecto pasa de «Atendido por» a «Elaborado por». | P2 | Esas tres líneas quedan fijas en toda plantilla. |

### Facturación, inventario y producción

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| S1 | Facturación y compra permiten agregar un servicio y guardar el documento. La pantalla no pide lote ni existencia para ese ítem. | F1 | Un servicio en la factura no muestra error de stock. |
| S2 | Facturación, compra, preventa y producción no dejan confirmar si la línea que mueve stock no tiene bodega del centro. | F2 | No se envía bodega 0 en un artículo con existencia. |
| S3 | Con el cobro de contado encendido, el puesto no ofrece emitir contado si el pago no cubre el total. El tiquete ya lo exige en pantalla. | F3 | La ruta vieja y la consignación de contado muestran el mismo bloqueo. |
| S4 | La pantalla de producción muestra el costo del terminado que devolvió la conversión, junto con cantidades y lotes. | F5 | El usuario ve el costo con el que quedó el artículo. |
| S5 | La prefactura de consignación muestra las bonificaciones que armó el API y, en la migración, solo ejecuta el simulacro hasta que el saldo coincida. | F7 | No hay botón de aplicar ingresos sobre un saldo ya cargado. |
| S6 | En la preventa pendiente, botón para modificar líneas y cantidades antes de cobrar. Cobrar y la bandeja hoy solo consultan o facturan. | F10 | Se abre la preventa, se guarda el cambio y el total que se cobra es el nuevo. |

### Importaciones

La pantalla `Views/Compras/Importaciones.razor` ya pide tributo DUA por línea, costos con IVA acreditable, lotes y el XML del proveedor local. Estas mejoras cambian lo que se le pide al usuario para que no capitalice el IVA.

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| U1 | El tributo por línea se captura en dos campos: IVA aduanal (no entra al costo) y Ley 6946 (sí entra). El rótulo deja de decir un solo «Tributo DUA». | I1 | En el resumen, el IVA acreditable y el costo nacionalizado se ven por separado. |
| U2 | Al agregar un costo en dólares, se pide el tipo de cambio de esa factura. El flete no hereda el tipo de cambio del DUA. | I2 | La factura BPCR de flete queda a 452.02 y la mercancía a 453.89. |
| U3 | Al subir un XML, se muestra el DUA citado. Si no coincide con el expediente, no se asocia. La factura de EBBA de mayo queda fuera de este DUA. | I3 | El usuario ve el rechazo antes de guardar el documento. |
| U4 | Aceptar, rechazar o aceptar parcial el XML desde la importación. El cierre advierte si todavía hay un mensaje sin aceptar. | I4, I6 | El estado del documento pasa de Pendiente a Aceptado, Rechazado o Parcial. |
| U5 | La proforma del recinto se marca como estimada. Cuando llega la factura electrónica del mismo movimiento, sustituye ese costo. | I7 | El costo nacionalizado cambia al reemplazar la proforma y el usuario ve cuál documento lo sostiene. |
| U6 | El reporte de la importación muestra el IVA acreditable aparte del costo de inventario, listo para la D-104. | I10 | El total de costo no incluye el ₡489,877.11 de IVA de este DUA. |

### Compras locales (XML, lotes y Hacienda)

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| K1 | Al importar el XML, exigir lote y vencimiento solo si el artículo asociado los maneja, y exigir que la suma de las líneas aplicadas coincida con el total del XML. El botón de aplicar ya bloquea el lote incompleto. | C1 | No se puede aplicar un XML cuyo total no cuadra o cuyo artículo con lote no tiene lote. |
| K2 | Una línea de servicio importada del XML no pide lote ni bodega. | C2 | El servicio se aplica a la compra sin error de existencia. |
| K3 | Después de guardar, la pantalla muestra el costo ponderado que quedó en el artículo y abre el mensaje receptor sin obligar a ir a otra pantalla. El inventario ya entró; el crédito espera la aceptación. | C3, C5 | El usuario ve el costo nuevo y puede aceptar, aceptar parcial o rechazar en el mismo momento. |
| K4 | La lista de documentos aceptados muestra hace cuántos días está pendiente cada factura. | C6 | Una factura electrónica sin mensaje no se queda solo en una lista plana. |
| K5 | Al abrir una compra ya guardada, «Guardar cambios» revierte el inventario anterior y aplica el nuevo, lote incluido. Quitar una línea la elimina de verdad. Si Hacienda ya aceptó el mensaje, la pantalla no deja cambiar cantidades, impuesto ni total. | C4 | El usuario corrige una compra sin dejar stock de más ni una línea fantasma, y una factura aceptada queda bloqueada en sus montos. |

### Orden de compra manual

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| R1 | En una orden pendiente, poder corregir líneas antes de enviarla. Después de enviada o entregada, la orden queda de solo lectura. | O1 | El mismo consecutivo se actualiza y no se crea otra orden para corregir un precio. |
| R2 | En el seguimiento, registrar lo recibido por línea y dejar la diferencia pendiente. El botón deja de cerrar toda la orden. | O2 | Se ve cantidad pedida, recibida y pendiente. |
| R3 | Desde la orden entregada, abrir la compra ya cargada con esas líneas y esa bodega. El campo deja de pedir un número interno con el rótulo de factura. | O3 | El usuario no copia la orden a mano en Compras. |
| R4 | Mostrar si el correo se envió, a quién y cuándo. | O6 | Una orden pendiente no se confunde con una ya mandada. |

### Traslados de bodega

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| B1 | Al elegir el artículo, el lote es obligatorio si lo maneja, y la lista solo muestra lotes con existencia en la bodega de origen, con la cantidad disponible. | T1 | No aparece la opción «Sin lote» en un artículo que lleva lote, y no se puede pedir más de lo que hay en el origen. |
| B2 | El buscador de artículos del traslado no ofrece servicios. | T2 | Un servicio no entra a la lista del traslado. |
| B3 | Las bodegas del formulario son solo las del centro de la sesión, no las globales de otro lado. | T3 | Origen y destino son del mismo centro. |
| B4 | Después de registrar, y en el historial, el botón imprime el PDF del traslado. El API ya lo genera. | W1 | El usuario no tiene que buscar el documento en otra pantalla. |

### Toma física

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| G1 | Al guardar, la pantalla dice si la toma es parcial o completa. El aviso deja de decir que se resetea toda la bodega cuando solo se ajustan las líneas contadas. | TF1 | El usuario confirma explícitamente antes de dejar en cero lo que no contó. |
| G2 | No se puede escribir un conteo negativo. | TF2 | El campo no acepta menos de cero. |
| G3 | La lista muestra el saldo sin lote de un artículo que sí maneja lotes, para poder contarlo. | TF3 | Esa existencia no queda fuera de la pantalla. |
| G4 | La bodega a contar es solo una del centro, y es obligatoria. | TF4 | No se abre la toma sin bodega. |
| G5 | Si el sistema cambió mientras se contaba, se muestra el aviso y no se guarda hasta recontar esa línea. Anular una toma guardada queda en la misma pantalla. | TF5, TF6 | El usuario ve el choque con una venta y puede revertir un conteo mal aplicado. |

### Correo y alertas

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| Y1 | La campana y la pantalla de alertas distinguen «sin destinatario», «SMTP del emisor» y «comprobante rechazado», y desde la alerta se abre el documento. | E1, E3 | El usuario no tiene que leer solo el texto largo para saber qué hacer. |
| Y2 | En facturación, si el cliente no tiene correo de comprobante, se avisa antes de emitir. El modal de correos sigue siendo donde se agregan. | E1 | No se emite en silencio una factura que el motor va a omitir. |
| Y3 | La pantalla de envíos permite reintentar a mano un correo fallido y ver el último error. La orden de compra muestra si el correo quedó en la cola. | E2 | Un fallo de SMTP no se pierde en un mensaje de una sola vez. |
| Y4 | En alertas, las de lote por vencer, stock mínimo y factura de compra sin aceptar en Hacienda usan la misma lista. | E5 | Esas alertas se marcan leídas igual que las de correo. |

### Caja, cobros, abonos y bancos

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| Z1 | Arqueo y conciliación muestran el mismo total esperado, armado con los movimientos de esa apertura. | CA1, CA2 | El cajero no ve una cifra en arqueo y otra en conciliación. |
| Z2 | Predepósito y generar depósito solo ofrecen cuentas de la empresa de la sucursal. Generar depósito no pide de nuevo el efectivo. | CA3 | No se deposita en una cuenta de otra empresa y el efectivo no sale dos veces. |
| Z3 | Consulta de depósitos confirma el cheque o el depósito sin volver a cobrar la factura. | CA4 | El estado pasa a confirmado y el saldo del cliente no se mueve otra vez. |
| Z4 | Abono Cobrar y Cobrar imprimen el mismo recibo y exigen apertura abierta. Una caja cerrada no deja cobrar. | CA5, W3 | El recibo y la caja quedan ligados al abono que se acaba de hacer. |

### Devoluciones

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| H1 | En la devolución de venta se elige cómo se reintegra: efectivo de la caja abierta, saldo a favor del cliente de crédito, o el mismo medio del cobro. La nota de crédito se imprime desde esa pantalla. | DV1, W1 | El cajero no devuelve efectivo de una factura que fue a crédito, y el PDF de la nota queda a la mano. |
| H2 | En la devolución de compra se elige el lote comprado y se ve el saldo que le queda al proveedor después de devolver. | DV2 | No se puede devolver un lote que esa factura no ingresó, y el saldo por pagar baja en pantalla. |
| H3 | Un servicio en la factura original se devuelve sin pedir lote ni bodega. | DV4 | La línea de servicio entra a la nota sin error de inventario. |

### Reportes

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| Q1 | Si el periodo trae más de 2.000 filas, el total de arriba sigue siendo el del periodo completo y la tabla dice que está paginada. El Excel descarga el periodo completo. | RP1 | El usuario no toma un total cortado como el cierre del mes. |
| Q2 | Empresa y sucursal arrancan en las de la sesión y no se pueden dejar en blanco para ver las demás. | RP2 | El filtro no abre el resto de las sucursales. |
| Q3 | Caja, depósitos y cuentas por cobrar muestran los mismos estados que las pantallas de cobro y de banco. | RP3, CA1 | Un depósito pendiente en el reporte es el mismo que en consulta de depósitos. |
| Q4 | La rentabilidad rotula el costo como el de la venta, no el del catálogo de hoy. | RP4 | El margen no cambia al reabrir el reporte después de una compra nueva. |

### Seguridad, usuarios, roles y perfiles

| Id | Mejora | Depende de | Queda hecho cuando |
|---|---|---|---|
| V1 | Si el API rechaza por permiso, la pantalla muestra denegado y no un error genérico. El menú sigue ocultando lo que el rol no tiene. | SG1 | Un usuario sin permiso no llega al formulario aunque pegue la ruta. |
| V2 | Cambiar rol, perfil o clave de otro usuario pide la clave interna de quien administra, como ya hace la matriz de roles. | SG4 | No se escala un usuario a superadmin con solo la sesión abierta. |
| V3 | Al guardar un usuario desactivado, la sesión del sitio lo manda a ingresar de nuevo. | SG5 | La sucursal no sigue operando con un usuario que ya se dio de baja. |
