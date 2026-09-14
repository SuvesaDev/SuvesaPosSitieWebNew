# Plan del Módulo de Reportes ERP Web

## Propósito y decisión de alcance

Este documento define cómo construir el módulo de reportes del sitio Blazor para una distribuidora. Es la guía de implementación del frente web y debe leerse junto con `../../DevSuvesaPosWeb/ApiSuvesaPos/docs/PLAN_MODULO_REPORTES_ERP_API.md` antes de iniciar cambios.

La decisión es construir un módulo de consulta y análisis con datos reales del API. No se deben portar las cinco pestañas maqueta del React anterior ni mostrar KPIs cuya fuente no esté definida. El primer entregable cubre ventas, cartera, caja, compras, inventario, lotes y trazabilidad; logística avanzada, cuotas y rentabilidad neta quedan condicionadas a completar sus datos fuente.

## Fuentes analizadas

- `TICOFOODSTER.docx`: antigüedad de CxC, estados de cuenta, facturas pendientes y detalle de abonos.
- `Reportes_ERP_Distribuidora_Consolidado_Optimizado vf.docx`: catálogo de ventas, compras, inventario, cartera, caja, agentes, lotes, bonificaciones, auditoría y panel gerencial; además propone DSO, ABC, DSI, OTIF, cobertura, cuota, churn y rentabilidad neta.
- Estado actual del proyecto: `/moduloReportes` es `Views/Reportes/Compras.razor` y consume únicamente `ObtenerReporterCompras`. El catálogo React de reportes no tiene llamadas ni acciones reales. Ya existen pantallas operativas que sirven de fuente, entre ellas Facturación, Compra, Cobrar, Cuentas por Cobrar, Cuentas por Pagar, Caja, Conciliación, Inventarios, Lotes, Producción y Seguimiento de Cotizaciones.

## Usuarios, decisiones y permisos

| Rol operativo | Necesita ver | Puede exportar o imprimir | Puede administrar definición |
| --- | --- | --- | --- |
| Administrador | Todo el catálogo y la auditoría | Sí | Sí, según permisos de Parámetros |
| Gerente | Tablero gerencial, ventas, margen, cartera, inventario, compras y caja consolidados | Sí | No cambia reglas ni fuentes |
| Cajero | Apertura, arqueo, cierre, movimientos, predepósitos y depósitos propios | Sí, de su alcance | No |
| Contador | CxC, CxP, cobros, compras, caja, depósitos confirmados, fiscal y auditoría | Sí | No, salvo permiso explícito |
| Auxiliar contable | Mismo dominio del contador, con datos sensibles limitados por sucursal si aplica | Sí | No |
| Control de inventario | Existencias, kardex, lotes, vencimientos, mermas, ajustes, rotación y reorden | Sí | No |
| Jefatura de facturación | Ventas, documentos fiscales, devoluciones, descuentos, notas de crédito y productividad | Sí | No |
| Personal de facturación | Sus documentos, ventas y errores de emisión; no márgenes globales ni auditoría | Sí, de su alcance | No |
| Control y trazabilidad de inventario | Lotes, movimientos, compra-origen, venta-destino, vencimientos y producción | Sí | No |

La seguridad se implementará por códigos de función y acciones `VER`, `EXPORTAR` e `IMPRIMIR`. No se codifican permisos por nombre de rol. El administrador configura cada rol en la matriz ya existente. Los filtros de sucursal, empresa y agente se aplican en el API, no solo se ocultan en la interfaz.

## Catálogo priorizado

### Fase uno datos disponibles y de mayor valor

