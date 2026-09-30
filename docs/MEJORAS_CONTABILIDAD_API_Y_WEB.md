# Mejoras de Contabilidad — API y Web

Documento de trabajo para `DevSuvesaPosWeb` y `SuvesaPosSitieWebNew`, rama `feature/modulo-contabilidad`. La copia vive en `docs/` de los dos repos y tiene que mantenerse igual.

El motor, el catálogo editable, los eventos, los auxiliares, el cierre y las pantallas W1–W6 ya están. Este documento lista solo lo que falta para cumplir el diseño multi-emisor / multi-sucursal, con una corrección de activación.

## Decisión que reemplaza la de A1

La activación de la contabilidad va en la **sucursal**, no en la **empresa**.

Hoy el efectivo es `EmpresaHabilitada && EmisorHabilitado`:

- API: `EstadoActivacionContabilidadDTO.Efectivo` y `ResolutorLibroContable.ResolverSiHabilitado`.
- Web: `ConfiguracionEmisor.razor` muestra el badge de empresa, y `ContextoContabilidad` oculta el menú según el emisor.

Eso se retira. El libro, el catálogo, los períodos y los estados oficiales siguen siendo del **emisor**. La sucursal no tiene libro. Lo que decide si un documento se poliniza es si **esa sucursal** tiene la contabilidad encendida.

| Pieza | Dueño |
|---|---|
| Libro, catálogo, plantillas, períodos, cierre, estados oficiales | Emisor |
| Encendido / apagado operativo | Sucursal |
| Disponibilidad de datos y permisos | Empresa, sin flag de contabilidad |

Regla efectiva para un documento:

1. La sucursal del documento tiene contabilidad activa.
2. El emisor del documento tiene libro.
3. Existe `EmisorSucursal` vigente entre ese emisor y esa sucursal.

Si falta cualquiera, el documento operativo se guarda igual y el evento queda no aplicable. Apagar una sucursal no borra pólizas ya publicadas en el libro del emisor.

⚠️ Una sucursal puede tener dos emisores. Con el flag en la sucursal, los dos publican cuando esa sucursal está encendida. Si hiciera falta encender solo a uno, el flag tendría que bajar a `EmisorSucursal`. Por ahora se cumple lo pedido: una sucursal, un interruptor.

## Lo que ya cumple y no se rehace

- Outbox en la misma transacción del documento y worker aparte.
- Plantillas versionadas por libro, con fórmula propia.
- Catálogo por emisor, alta y edición (código, tipo, naturaleza y padre solo si la cuenta no tiene pólizas).
- Fecha fiscal inmutable y fecha contable contra el período del libro.
- Repolinización por reverso, sin borrar historia.
- Auxiliares CxC, CxP, inventario y caja contra cuentas de control, tolerancia 0.01.
- Cierre mensual y anual por libro, reapertura en cascada con contraseña de Identity.
- Estados desde las pólizas. Flujo de efectivo por método directo.
- `EmisorSucursal` como relación muchos a muchos.
- Moneda funcional CRC y tipo de cambio de venta. Centros de costo como dimensión extensible.
- Menú de contabilidad escondido cuando el módulo no está efectivo.

## Mejoras del API

### 1. Activación por sucursal

Nueva fila `ContabilidadConfiguracionSucursal` (`IdSucursal`, `Habilitada`, auditoría). No se reutiliza `ContabilidadConfiguracionEmpresa` como interruptor.

`Efectivo` pasa a `SucursalHabilitada && EmisorConLibro && RelacionVigente`. `EmpresaHabilitada` sale del DTO y del resolutor.

`ResolutorLibroContable` deja de leer `Empresas.Actual` y `ContabilidadConfiguracionesEmpresa`. El método de publicación recibe emisor y sucursal del documento:

`ResolverSiHabilitado(int idEmisor, int idSucursal)`.

Si la sucursal no está activa, devuelve null. El libro sigue saliendo de `ContabilidadConfiguracionEmisor` del emisor del documento. Crear el libro al configurar el emisor se mantiene: es alta del libro, no el interruptor.

`ResolverPorSucursalSiHabilitado` (toma física, comisiones) exige además que esa sucursal esté activa. Sigue sin adivinar el emisor cuando hay varios y ninguno es predeterminado.

Llamadas a actualizar para pasar la sucursal del documento: `VentasManager`, `ComprasManager`, `ServicioCobroCredito`, `ServicioFacturarPreventaCredito`, `ServicioDevolucionInterna`, `ServicioNotaCreditoCxC`, `InstrumentosConfirmacionManager`, `ConsignacionInventarioManager`, `ImportacionesManager`. Toma física y comisiones ya parten de la sucursal.

Endpoints: activar y desactivar sucursal, y el estado de activación consultado por sucursal (y emisor, para saber si hay libro). Permiso `CONTABILIDAD.CONFIGURACION_EMISOR` se conserva o se renombra al de sucursal; no se suelta el `ExigePermiso` forzado de contabilidad.

Pruebas: `ObtenerEstadoActivacionAsync_ExigeAmbosNiveles` y las semillas `SeedContabilidadHabilitadaAsync` dejan de prender la empresa. Una venta en sucursal apagada no crea evento; la misma venta en sucursal encendida sí, sobre el libro del emisor.

### 2. El documento guarda emisor y sucursal en el evento

Cada `EventoContable` (o su payload) lleva `IdEmisor`, `IdSucursal` y el id de empresa solo como dato de grupo, no como llave del libro. La plantilla de traslado y de venta lee la sucursal como dimensión, no como otro libro.

### 3. Póliza de un solo libro

