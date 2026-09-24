# Plan del módulo de Contabilidad General Web

## Propósito

Este plan define la capa Blazor Server del módulo de Contabilidad General. La web no calcula ni persiste reglas contables: consulta, configura bajo permiso y presenta el resultado inmutable devuelto por el API. Debe seguir funcionando si la contabilidad está desactivada, mediante el feature flag `Contabilidad:Habilitada` que recibe desde configuración y permisos.

La habilitación no será global. La empresa habilita la disponibilidad del módulo y cada emisor activa su propia contabilidad. Cada emisor tiene NIF único, libro independiente y estados financieros propios. La relación sucursal-emisor es muchos-a-muchos: una sucursal puede trabajar con varios emisores y un emisor puede estar disponible en varias sucursales. Todo emisor activo debe estar asociado al menos a una sucursal.

## Alcance de pantallas

| Área | Pantallas y responsabilidades |
|---|---|
| Configuración | Habilitación por empresa/emisor, relación sucursal-emisor, catálogo de cuentas, centros de costo, dimensiones, períodos, monedas, políticas y plantillas de contabilización. Toda edición exige permisos específicos y vigencia. |
| Operación | Bandeja de documentos contables, pólizas automáticas, pólizas manuales, detalle de evento, errores de contabilización y reproceso controlado. |
| Consulta | Diario, mayor, auxiliares CxC/CxP/inventario, conciliaciones y bitácora inmutable. |
| Cierre | Prevalidación, cierre mensual/anual, bloqueo de período, reapertura excepcional y evidencia de autorización. |
| Reportes | Balanza, Balance General, Estado de Resultados, Flujo de Efectivo, exportación Excel/PDF y drill-down hasta el documento origen. |

## Arquitectura web

1. `ApiConexion/ProxyInterface/IContabilidad` y `ProxyClass/Contabilidad` consumen exclusivamente endpoints del API. Nunca construyen asientos ni aplican débitos/créditos en el navegador.
2. DTOs específicos en `DTOs/Contabilidad/`; contratos para listados paginados, detalle de póliza, dimensiones, plantillas, estados financieros y conciliación.
3. Servicios de pantalla conservan filtros, formato y navegación. Las fechas predeterminadas se obtienen con `IRelojEquipo`, no con la hora del servidor.
4. Las pantallas usan `AppPantalla`, `AppFiltros`, `AppRejilla`, `AppModal`, `IServicioDialogos` e `IManejadorRespuestas`.
5. Cada póliza enlaza mediante rutas a su factura, cobro, compra, pago, movimiento de inventario o corte de comisión. El enlace es lectura; el documento origen no se altera desde Contabilidad.

## Permisos sugeridos

| Código | Acciones |
|---|---|
| `CONTABILIDAD.CATALOGO_CUENTAS` | VER, CREAR, EDITAR, ACTIVAR |
| `CONTABILIDAD.CONFIGURACION_EMISOR` | VER, CREAR, EDITAR, ACTIVAR |
| `CONTABILIDAD.PLANTILLAS` | VER, CREAR, EDITAR, ACTIVAR |
| `CONTABILIDAD.POLIZAS` | VER, CREAR, EDITAR, IMPRIMIR, EXPORTAR |
| `CONTABILIDAD.REPROCESO` | VER, CREAR, ACTIVAR |
| `CONTABILIDAD.CIERRES` | VER, CREAR, ACTIVAR |
| `CONTABILIDAD.ESTADOS_FINANCIEROS` | VER, EXPORTAR, IMPRIMIR |

Revertir, reabrir períodos y modificar plantillas vigentes requieren autorización reforzada: usuario, fecha, razón y confirmación de clave propia.

> Resuelto en revisión cruzada con el plan API: esta confirmación usa un
> **mecanismo nuevo y separado**, no el `ClaveInterna` que ya usan Agente/SAC
> en Ventas. Contabilidad tiene su propio rol de "contador autorizando un
> cierre", distinto del cajero validado en caja — reabrir un período o
> revertir una póliza publicada es una operación de mayor impacto que no debe
> compartir el PIN corto de una venta. El mecanismo exacto (contraseña de
> Identity reingresada, segundo factor, u otro) lo define A0/A1 del plan API;
> la Web solo debe estar lista para pedir esa confirmación por el canal que
> el API exponga, sin asumir que es el mismo modal de `ClaveInterna` ya
> existente en Facturación.