1. **Ventas y facturación:** ventas netas por fecha, cliente, vendedor, familia, proveedor o artículo; detalle de documentos; contado/crédito; descuentos; devoluciones; notas de crédito; ventas por hora; top de clientes, artículos y servicios.
2. **Cuentas por cobrar:** antigüedad 1 a 30, 31 a 60, 61 a 90 y más de 90 días; estado de cuenta; facturas pendientes; aplicaciones y recibos; recuperación por periodo; DSO cuando la definición de fecha de vencimiento esté validada.
3. **Caja y depósitos:** aperturas, arqueos, cierres, diferencias, movimientos; predepósitos; depósitos y cheques por estado de confirmación. Los pendientes se muestran separados de lo confirmado y nunca se suman como efectivo conciliado.
4. **Compras y cuentas por pagar:** detalle de compras, proveedor, impuesto, descuentos, vencimiento, saldo y pagos. Se reemplaza el listado actual sin filtros por un reporte parametrizable.
5. **Inventario y lotes:** existencia y valoración, kardex, lotes por vencimiento, artículos bajo mínimo, ajuste de inventario y trazabilidad venta-lote-compra cuando exista el vínculo de lote.
6. **Bonificaciones, devoluciones y auditoría:** bonificaciones entregadas, devoluciones de venta y compra, bitácora de cambios, reimpresiones y cambios de estado de cotizaciones.

### Fase dos requiere validación de datos o nueva captura

- ABC/Pareto, rotación y DSI: viables tras acordar costo base, periodo de consumo, existencia por bodega y tratamiento de bonificaciones/devoluciones.
- Margen por factura, artículo, vendedor y cliente: requiere costo histórico congelado por línea de venta y una regla para flete, comisiones y costos indirectos.
- Comisiones, cumplimiento de cuota, rentabilidad por agente y zona: no deben construirse hasta tener metas por agente y periodo, reglas de comisión y costos asociados.
- Cobertura, visitas, efectividad de ruta, OTIF, carga de vehículos y heatmap: requieren entidad de ruta, planificación, visita real, entrega, motivo de incumplimiento, vehículo y geolocalización. No se infieren desde facturas.
- Churn, reactivación y penetración de catálogo: requieren periodos y definición comercial aprobada; sí pueden iniciar como análisis de última compra y familias compradas.
- Mermas, quiebres y ventas perdidas: los ajustes existen, pero hace falta clasificar motivo, capturar demanda no atendida y distinguir vencimiento, daño, pérdida y conteo físico.
- Trazabilidad inversa de producción: hay cabecera y líneas de producción, pero se debe confirmar que cada insumo y producto terminado guarda lote y cantidad trazable.

## Experiencia de usuario

### Página inicial

La ruta `/moduloReportes` pasa a ser un catálogo, no un reporte de compras. Muestra tarjetas agrupadas por Ventas, Finanzas y Caja, Compras, Inventario y Trazabilidad, Comercial y Auditoría. Cada tarjeta indica su objetivo, filtros disponibles, última actualización y permisos requeridos. Las funciones no autorizadas no aparecen.

### Patrón de una pantalla de reporte

1. Cabecera con nombre, descripción, ayuda de fórmula y acciones Exportar Excel, Exportar PDF e Imprimir cuando el rol lo permita.
2. `AppFiltros` reutilizable: empresa, sucursal, rango de fecha obligatorio, moneda, cliente, proveedor, familia, artículo, bodega, agente, estado fiscal y condición, según corresponda.
3. Consulta explícita. No cargar millones de filas al abrir ni recalcular por cada pulsación.
4. Tarjetas de resumen y una tabla `AppRejilla` o isla `MudDataGrid` para consultas pesadas. La tabla debe paginar en servidor, ordenar por columnas permitidas y mostrar una fila de total coherente con los filtros.
5. Gráficas solo cuando agreguen lectura: tendencia mensual, composición por categoría, antigüedad y comparativos. Deben tener la misma tabla accesible debajo y no ser la única fuente del dato.
6. Enlace de desglose: desde una cifra se abre el detalle filtrado, conservando los filtros y sin duplicar cálculos en el navegador.

### Reglas visuales y de seguridad

