# Plan del Módulo de Reportes ERP Web

## Propósito y decisión de alcance

Este documento define cómo construir el módulo de reportes del sitio Blazor para una distribuidora. Es la guía de implementación del frente web y debe leerse junto con `../../DevSuvesaPosWeb/ApiSuvesaPos/docs/PLAN_MODULO_REPORTES_ERP_API.md` antes de iniciar cambios.

La decisión es construir un módulo de consulta y análisis con datos reales del API. No se deben portar las cinco pestañas maqueta del React anterior ni mostrar KPIs cuya fuente no esté definida. El primer entregable cubre ventas, cartera, caja, compras, inventario, lotes y trazabilidad; logística avanzada, cuotas y rentabilidad neta quedan condicionadas a completar sus datos fuente.

## Fuentes analizadas

- `/Users/amartinez/Downloads/TICOFOODSTER.docx`: antigüedad de CxC, estados de cuenta, facturas pendientes y detalle de abonos.
- `/Users/amartinez/Downloads/Reportes_ERP_Distribuidora_Consolidado_Optimizado vf.docx`: catálogo de ventas, compras, inventario, cartera, caja, agentes, lotes, bonificaciones, auditoría y panel gerencial; además propone DSO, ABC, DSI, OTIF, cobertura, cuota, churn y rentabilidad neta.

> **Nota de vigencia (14 sep 2026):** el tercer punto original de esta lista ("Estado actual del proyecto: `/moduloReportes` consume únicamente `ObtenerReporterCompras`") describía el estado *antes* de este módulo y ya no es cierto — ver **`## Estado actual auditado`** más abajo para lo que existe hoy. Se conserva el resto de este documento (decisiones de UX, contrato, fases) porque sigue siendo la guía vigente; lo que cambia es qué falta de él.

## Estado actual auditado (14 sep 2026)

Auditoría de código sobre `feature/modulo-reportes` hecha para retomar este plan sin repetir trabajo. La fase W0/W1/W2 de este documento **ya se construyó** en buena medida — no como un catálogo de tarjetas por dominio con permisos VER/EXPORTAR/IMPRIMIR por función, sino como un único hub con pestañas.

### Lo que existe y funciona

- `Views/Reportes/Compras.razor` (`/moduloReportes`) es hoy un **hub con ~19 tipos** de reporte agrupados en 4 dominios visuales (Ventas y cartera, Compras, Caja y bancos, Inventario), cada uno con filtros de fecha/sucursal/empresa/cliente/proveedor/artículo/bodega/lote/texto libre, indicadores (tarjetas de KPI) y botones Exportar Excel/PDF — consume `GET api/reportes-operacion/{tipo}` vía `IReportesOperacion`/`ReportesOperacion.cs`.
- `Services/GeneradorReporteOperacion.cs` ya genera Excel (ClosedXML) y PDF (QuestPDF) de forma genérica para cualquier tipo del hub — es el motor de exportación reutilizable que el plan pedía en W0.
- `Views/Shared/Componentes/AppGraficosReporte.razor` + `Services/ResumenGraficasReporteOperacion.cs` ya dan un gráfico por tipo de reporte (barras horizontales dibujadas a mano, sin librería).
- `Views/Ventas/Comisiones.razor` (`/sales/commissions`) es el reporte de comisiones, con export CSV/XLSX propio y cierre de período.
- `Views/Ventas/EstadoCuenta.razor` ya cubre "estado de cuenta por cliente" con antigüedad y export PDF/CSV — el requerimiento explícito de `TICOFOODSTER.docx` de estado de cuenta individual ya existe.

### Bugs y deuda concretos detectados (antes de sumar reportes nuevos, conviene resolver esto)