Al publicar, cada línea usa una cuenta de ese `IdLibroContable`. Una cuenta de otro emisor se rechaza. Una póliza manual pide emisor (libro) y sucursal; la sucursal tiene que estar activa y ligada a ese emisor, pero el asiento no se parte entre libros.

### 4. Intrasucursal

El traslado de bodega o sucursal del mismo emisor emite un evento del libro de ese emisor: débito y crédito a la misma cuenta de inventario (`1.1.6.1` en Tico Foodster) con distinta dimensión de sucursal. No crea CxC ni CxP. Si el costo no cambia, el saldo de la cuenta de control no se mueve.

### 5. Catálogo del Excel, antes de la primera póliza

Cargar `INFORMACION TICO.xls` como catálogo del primer emisor, después de corregir en la pantalla (la pantalla ya lo permite mientras no haya pólizas):

| Problema del Excel | Acción |
|---|---|
| `2.1.2.2` dos veces (cargas sociales y retención 2 %) | Dejar cargas sociales. La retención va a un código libre. Hueco natural: `2.1.3.2`. ⚠️ El contador lo confirma. |
| `2.1.2.3` ya es aguinaldo y `2.1.2.4` ya es retención de salario | No usarlos para retenciones ni para comisiones por pagar. |
| `56-02-01` intereses | Pasar a `6.3.2.1`. |
| Faltan banco en dólares, depósitos en tránsito y comisiones por pagar | Agregarlas en el catálogo (`1.1.2.2`, `1.1.2.3` y el siguiente pasivo laboral libre). ⚠️ Confirmar códigos. |
| Nombres cortados y grupos sin hijas (`2.2`, `6.4.1`) | Completar el nombre. Un grupo sin hijas o permite movimiento o no se usa en pólizas. |

### 6. Cobro con depósito o cheque pendiente

`VentasManager.RegistrarCobroContado` registra `CobroAplicado` al emitir, también cuando el cobro queda `PendienteConfirmacion`. La confirmación del instrumento vuelve a intentar la misma clave de idempotencia, así que no se duplica, pero un rechazo no revierte ese evento.

Cambio: si hay instrumento pendiente, no se publica `CobroAplicado` en la venta. Se publica en `DepositoConfirmado` o `ChequeConfirmado`. `ChequeRechazado` y `DepositoNoConfirmado` no deben dejar el cobro reconocido.

### 7. Disparador Hacienda

La política cerrada es contabilizar venta y compra solo con comprobante aceptado. Hay que revisar que `VentaEmitida`, `CompraRegistrada` e `ImportacionCerrada` salgan en ese momento y no al guardar el documento en estado pendiente.

### 8. Consolidación e intercompany — no entra en este corte

No se agregan `intercompany_transactions` ni libro de grupo hasta que el contador defina cuentas de partes relacionadas y participaciones. Una venta entre emisores, el día que se configure, son dos pólizas en dos libros. Sumar emisores en un reporte de empresa no es estado oficial y no elimina operaciones internas.

## Mejoras de la Web

### 1. Pantalla de activación

`ConfiguracionEmisor.razor` deja de prender la empresa. La pantalla elige sucursal y muestra:

- sucursal encendida o apagada;
- emisor ligado (libro existe o no);
- contabilidad efectiva para esa pareja.

Activar y desactivar actúan sobre la sucursal. Crear el libro del emisor sigue en la configuración del emisor, separado del interruptor.

### 2. Menú y contexto de sesión

`ContextoContabilidad` resuelve con la sucursal del centro abierto, no solo con el emisor. `FiltroMenu` oculta `CONTABILIDAD` cuando esa sucursal está apagada. Al cambiar de centro se vuelve a resolver. El cache por emisor (`_cachePorEmisor`) pasa a ser por sucursal, o por pareja emisor–sucursal.

### 3. Selectores de las pantallas contables

`CatalogoCuentas`, bandeja, diario, mayor, auxiliares, balanza, estados, cierre, plantillas, dimensiones, bitácora y reproceso hoy llaman `EstadoActivacion(empresa, emisor)`. Pasan también la sucursal de trabajo para saber si pueden operar, y el libro lo siguen tomando del emisor seleccionado. El catálogo y el cierre no se filtran por sucursal: son del libro. Los reportes operativos (diario, auxiliares) aceptan sucursal como filtro de dimensión.

### 4. Catálogo

Ya permite nueva cuenta, agregar hija y editar. Falta usarlo para el alta del Excel corregido (mejora API 5) y mostrar el aviso cuando el API rechace un cambio de código porque ya hay pólizas.

### 5. Póliza manual y traslado

La captura manual elige emisor y sucursal activa. El traslado entre sucursales del mismo emisor no ofrece cuentas de cliente ni de proveedor.

## Orden

1. API: flag de sucursal, resolutor y pruebas de “sucursal apagada no poliniza”.
2. Web: pantalla, menú y selectores contra ese contrato.
3. Ajuste de `CobroAplicado` y del disparador Hacienda.
4. Carga del catálogo de Tico Foodster ya corregido, en modo sombra, una sucursal piloto.
5. Consolidación, solo si el contador la pide después.

## Pruebas que cierran el corte

- Sucursal apagada: la venta se guarda y no hay evento contabilizado.
- Sucursal encendida, emisor con libro, relación vigente: el evento cae en ese libro.
- Misma empresa, otra sucursal apagada: tampoco poliniza.
- Dos emisores en la sucursal encendida: cada documento va al libro de su emisor.
- Traslado entre sucursales del mismo emisor: el saldo de `1.1.6.1` no cambia y no hay movimiento de CxC ni CxP.
- Cuenta con pólizas: el código no cambia. Cuenta sin pólizas: `56-02-01` puede pasar a `6.3.2.1`.
- Menú de contabilidad visible solo con la sucursal del centro encendida.