- Fechas de filtro se inicializan desde el reloj del equipo; el API recibe fechas de negocio sin convertirlas de manera ambigua a UTC.
- Los montos se calculan y redondean en el servidor con `decimal`; la web solo formatea.
- El estado de un depósito o cheque usa las etiquetas definidas: Pendiente de confirmación, Confirmado, Recuperación, Cobrado, No confirmado y Rechazado para cheque.
- Las exportaciones deben incluir título, periodo, filtros, usuario y hora de generación. No se exportan más filas de las autorizadas ni datos fuera del alcance de sucursal.

## Contrato esperado del API

El sitio no consulta tablas ni descarga conjuntos completos. Para cada reporte consume un contrato con:

```text
Filtro común + dimensiones específicas
Resumen (KPIs y totales)
Página de filas ordenada en servidor
Metadatos: fecha de corte, moneda, fórmula o notas, total de registros
Token o identificador de exportación cuando el archivo se genera en segundo plano
```

Los DTOs del sitio se crean a partir de contratos regenerados o, mientras el contrato se estabiliza, clientes manuales encapsulados en `ApiConexion`. No se edita código generado.

## Secuencia de implementación web

### W0 contrato y navegación

- Crear el árbol de funciones bajo `REPORTES` con categorías y reportes hoja. Regenerar la semilla de seguridad desde `MenuSeePos.cs` y revisar permisos de rol.
- Convertir `/moduloReportes` en catálogo y conservar el reporte de compras como una hoja concreta, no como página raíz.
- Definir el componente compartido de filtros, estado de carga, formato de totales, explicación de métricas y exportación.

### W1 finanzas operativas

- Construir Compras, CxP, Antigüedad CxC, Estado de cuenta, Facturas pendientes y Recuperación de cartera.
- Construir Caja y depósitos, con separación visual de pendientes bancarios.
- Reusar los PDF de recibo/factura solo para documentos; los reportes consolidados se exportan desde el nuevo endpoint de reporte.

### W2 ventas e inventario

- Construir Ventas netas/detalle, descuentos/devoluciones/notas y top de clientes/artículos.
- Construir Kardex, existencias, lotes/vencimientos y trazabilidad por artículo.
- Incorporar gráficos de tendencia únicamente sobre los agregados que entregue el API.

### W3 indicadores avanzados

- Activar ABC, DSI, margen y comportamiento de clientes solo al aprobar fórmulas y fuentes.
- Agregar rutas, cuotas, visitas, flete y OTIF después de que sus módulos transaccionales existan y pasen pruebas de calidad de datos.

## Pruebas y aceptación web

- Pruebas de permisos: un cajero no ve margen gerencial; un contador sí ve cartera; control de inventario no ve datos financieros no concedidos.
- Pruebas de filtros: el mismo periodo y dimensión da igual total en pantalla, PDF y Excel.
- Pruebas de límites: tabla paginada, carga y exportación no congelan el circuito Blazor.
- Pruebas de accesibilidad: tablas navegables, estados no dependientes solo de color y filtros etiquetados.
- Pruebas E2E con datos semilla controlados para una venta, devolución, nota, cobro, depósito pendiente/confirmado, compra, lote y vencimiento.

## Criterios de salida de fase uno

- Todo total mostrado tiene definición, fuente, filtros y moneda explícitos.
- CxC concilia con el mayor de cuentas por cobrar, no con un campo calculado en el navegador.
- Caja concilia con `MovimientoCaja`; depósitos/cheques pendientes no se presentan como efectivo ni cobro confirmado.
- Toda exportación reproduce exactamente los filtros y respeta autorización.
- Los reportes intensivos consultan páginas o agregados del servidor y no cargan el historial completo en Blazor.

## Decisiones que debe aprobar Producto antes de fase dos

1. Fórmula oficial de venta neta, costo, margen, DSO, DSI, cartera vencida y devolución.
2. Alcance de sucursal y empresa para cada rol.
3. Si una bonificación reduce margen y cómo se atribuye a cliente, agente y proveedor.
4. Modelo operativo de rutas, visitas, cuota, entrega, flete y ventas perdidas.
5. Retención histórica, horario de corte y límite permitido de exportación.