1. **Exportar Excel/PDF da 404 en 9 de los ~19 tipos.** El whitelist de `Program.cs:430` (`GET /reportes/operacion/{tipo}/{formato}`) solo acepta `ventas, cuentas-por-cobrar, cuentas-por-pagar, caja, arqueos-cierres, depositos, compras, inventario, lotes, trazabilidad, auditoria`. El botón Excel/PDF se muestra igual para `ventas-detalle`, `clientes`, `rentabilidad`, `ventas-compras`, `recuperacion-cxc`, `inventario-abc`, `rotacion-inventario`, `bonificaciones` → el usuario hace clic y recibe un error silencioso (404). Es un bug de cara al usuario, no solo deuda técnica; corregirlo (ampliar el whitelist o quitar el botón cuando el tipo no es exportable) debería ir antes que cualquier reporte nuevo.
2. **Dos implementaciones de "cuentas por pagar"**: el tipo `cuentas-por-pagar` del hub (exportable) convive con un endpoint suelto `GET /reportes/cuentas-por-pagar` (Program.cs:469) que usa `ICuentasPorPagar.ObtenerDeudas` directamente y genera un PDF distinto. Decidir cuál es la fuente de verdad y retirar la otra.
3. **Dos implementaciones de "compras"**: el tipo `compras` del hub convive con `GET /reportes/compras/pdf` (Program.cs:375), que sigue llamando al `ReportsMaster` legado del lado API (ver plan API, `## Estado actual auditado`). Mismo criterio: unificar.
4. **Permisos planos, no por reporte**: el nodo de menú "Módulo Reportes" (`MenuSeePos.cs:610-616`) es un único ítem sin submenú (`MODULO_REPORTES.MODULO_REPORTES`) — no hay forma hoy de que un rol vea unos tipos de reporte y otros no; la tabla de la sección siguiente ("Usuarios, decisiones y permisos") no se puede cumplir mientras esto no cambie. Depende del árbol `REPORTES.*` que el plan API debe construir primero (A4.1 de ese documento).
5. **Sin librería de gráficos real**: no hay Chart.js/ApexCharts/etc. en el proyecto. El ícono de "gráfico de pastel" en `AppGraficosReporte.razor` en realidad dibuja una segunda lista de barras horizontales — no hay pie/donut ni curva ABC/Pareto real, pese a que el tipo `inventario-abc` existe.
6. **`Views/Consignacion/Tablero.razor`** (dashboard de consignación: saldo, venta liquidada 30 días, rotación por artículo) está construido y funciona, pero **no está enlazado en el menú** — es invisible salvo que alguien conozca la URL `/consignment/dashboard`. Decidir si se enlaza tal cual o se absorbe dentro del dashboard ejecutivo de la fase W3/nueva fase BI.
7. **Los 8 tipos fuera del whitelist de exportación tampoco tienen agrupamiento de gráfico dedicado** en `ResumenGraficasReporteOperacion.cs` — caen a un agrupamiento genérico "por Estado" que probablemente no representa nada útil para, por ejemplo, `clientes` o `rentabilidad`.

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

**Estado (14 sep 2026): esta tabla es la meta, no la realidad actual.** Hoy el módulo entero cuelga de un único permiso plano (ver `## Estado actual auditado`, punto 4) — cualquier rol con acceso a "Módulo Reportes" ve y exporta los 19 tipos, incluidos rentabilidad/márgenes y comisiones de todos los agentes. Esta tabla no se puede aplicar en la UI hasta que el API tenga el árbol `REPORTES.*` (plan API, fase A4.1) y el sitio convierta el nodo plano del menú en el submenú jerárquico que ya proponía la sección "Página inicial" más abajo.

### Matriz reporte × rol (detalle sobre la tabla anterior)

Referencia directa entre los tipos ya construidos (o por construir) y los roles pedidos, para guiar el seed de permisos cuando se construya el árbol `REPORTES.*`. "Propio" significa acotado a los datos del usuario (su caja, su cartera de clientes, sus documentos), no el consolidado de la empresa.

