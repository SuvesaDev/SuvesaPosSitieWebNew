# Plan del módulo de Contabilidad General Web

## Propósito

Este plan define la capa Blazor Server del módulo de Contabilidad General. La web no calcula ni persiste reglas contables: consulta, configura bajo permiso y presenta el resultado inmutable devuelto por el API. Debe seguir funcionando si la contabilidad está desactivada, mediante el feature flag `Contabilidad:Habilitada` que recibe desde configuración y permisos.

## Alcance de pantallas

| Área | Pantallas y responsabilidades |
|---|---|
| Configuración | Catálogo de cuentas jerárquico, centros de costo, dimensiones, períodos, monedas y plantillas de contabilización. Toda edición exige permisos específicos y vigencia. |
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
| `CONTABILIDAD.PLANTILLAS` | VER, CREAR, EDITAR, ACTIVAR |
| `CONTABILIDAD.POLIZAS` | VER, CREAR, EDITAR, IMPRIMIR, EXPORTAR |
| `CONTABILIDAD.REPROCESO` | VER, CREAR, ACTIVAR |
| `CONTABILIDAD.CIERRES` | VER, CREAR, ACTIVAR |
| `CONTABILIDAD.ESTADOS_FINANCIEROS` | VER, EXPORTAR, IMPRIMIR |

Revertir, reabrir períodos y modificar plantillas vigentes requieren autorización reforzada: usuario, fecha, razón y confirmación de clave propia.

## Fases web

| Fase | Semanas | Entregables | Dependencia API |
|---|---:|---|---|
| W1 Cimientos | 1–2 | Navegación, feature flag, permisos, DTOs y proxy | Contratos de configuración y períodos |
| W2 Configuración | 2–3 | Cuentas, dimensiones y plantillas versionadas | CRUD y validación de catálogos |
| W3 Operación | 2–3 | Bandeja, detalle, manuales, errores y reproceso | Eventos, pólizas y comandos idempotentes |
| W4 Consulta | 2 | Diario, mayor, auxiliares y conciliación | Consultas paginadas y drill-down |
| W5 Cierre y estados | 2–3 | Cierre, balanza, estados y exportaciones | Motor de cierre y reportes |
| W6 UAT | 1–2 | Casos de contador, accesibilidad y regresión | Ambiente con saldos de prueba |

## Criterios de aceptación web

- Ninguna pantalla contable se muestra o llama al API si el feature flag está apagado.
- Un documento muestra su estado contable, identificación de póliza y error legible sin revelar excepción técnica.
- Los importes se presentan por moneda y los reportes indican empresa, período, moneda y fecha/hora del equipo de generación.
- No existe botón de editar sobre una póliza contabilizada; solo reversión autorizada o póliza manual nueva.
- Las exportaciones preservan filtros, identificador de ejecución y trazabilidad de origen.