## Fases web

| Fase | Semanas | Entregables | Dependencia API |
|---|---:|---|---|
| W1 Cimientos | 1–2 | Navegación, feature flag, permisos, DTOs y proxy | Contratos de configuración y períodos |
| W2 Configuración | 2–3 | Cuentas, dimensiones y plantillas versionadas | CRUD y validación de catálogos |
| W3 Operación | 2–3 | Bandeja, detalle, manuales, errores y reproceso | Eventos, pólizas y comandos idempotentes |
| W4 Consulta | 2 | Diario, mayor, auxiliares y conciliación | Consultas paginadas y drill-down |
| W5 Cierre y estados | 2–3 | Cierre, balanza, estados y exportaciones | Motor de cierre y reportes |
| W6 UAT | 1–2 | Casos de contador, accesibilidad y regresión | Ambiente con saldos de prueba |

Correspondencia exacta con las fases A0–A6 del plan API (qué debe estar
*entregado*, no solo iniciado, antes de dar cada fase Web por lista) en
`docs/PLAN_MODULO_CONTABILIDAD_CORRESPONDENCIA_FASES.md`.

## W1 — Decisiones cerradas (esta sesión)

Cierra el entregable de W1 ("Navegación, feature flag, permisos, DTOs y
proxy"), verificado contra el código real de la Web antes de decidir:

| Tema | Decisión | Nota |
|---|---|---|
| Ubicación en el menú | **Módulo raíz nuevo**, mismo nivel que Inicio/Caja/Compras en `MenuSeePos.cs` | El alcance de pantallas ya definido (Configuración, Operación, Consulta, Cierre, Reportes) es tan amplio como el de cualquier módulo raíz existente — anidarlo dentro de otro lo escondería y mezclaría permisos de dominios distintos. |
| Resolución del flag por emisor | **Se consulta al API por emisor activo y se cachea en el estado de sesión Blazor**, no en un claim de Identity fijo al login | El patrón existente (`ContextoSesion.HabilitaImportaciones`, claim resuelto una vez al iniciar sesión) sirve para un flag global del usuario, pero `Contabilidad:Habilitada` varía por emisor y el emisor activo puede cambiar dentro de la misma sesión (Facturación ya lo permite). Se llama al endpoint de activación (dependencia de A1) en el mismo momento en que Facturación ya recarga serie/certificado/cuentas al cambiar de emisor — sin forzar cierre de sesión cada vez que un emisor activa Contabilidad. |
| Visibilidad cuando el emisor activo no tiene Contabilidad habilitada | **Oculto completamente** del menú, no visible-deshabilitado | Un usuario sin Contabilidad activa en el emisor actual no ve el módulo. Un usuario con permiso de configuración que necesite activarlo llega a esa pantalla por otra vía (configuración de emisor), no desde un ítem de menú deshabilitado. |
| Confirmación de clave propia (reautenticación de A1) | **Componente modal reutilizable único** para todo Contabilidad | Mismo espíritu que `AppBoton`/`AppCargable` (indicadores de carga compartidos, ver memoria de sesión). Un solo punto de mantenimiento para el flujo de contraseña de Identity reingresada, en vez de reimplementarlo por pantalla con riesgo de que una copia quede desactualizada o diverja en seguridad. |

## Criterios de aceptación web

- Ninguna pantalla contable se muestra o llama al API si el feature flag está apagado.
- La disponibilidad efectiva exige empresa habilitada y emisor habilitado; un emisor inactivo no impide operar con los demás.
- Un documento muestra su estado contable, identificación de póliza y error legible sin revelar excepción técnica.
- Los importes se presentan por moneda y los reportes indican empresa, período, moneda y fecha/hora del equipo de generación.
- No existe botón de editar sobre una póliza contabilizada; solo reversión autorizada o póliza manual nueva.
- Las exportaciones preservan filtros, identificador de ejecución y trazabilidad de origen.
- Facturación presenta exclusivamente emisores vigentes asociados a la sucursal activa y permitidos al usuario.
- Con un emisor disponible se autoselecciona; con varios se exige selección; con ninguno se muestra el bloqueo de configuración.
- Cambiar una regla contable genera una versión con fecha de vigencia y nunca altera la visualización histórica de una póliza.

## Decisiones contables confirmadas

| Decisión | Comportamiento requerido en la Web |
|---|---|
| Moneda funcional | La moneda funcional y de presentación de la compañía será CRC (colones). Debe mostrarse en configuración como dato controlado por compañía. |
| Operaciones en USD | La captura permite USD. Antes de confirmar, muestra importe USD, tipo de cambio de venta del día, equivalente CRC y fuente de la tasa. Al publicar, la tasa queda solo de lectura junto a la póliza. |
| Centros de costo iniciales | Se cargarán **Productos Veterinarios** y **Alimento animales y accesorios**. La pantalla permite crear, editar vigencia y desactivar centros posteriores, pero nunca eliminar uno con movimientos. |
| IVA soportado | La cuenta 1.1.7.1 IVA Soportado se presenta como cuenta de impuesto acreditable para importaciones, compras y gastos. |
| IVA devengado | La cuenta 2.1.3.1 IVA Devengado se presenta como impuesto generado por ventas; las notas de crédito lo disminuyen. |
| Activación | La empresa habilita la disponibilidad del módulo y cada emisor activa su contabilidad individual. Una empresa puede tener emisores activos y otros sin Contabilidad. |
| Independencia del emisor | Cada emisor tiene NIF, libro, períodos, pólizas, impuestos y estados financieros propios; no comparte libro con otro emisor. |
| Relación operativa | Una sucursal puede mostrar uno o varios emisores asociados y vigentes. |

La pantalla de configuración debe incorporar una importación controlada del catálogo suministrado: vista previa, árbol jerárquico, validación de códigos duplicados, mapeo de naturaleza/tipo y confirmación explícita. La importación no debe publicar cuentas automáticamente si hay errores.

## Reglas específicas de experiencia

- El usuario no digita una tasa libre en una transacción USD normal. La Web consulta la tasa de venta vigente para la fecha del documento y enseña el cálculo antes de registrar.
- Si la tasa no existe, debe impedir la publicación contable y explicar que falta la tasa de venta para la fecha fiscal; nunca debe convertir a cero ni usar la tasa del servidor por defecto.
- Los centros de costo deben ser selectores buscables. Una cuenta o plantilla que los requiera no puede contabilizar sin selección válida.
- El catálogo mostrará cuenta contable, descripción, tipo, naturaleza, si permite movimiento, si es cuenta de control, moneda permitida, vigencia y mapeo a estado financiero.
- La ficha del artículo/familia debe poder llevar un centro de costo predeterminado. La póliza conservará el centro finalmente usado, aunque luego cambie el predeterminado.
- Una factura o compra que mezcle artículos de ambos centros debe mostrar el desglose por centro en su vista contable. El centro del encabezado no puede ocultar ni reemplazar la clasificación del detalle.
- Toda consulta contable debe mostrar y filtrar empresa, emisor y sucursal, respetando las asociaciones vigentes y los permisos del usuario.
- En Facturación, el selector de emisor se alimenta exclusivamente con los emisores asociados a la sucursal activa. Si hay uno, se selecciona y se muestra; si hay varios, la selección es obligatoria; si no hay ninguno, se bloquea la emisión con un mensaje de configuración.
- Al cambiar de emisor en Facturación se limpian y recargan serie, actividad económica, certificado, cuentas bancarias y demás datos dependientes. Si ya existe detalle capturado, la Web debe advertir y evitar combinar configuraciones de dos emisores.

## Pantalla de configuración contable por emisor

La configuración no se construye como valores cerrados. Un usuario con permiso podrá seleccionar y versionar, por emisor:

- Cuentas para retención, intereses, comisiones por pagar, depósitos/cheques en tránsito, cheques rechazados, bancos, redondeos y diferencias cambiarias.
- Política de IVA soportado, IVA no acreditable, IVA devengado, exoneraciones, importaciones y notas de crédito.
- Fuente y clase de tipo de cambio, tratamiento de feriados, revaluación y redondeo.
- Reglas de centros de costo y distribución de gastos compartidos.
- Momento de devengo contable de comisiones.
- Calendario fiscal, períodos, cierres, reapertura y aprobadores.
- Estados de Hacienda que permiten contabilizar o exigen reversión.
- Método de inventario y cuentas aplicables por tipo de movimiento.

Las cuentas se eligen mediante buscadores filtrados por naturaleza, tipo y condición de cuenta de movimiento. Antes de activar una configuración, la Web muestra validaciones y una simulación de los asientos principales. Una modificación crea una nueva versión con vigencia; no cambia pólizas históricas.