| Tipo de reporte | Admin | Gerente | Cajero | Contador | Auxiliar | Ctrl. inventario | Jefe facturación | Personal facturación | Ctrl./trazab. inventario |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `ventas`, `ventas-detalle` | ✔ | ✔ | — | ✔ | ✔ | — | ✔ | Propio | — |
| `clientes` (comportamiento) | ✔ | ✔ | — | — | ✔ | — | ✔ | — | — |
| `rentabilidad` | ✔ | ✔ | — | — | — | — | — | — | — |
| `ventas-compras` (comparativo) | ✔ | ✔ | — | ✔ | — | — | — | — | — |
| `comisiones` | ✔ | ✔ | — | — | — | — | ✔ | Propio | — |
| `cuentas-por-cobrar`, `recuperacion-cxc`, estado de cuenta | ✔ | ✔ | — | ✔ | ✔ | — | — | — | — |
| `cuentas-por-pagar`, `compras` | ✔ | ✔ | — | ✔ | ✔ | — | — | — | — |
| Gastos *(nuevo)* | ✔ | ✔ | — | ✔ | ✔ | — | — | — | — |
| `caja`, `arqueos-cierres`, `depositos` | ✔ | ✔ | Propio | ✔ | — | — | — | — | — |
| `inventario`, `inventario-abc`, `rotacion-inventario` | ✔ | ✔ | — | — | — | ✔ | — | — | ✔ |
| `lotes`, `trazabilidad` (kardex) | ✔ | ✔ | — | — | — | ✔ | — | — | ✔ |
| Mermas/quiebres *(nuevo)* | ✔ | ✔ | — | — | — | ✔ | — | — | ✔ |
| `bonificaciones` | ✔ | ✔ | — | — | — | — | ✔ | — | — |
| CABYS *(nuevo)* | ✔ | ✔ | — | ✔ | — | — | ✔ | — | — |
| Apartados/Préstamos *(nuevo)* | ✔ | ✔ | — | ✔ | ✔ | — | — | — | — |
| Empaquetado/Maquila *(nuevo)* | ✔ | ✔ | — | — | — | ✔ | — | — | ✔ |
| `auditoria` / reimpresiones | ✔ | — | — | ✔ | — | — | ✔ | — | — |
| KPI ruta/agente *(nuevo)* | ✔ | ✔ | — | — | — | — | ✔ | — | — |
| Dashboard BI/gerencial *(nuevo)* | ✔ | ✔ | — | — | — | — | — | — | — |

## Catálogo priorizado

### Fase uno datos disponibles y de mayor valor — ✅ construida, con deuda (ver `## Estado actual auditado`)

1. **Ventas y facturación:** ventas netas por fecha, cliente, vendedor, familia, proveedor o artículo; detalle de documentos; contado/crédito; descuentos; devoluciones; notas de crédito; ventas por hora; top de clientes, artículos y servicios. — ✅ `ventas`/`ventas-detalle`/`clientes` construidos; falta confirmar que cubren explícitamente las 10 modalidades del catálogo original (contado/crédito como filtro, ventas entre horas) y ampliar exportación (bug #1 de arriba).
2. **Cuentas por cobrar:** antigüedad 1 a 30, 31 a 60, 61 a 90 y más de 90 días; estado de cuenta; facturas pendientes; aplicaciones y recibos; recuperación por periodo; DSO cuando la definición de fecha de vencimiento esté validada. — ✅ construido (`cuentas-por-cobrar`, `EstadoCuenta.razor`), con DSO ya calculado; falta el ajuste de cubetas a 1-30/31-60/61-90/+90 (hoy es por vencer/1-30/31-60/+60, lado API).
3. **Caja y depósitos:** aperturas, arqueos, cierres, diferencias, movimientos; predepósitos; depósitos y cheques por estado de confirmación. — ✅ construido (`caja`, `arqueos-cierres`, `depositos`, pantallas operativas de Caja).
4. **Compras y cuentas por pagar:** detalle de compras, proveedor, impuesto, descuentos, vencimiento, saldo y pagos. — ✅ construido pero **duplicado** (bugs #2 y #3 de arriba) — resolver antes de sumar reportes nuevos de este dominio (Gastos).
5. **Inventario y lotes:** existencia y valoración, kardex, lotes por vencimiento, artículos bajo mínimo, ajuste de inventario y trazabilidad venta-lote-compra cuando exista el vínculo de lote. — ✅ construido (`inventario`, `lotes`, `trazabilidad`); "artículos bajo mínimo" vive aparte en `Modulo.razor` → "Alertas operativas", no integrado al hub de reportes.
6. **Bonificaciones, devoluciones y auditoría:** bonificaciones entregadas, devoluciones de venta y compra, bitácora de cambios, reimpresiones y cambios de estado de cotizaciones. — ⚠️ parcial: `bonificaciones` y `auditoria` existen, pero "reimpresiones" específicamente no se encontró en ningún lado del código — verificar si las reimpresiones de comprobantes quedan registradas en `Bitacoras` (fuente de `auditoria`) o hay que agregar el evento.

### Reportes nuevos identificados en la relectura de fuentes (no estaban en el catálogo original de este plan)

Mapeados 1:1 con la sección "Lo que sigue sin construirse" del plan API — depende de que el API los construya primero (A4.3–A4.7 de ese documento):

- **Reporte de Gastos** (catálogo N°3) — pantalla nueva en el dominio "Compras" del hub, misma UX que `compras`.
- **Cumplimiento CABYS** (submenú Ventas N°9) — pantalla nueva, con export.
- **Apartados y Préstamos** (catálogo N°7) — pantalla nueva en el dominio "Ventas y cartera".
- **Empaquetado/Maquila** (catálogo N°13) — pantalla nueva en el dominio "Inventario".
- **KPI por ruta/agente** (anexo "Comportamiento del Agente") — pantalla nueva, posiblemente junto a `Views/Ventas/Comisiones.razor` ya que comparte `IdRutaComercial`.
- **Mermas consolidadas** — evaluar si se construye como reporte nuevo o se enriquece "Alertas operativas" (`Modulo.razor`) con filtros de fecha y exportación.

Del anexo "extras" (notas de seguimiento comercial, boletas de cambio, validación de depósito acreditado): son **funcionalidad nueva, no reportes sobre datos existentes** — requieren decisión de Producto sobre alcance antes de diseñar pantalla (ver API, punto 10 de "Lo que sigue sin construirse", y `## Decisiones que debe aprobar Producto` al final de este documento).

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

**Estado (14 sep 2026):** `/moduloReportes` ya dejó de ser un reporte único de compras (eso está cumplido), pero terminó como pestañas dentro de una sola página (`Views/Reportes/Compras.razor`, arreglo estático `Grupos`) en vez de tarjetas por dominio con permiso propio y "última actualización" — la intención de este párrafo sigue vigente como mejora pendiente, no como algo ya resuelto. Requiere el árbol de permisos `REPORTES.*` primero (no tiene sentido ocultar tarjetas si el permiso de fondo sigue siendo uno solo).

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

### W0 contrato y navegación — ⚠️ parcial

- Crear el árbol de funciones bajo `REPORTES` con categorías y reportes hoja. Regenerar la semilla de seguridad desde `MenuSeePos.cs` y revisar permisos de rol. — ❌ no hecho, es el bug/deuda #4 de arriba.
- Convertir `/moduloReportes` en catálogo y conservar el reporte de compras como una hoja concreta, no como página raíz. — ⚠️ hecho a medias: ya no es solo compras, pero es pestañas en una página, no tarjetas navegables por dominio.
- Definir el componente compartido de filtros, estado de carga, formato de totales, explicación de métricas y exportación. — ✅ hecho (`AppFiltros`, `AppRejilla`, `GeneradorReporteOperacion`).

### W1 finanzas operativas — ✅ hecho, con deuda

- Construir Compras, CxP, Antigüedad CxC, Estado de cuenta, Facturas pendientes y Recuperación de cartera. — ✅ construido; deuda: duplicidad de Compras/CxP (bugs #2, #3) y cubetas de antigüedad.
- Construir Caja y depósitos, con separación visual de pendientes bancarios. — ✅ construido.
- Reusar los PDF de recibo/factura solo para documentos; los reportes consolidados se exportan desde el nuevo endpoint de reporte. — ✅ cumplido (`GeneradorReporteOperacion` separado de la impresión fiscal `QuestPDF`).

### W2 ventas e inventario — ✅ hecho, con deuda

- Construir Ventas netas/detalle, descuentos/devoluciones/notas y top de clientes/artículos. — ✅ construido.
- Construir Kardex, existencias, lotes/vencimientos y trazabilidad por artículo. — ✅ construido.
- Incorporar gráficos de tendencia únicamente sobre los agregados que entregue el API. — ⚠️ hecho pero limitado: solo barras horizontales dibujadas a mano, sin librería, y 8 de los 19 tipos caen a un agrupamiento genérico sin sentido de dominio (bug #7).

### Fase de estabilización (nueva, antes de W3) — ⏳ sin iniciar

Resolver la deuda de W0–W2 antes de sumar los reportes nuevos o pasar a W3, porque varios de esos huecos son bugs visibles al usuario, no solo deuda interna:

1. Corregir el whitelist de exportación (`Program.cs:430`) para los 8 tipos que hoy dan 404, o quitar el botón cuando el tipo no exporta.
2. Unificar Compras (hub vs. `/reportes/compras/pdf` legado) y CxP (hub vs. `/reportes/cuentas-por-pagar` suelto) en una sola implementación cada uno.
3. Construir el árbol `REPORTES.*` (API) + convertir el nodo plano del menú en submenú filtrado por permiso (web) — depende del plan API, A4.1.
4. Ajustar cubetas de antigüedad de CxC a 1-30/31-60/61-90/+90.
5. Decidir el destino de `Views/Consignacion/Tablero.razor` (enlazar en el menú o absorber en el dashboard de W3-BI).
6. Adoptar una librería de gráficos real (Chart.js vía CDN u otra ya evaluada por el equipo) para reemplazar el SVG a mano, mínimo en los tipos que sí necesitan pie/curva (ABC/Pareto, comparativos).
7. Construir las pantallas de los reportes nuevos del catálogo (Gastos, CABYS, Apartados/Préstamos, Empaquetado/Maquila, KPI ruta/agente, Mermas) una vez el API los exponga (plan API, A4.3–A4.7).

### W3 indicadores avanzados — ⏳ sin iniciar (vigente tal cual)

- Activar ABC, DSI, margen y comportamiento de clientes solo al aprobar fórmulas y fuentes.
- Agregar rutas, cuotas, visitas, flete y OTIF después de que sus módulos transaccionales existan y pasen pruebas de calidad de datos.
- Nuevo en esta revisión: evaluar aquí el **dashboard ejecutivo/BI** pedido en `Reportes_ERP_Distribuidora_Consolidado_Optimizado vf.docx` (sección 7) — decidir si se construye un panel propio (reutilizando/absorbiendo `Tablero.razor` de consignación como precedente) o si el entregable es un feed/export pensado para Power BI/Tableau en vez de un dashboard propio.

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

**Estado (14 sep 2026):** los primeros tres puntos y el último están cumplidos. "Toda exportación... respeta autorización" **no se cumple** (no hay autorización por tipo de reporte) y además hay 8 tipos donde la exportación ni siquiera funciona (bug #1). Estos dos son los criterios que faltan para cerrar fase uno de verdad — ver "Fase de estabilización" arriba.

## Decisiones que debe aprobar Producto antes de fase dos

1. Fórmula oficial de venta neta, costo, margen, DSO, DSI, cartera vencida y devolución.
2. Alcance de sucursal y empresa para cada rol.
3. Si una bonificación reduce margen y cómo se atribuye a cliente, agente y proveedor.
4. Modelo operativo de rutas, visitas, cuota, entrega, flete y ventas perdidas.
5. Retención histórica, horario de corte y límite permitido de exportación.
6. **(nuevo)** Alcance de las funcionalidades del anexo "extras" de `Reportes_ERP_Distribuidora_Consolidado_Optimizado vf.docx`: notas de seguimiento/bitácora comercial de visitas, boletas de cambio por producto vencido/dañado, y validación de que un pago por depósito fue efectivamente acreditado. Son funcionalidad nueva (no solo reportes sobre datos existentes) — definir si entran a este módulo, a otro, o quedan fuera de alcance antes de diseñarlas.
7. **(nuevo)** Si se retira `ReportsMaster`/`ReportesController` (API) y las rutas web duplicadas `/reportes/compras/pdf` y `/reportes/cuentas-por-pagar` (ver "Bugs y deuda concretos" arriba), o si se documenta formalmente por qué coexisten dos implementaciones.
