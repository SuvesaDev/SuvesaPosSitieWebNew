# Plan de UAT para certificar el sitio completo (todos los módulos y Contabilidad)

Documento de trabajo de la rama `feature/modulo-contabilidad`. La misma copia vive en:

- `DevSuvesaPosWeb/docs/PLAN_UAT_CERTIFICACION_SITIO.md`
- `SuvesaPosSitieWebNew/docs/PLAN_UAT_CERTIFICACION_SITIO.md`

Este plan dice **qué se prueba, en qué orden, con qué datos, quién lo prueba y cuándo se da por certificado**. La evidencia de cada corrida se registra en `docs/UAT_CONTABILIDAD_EN_VIVO.md` (Contabilidad) y en `docs/UAT_EVIDENCIA_<fecha>.md` (el resto), con el mismo formato de tabla.

## 1. Qué significa «certificado»

El sitio se certifica cuando se cumplen **las cinco** condiciones:

1. **Cobertura:** cada una de las ~150 funciones del menú (`MenuSeePos`) tiene al menos una corrida «Pasa» en el entorno UAT con datos reales, y cada flujo de negocio de la sección 6 pasó de punta a punta.
2. **Contabilidad cuadrada:** cada documento que debe llegar al mayor llegó con la póliza esperada (sección 7), el pre-cierre del período de prueba no tiene documentos sin evento ni diferencias de conciliación, y el período se cerró.
3. **Seguridad:** la matriz de roles (sección 8) se cumple: lo permitido funciona y lo no permitido está denegado en pantalla **y** en el API.
4. **Defectos:** cero abiertos de severidad 1 y 2; los de severidad 3 y 4 tienen dueño y fecha aceptados por quien certifica.
5. **Automatizado verde:** `dotnet test` del API y del Web en verde, salvo la prueba conocida de consignación mientras no se actualice (ver 10.3).

Quien certifica firma por área (sección 11). Un hallazgo reabierto cancela la certificación del área hasta que se re-pruebe.

## 2. Alcance

| Módulo del menú | Funciones | Observación |
|---|---|---|
| Inicio | Clientes, Inventarios, Facturación, Bandeja de documentos, Envíos de correo, Alertas | Facturación es el flujo más crítico del sitio |
| Caja | Apertura, Arqueo, Consulta de arqueos, Conciliación, Cobrar, Depósitos (pre-depósito, generar, consultar) | Genera fondo de caja y diferencia de caja contables |
| Compras | Compra, Proveedores, Documentos aceptados, Cuentas por pagar, Pedidos, Toma (pretoma y física), Traslado entre bodegas, Orden manual, Importaciones, Devoluciones compra, Abono Pagar, Recibos de pago | |
| Producción | Calculadora, Bitácora | |
| Bodega | Cola de alistamiento, Recibo de mercadería, Empaque, Despacho, Rutas de entrega | |
| Consignación | Tablero, Bodegas, Ajuste, Boletas, Inventario físico, Kardex, Facturación, Estados | |
| Ventas | Abono Cobrar, Recibos y fallidas, Perfiles de emisión, Comisiones, Trámite de cobro, Devoluciones | |
| Presupuestos | Proformas, Seguimiento de cotizaciones | Sin efecto contable por diseño |
| Cobranza | Seguimiento, Dashboard, Gerencial, Autorizaciones de crédito | Sin recorrer en navegador con datos reales |
| Parámetros y Catálogos | Usuarios, Roles, Sucursales, Series, Plantillas de impresión, Correo, Configuración, Emisores, Bodegas, Áreas, Cajas, 17 catálogos | Base de todo lo demás |
| Módulo Inventario | Consulta Estados Albaranes, Inventario QR | Requiere datos de Qvet |
| Reportes | 31 reportes | Se prueban contra los documentos de la corrida, no contra datos sueltos |
| **Contabilidad** | 34 funciones (configuración, catálogo, plantillas, bandeja, diario, mayor, auxiliares, cierre, cierre anual, aprobaciones, tipos de cambio, revaluación, procesadores, gastos, diferencias de caja, deducibilidad, reclasificación, conciliación, saldos iniciales, puesta en marcha y 6 estados financieros) | Se prueba encadenada a los documentos de los demás módulos |

Fuera de alcance de esta certificación: las pantallas que son mockup en el sistema original (ver `CLAUDE.md` del sitio, «Fuera de alcance»), el rendimiento bajo carga y la aceptación contra el ambiente de producción de Hacienda.

## 3. Estrategia: cuatro capas

| Capa | Qué es | Quién | Cuándo |
|---|---|---|---|
| **A. Automatizada** | Suites del API (`ApiSuvesaPos.SmokeTests`, ~1.680 pruebas) y del Web (240 unitarias + E2E con Playwright) | Desarrollo | Antes de cada corrida UAT y al cerrar cada fase |
| **B. Flujos de negocio de punta a punta** | Los 18 flujos de la sección 6, ejecutados en el sitio con datos reales, con su póliza verificada | Usuarios clave de operación + contador | Fases 4 a 8 |
| **C. Recorrido por función** | Cada función del menú: abre, lista, crea/edita/anula lo que le toca, filtra, exporta, imprime, respeta permisos | Un probador por módulo | Fases 1 a 3 y 9 |
| **D. Cierre contable** | Pre-cierre, cierre mensual, conciliación contra el mayor, estados financieros, cierre anual en un libro de prueba | Contador | Fase 10 |

Regla: **la capa C no sustituye a la B.** Una pantalla puede abrir bien y aun así no contabilizar; por eso cada flujo B termina mirando la bandeja y la póliza.

## 4. Entorno, datos y herramientas

### 4.1 Entorno

- Un entorno UAT estable: Blazor `http://localhost:5199` + API `http://localhost:5000` contra la base `CostaPetsPOSDev` (o una copia dedicada). **No se prueba mientras alguien despliega.** Cada corrida anota el commit del API y del Web.
- Antes de empezar: migraciones aplicadas, API reiniciado con el binario del commit a probar, catálogo contable sincronizado.
- Hacienda: sandbox. Las aceptaciones y rechazos reales se registran en el hallazgo, no se simulan.
- Navegador integrado a **1440×900 como mínimo** y forzar nivel C; por debajo de 1280 px la rejilla se oculta y la prueba parece fallar sin ser un defecto.
- Cada corrida usa un **prefijo único** (`UAT-<módulo>-<MMDD><letra>`) en referencias, para encontrar sus documentos y no repetirlos.

### 4.2 Datos maestros mínimos (los crea la Fase 0)

- 1 emisor con 2 sucursales, una con contabilidad encendida y otra apagada (para probar que la apagada no genera pólizas).
- Bodegas: central, de consignación del cliente y una de otra sucursal.
- Artículos de tres tipos de inventario (mercadería, producto terminado, materia prima), uno por peso, uno con lote y vencimiento, uno con bonificación.
- Clientes: contado, crédito con límite, crédito vencido (bloqueado), con exoneración, con bonificación, extranjero en dólares.
- Proveedores: con y sin XML, en colones y dólares.
- Cajas: 2, con denominaciones en colones y dólares.
- Bancos: al menos 2, **cada uno mapeado a su cuenta** en «Cuentas por banco».
- Usuarios: uno por cada rol de la matriz de la sección 8.

### 4.3 Preparación contable (una sola vez, del equipo)

Estos pasos no son código y bloquean el cierre si no se hacen:

1. Aplicar la migración `ContabilidadFechaPuestaEnMarcha` y reiniciar el API.
2. Fijar la fecha de puesta en marcha (Bandeja → «Documentos sin contabilizar» → «Fecha de puesta en marcha») en **2026-09-30**.
3. «Actualizar catálogo» y activar todas las plantillas con vigencia desde el inicio del primer período.
4. Mapear cada banco a su cuenta.
5. Cargar los saldos iniciales al 2026-09-30.
6. Reclasificar los saldos a favor de cuentas por cobrar a `2.1.3.4`.
7. Corregir el tipo del artículo 002 (hoy cae en producto terminado) y reclasificar.
8. Anular el recibo duplicado `UAT-AB-0511` (Compras #2).

## 5. Fases y orden de ejecución

El orden respeta las dependencias: no se factura sin catálogos, no se cierra sin documentos.

| Fase | Contenido | Entrada | Salida |
|---|---|---|---|
| **0. Preparación** | Entorno, datos maestros, usuarios, preparación contable (4.3), suites automatizadas en verde | Commit a probar | Entorno firmado por desarrollo |
| **1. Seguridad y parámetros** | Login, selección de sucursal, usuarios, perfiles, roles y matriz de permisos, sucursales, emisores, series, cajas, correo, configuración, plantillas de impresión | Fase 0 | Cada rol entra y ve solo lo suyo |
| **2. Catálogos** | Los 17 catálogos: alta, edición, baja lógica, uso en otras pantallas | Fase 1 | Catálogos listos para uso |
| **3. Maestros** | Clientes (con bonificación y crédito), proveedores, inventarios (lotes, relacionados, bonificación), bodegas | Fase 2 | Datos maestros de la sección 4.2 |
| **4. Compras e inventario** | Flujos F1 a F5 | Fase 3 | Existencias y cuentas por pagar con póliza |
| **5. Ventas, facturación y caja** | Flujos F6 a F11 | Fases 3 y 4 | Ventas, cobros y cajas con póliza |
| **6. Crédito y cobranza** | Flujos F12 y F13 | Fase 5 | Cartera, abonos y gestión |
| **7. Bodega, consignación y producción** | Flujos F14 a F16 | Fases 4 y 5 | Alistamiento, consignación y producción |
| **8. Contabilidad encadenada** | Verificación póliza por póliza de todo lo anterior (sección 7), pólizas manuales, aprobaciones, reversos, gastos, diferencias, reclasificación | Fases 4 a 7 | Mayor y auxiliares cuadran |
| **9. Reportes y consultas** | Los 31 reportes y las consultas contra los documentos de las fases previas | Fase 8 | Cada reporte cuadra con su fuente |
| **10. Cierre y certificación** | Pre-cierre, cierre del período, estados financieros, cierre anual de prueba, regresión, re-prueba de defectos, firma | Fases 1 a 9 | Certificación o lista de pendientes |

Las fases 4, 5 y 7 pueden correrse en paralelo por distintos probadores si usan prefijos distintos y bodegas distintas.

## 6. Flujos de negocio de punta a punta (capa B)

Cada flujo termina con **tres comprobaciones**: el documento (estado, números, impresión/PDF), el efecto operativo (existencia, saldo, caja) y el efecto contable (evento en bandeja → póliza con las cuentas y montos esperados).

| Id | Flujo | Pasos clave | Variantes obligatorias |
|---|---|---|---|
| **F1** | Compra | Registrar con XML, sin XML, de contado y a crédito; mensaje receptor | Dólares; IVA sin XML (va a `6.1.6.12`); compra rechazada por Hacienda y forzada con motivo; fecha futura rechazada; devolución de compra |
| **F2** | Pago a proveedor | Abono Pagar sobre una o varias facturas; recibo de pago | Banco distinto por pago (cuenta correcta); doble clic no duplica; recibo sin número |
| **F3** | Toma física | Pretoma → toma → aprobar; anular | Faltante, sobrante (`4.2.1.2`), mixta, que cuadra (sin póliza) |
| **F4** | Traslado entre bodegas | Misma sucursal (solo operativo, sin póliza) y entre sucursales (con póliza) | Existencia negativa permitida/no |
| **F5** | Pedidos y orden de compra manual | Pedido a bodega, consulta, orden manual, importaciones | |
| **F6** | Proforma → preventa → factura | Proforma, seguimiento, conversión | Sin efecto contable hasta facturar |
| **F7** | Factura de contado (electrónica y tiquete) | Cobro 100 % en la misma emisión, vuelto, caja abierta obligatoria | Efectivo, tarjeta, depósito, mixto; bonificación; exoneración; precio por peso; dólares; interna sin valor tributario |
| **F8** | Factura a crédito | Límite, bloqueo por vencida, plazo | Cliente moroso rechazado |
| **F9** | Anulación y nota de crédito | Anulación de tiquete, NC parcial y total, aceptación de Hacienda | NC sin plantilla vigente (debe fallar claro); devolución de venta en caja |
| **F10** | Cobro y abono de clientes | Cobrar por ficha/cédula, Abono Cobrar, recibos, anticipos/entrega a cuenta | Cheque/depósito pendiente de confirmar (no contabiliza hasta confirmar); depósito no confirmado; abono legado |
| **F11** | Ciclo de caja completo | Apertura con fondo → ventas y cobros → arqueo → conciliación → pre-depósito → depósito → cierre | Cuadre; faltante bajo el tope (gasto); sobrante; faltante sobre ₡5.000 con rebajo por planilla; con castigo; anular el cierre |
| **F12** | Cobranza | Seguimiento, promesa de pago, bitácora, cierre automático al cobrar, dashboard, gerencial, autorizaciones de crédito | |
| **F13** | Comisiones | Ruta, agente, servicio al cliente, corte previsualizado, liquidación | Corte que repite rango (rechazado); movimientos que arrastra |
| **F14** | Bodega | Cola de alistamiento → empaque → despacho → rutas; recibo de mercadería | |
| **F15** | Consignación | Boleta de ingreso → conteo → prefactura → facturación de contado y de crédito → liquidación → estados | Lote y código correctos en la grilla; `Facturado` casa con el conteo; kardex |
| **F16** | Producción | Calculadora, conversión de lotes, bitácora | |
| **F17** | Fiscal V4.4 | Envío, aceptación, rechazo, reintento, correo del comprobante, bandeja de documentos | Rechazos -454/-476/-488 de bonificación ya corregidos: no deben reaparecer |
| **F18** | Reportes e impresión | PDF A4 y térmico de cada documento, plantillas, exportación CSV/PDF | |

## 7. Contabilidad: qué póliza se espera de cada documento

Se verifica en Bandeja (evento → `Contabilizado`) y en Diario (cuentas y montos). El detalle de cuentas por plantilla vive en `ApiSuvesaPos/docs/GUIA_ACTIVACION_CONTABILIDAD.md`.

| Documento | Evento | Póliza esperada (resumen) |
|---|---|---|
| Venta emitida (crédito o contado) | `VentaEmitida` | Cuenta por cobrar o caja contra ingreso e IVA; costo contra inventario por tipo |
| Cobro de contado / recibo | `CobroAplicado` (`…Usd`) | Caja, tarjeta o **el banco elegido** contra cuenta por cobrar |
| Nota de crédito | `NotaCreditoEmitida` | Reverso de venta y de costo |
| Compra | `CompraRegistrada` | Inventario e IVA acreditable (o no acreditable sin XML) contra cuenta por pagar |
| Pago a proveedor | `PagoProveedorAplicado` (`…Usd`) | Cuenta por pagar contra **banco de la cuenta elegida** |
| Toma física | `AjusteInventario` | Faltante a gasto; sobrante a `4.2.1.2`; mixta en una sola póliza; cuadre sin póliza |
| Depósito de caja confirmado | `DepositoConfirmado` | Banco contra caja; el no confirmado no genera nada |
| Apertura de caja | `AperturaCaja` | Débito `1.1.1.2` contra el origen del fondo |
| Cierre de caja | `DevolucionFondoCaja` y `DiferenciaCaja` | Devolución por lo entregado; diferencia a gasto, cobro al cajero o ingreso; rebajo y castigo |
| Comisión | `ComisionLiquidada` | Gasto de comisión contra comisiones por pagar |
| Consignación facturada | `VentaEmitida` + `CobroAplicado` | Igual que una venta normal; el traslado de bodega misma sucursal no genera póliza |
| Gasto, procesador de pago, reclasificación, revaluación | sus eventos | Según la plantilla activa |
| Póliza manual | `AsientoManual` | Cuadrada, con sucursal, período abierto, cuenta de control pide contraseña; con maker-checker solo vía aprobación |
| Proforma / preventa | — | **Ninguna** (es lo esperado) |

### 7.1 Casos contables transversales

| Id | Caso | Resultado esperado |
|---|---|---|
| C1 | Sucursal con contabilidad apagada factura y cobra | Sin eventos, sin errores |
| C2 | Documento sin plantilla vigente en su fecha | Evento en `ErrorConfiguracion` con mensaje que dice desde cuándo rige la plantilla; tras corregir, «Reintentar» lo contabiliza |
| C3 | Evento en error cuyo documento cambió | «Regenerar» rearma el contenido y lo reencola (ventas, compras y notas de crédito) |
| C4 | Documento anterior a la puesta en marcha | No se pide como pendiente |
| C5 | Documentos sin contabilizar | Ve ventas, compras, notas, cobros, abonos de clientes, depósitos, tomas, pagos y fondo de caja; «Regularizar» crea solo ventas, compras y notas, por rango o por selección |
| C6 | Período cerrado | Documento con fecha en período cerrado no contabiliza y lo dice; «Abrir período» lo reabre con bitácora |
| C7 | Mes nuevo | Abre el período solo (con bitácora), dentro del mes en curso |
| C8 | Moneda extranjera | Cobro y pago en dólares con tasa del documento o BCCR; diferencial cambiario; revaluación |
| C9 | Reverso de póliza | Con motivo; maker-checker pide aprobación; el reverso también es auditable |
| C10 | Conciliación contra el mayor | Auxiliares CxC, CxP e Inventario vs mayor sin diferencias tras saldos iniciales |
| C11 | Pre-cierre | Sin documentos sin evento, sin eventos bloqueantes, sin diferencias; con alguno, **no deja cerrar** y dice por qué |
| C12 | Cierre anual (en libro de prueba) | Cierra cuentas nominales y traslada a utilidades |

## 8. Matriz de seguridad (capa C, Fase 1 y repaso en Fase 10)

Roles de prueba: **Super administrador**, **Administrador** (gestiona usuarios, lee la configuración de seguridad), **Cajero**, **Vendedor**, **Bodeguero**, **Comprador**, **Contador**, **Aprobador contable**, **Consulta**.

Para cada rol se prueba:

1. Solo ve en el menú lo que su rol permite; las pantallas no gobernadas se ocultan.
2. Cada acción (VER, CREAR, EDITAR, BORRAR, ACTIVAR, EXPORTAR, IMPRIMIR) funciona si está concedida y devuelve 403/oculta el botón si no. **Se prueba contra el API directamente además de la pantalla.**
3. Contabilidad: el contador crea y contabiliza; el aprobador aprueba pero **no** aprueba lo que él mismo solicitó; quien no tiene `ACTIVAR` en configuración del emisor no ve «Fecha de puesta en marcha».
4. Sesión: caducada → aviso y reingreso, sin error técnico; el token no se ve en el navegador.
5. Sucursal: cada usuario opera solo en su sucursal.

## 9. Casos por función (capa C): plantilla mínima

Para **cada función** del menú el probador comprueba, y anota «Pasa / Falla / No aplica»:

1. Abre sin error de consola y sin quedar en blanco (a 1440 px y a tableta cuando el nivel B/A aplica).
2. Lista y filtra; paginación y orden; búsqueda con tildes.
3. Crear con datos válidos; validaciones con datos inválidos (mensaje claro, no el error técnico del API).
4. Editar y cambiar de estado; baja lógica donde aplique.
5. Efectos secundarios: aparece en las pantallas relacionadas (catálogo → formulario que lo usa).
6. Exportar/imprimir (PDF, CSV) con los datos correctos y formato de importes (`₡1.234,56`, porcentajes, fechas).
7. Permisos según la matriz.
8. Texto de interfaz: sin código de expresión visible (`.ToString(`, `0:N2%`), sin identificadores internos donde debe verse código y lote.

Pantallas con atención especial por defectos pasados: Estados de consignación (fecha), Compra (porcentajes y vencimiento de lote), grillas de comisiones y de prefactura (código y lote), Arqueo (efectivo esperado), Conciliación de caja (cajas abiertas), diálogo de cierre de caja (contado, esperado, diferencia y destino), Catálogo de cuentas (deducible, cuentas por banco), Bandeja (regenerar, regularizar, fecha de puesta en marcha).

## 10. Defectos, evidencia y criterios

### 10.1 Severidad

| Sev. | Definición | Ejemplo | Plazo de decisión |
|---|---|---|---|
| 1 | Detiene la operación o corrompe datos/contabilidad | Documento fiscal mal emitido, póliza con monto incorrecto, doble cobro | Mismo día |
| 2 | Función principal falla, sin alternativa razonable | No se puede cobrar, la póliza manual no inserta | 2 días |
| 3 | Falla con alternativa, o error de presentación que confunde | Rejilla con id interno en vez de código | Antes de certificar o aceptado con fecha |
| 4 | Cosmético | Tilde, alineación | Backlog |

### 10.2 Registro de cada corrida

Una fila por caso en el documento de evidencia, con: id del caso, fecha, probador, commit del API y del Web, documento(s) generado(s) con su número, evento y póliza (módulo, id, estado), resultado y, si falla, el mensaje exacto del diálogo y del API. Los hallazgos se numeran de forma continua (siguiente: **27**) con causa y corrección cuando se resuelven. Una captura o el número de documento basta como evidencia; «se ve bien» no.

### 10.3 Antes de cada corrida (capa A)

```bash
dotnet test ApiSuvesaPos.SmokeTests          # API
dotnet test                                  # Web
```

Hoy el API tiene **1 prueba que ya fallaba y no es parte de estos cambios**: `ConsignacionOperacionControlTests.Liquidacion_PersisteEmisorDiferenciasYReciboOperativo` (lee texto fuente que ya no existe). Hay que actualizarla o borrarla **antes** de certificar, para que «automatizado verde» sea literal.

Limitación a tener presente: las pruebas del API usan InMemory, que **no valida llaves foráneas** (el hallazgo 26 pasó así). Lo que persiste en SQL Server solo se certifica en vivo.

### 10.4 Re-prueba

Todo defecto corregido se re-prueba por quien lo encontró, con el mismo caso, en el commit nuevo. Al cerrar cada fase se corre una regresión corta: F7, F11 y C11.

## 11. Responsables y firma

| Área | Prueba | Firma la certificación |
|---|---|---|
| Seguridad y parámetros | Administrador del sistema | Administrador del sistema |
| Compras, inventario, bodega, consignación, producción | Jefe de compras / bodega | Jefe de operaciones |
| Ventas, facturación, caja, cobranza, comisiones | Jefe de ventas y cajero líder | Jefe de ventas |
| Fiscal V4.4 | Responsable de facturación electrónica | Responsable fiscal |
| Contabilidad | Contador | Contador |
| Reportes | Quien consume cada reporte | Gerencia |
| Automatizado y entorno | Desarrollo | Desarrollo |

Los nombres se completan antes de la Fase 0.

## 12. Calendario sugerido

| Semana | Fases | Entregable |
|---|---|---|
| 1 | 0, 1, 2 | Entorno firmado; seguridad y catálogos |
| 2 | 3, 4 | Maestros; compras e inventario con póliza |
| 3 | 5, 6 | Ventas, caja, cobranza |
| 4 | 7, 8 | Bodega, consignación, producción; contabilidad encadenada |
| 5 | 9, 10 | Reportes; cierre del período; re-pruebas; firma |

Con probadores en paralelo en las fases 4, 5 y 7 el calendario baja a cuatro semanas. Cada fase incluye un día de corrección y re-prueba.

## 13. Riesgos conocidos

| Riesgo | Mitigación |
|---|---|
| Pasos operativos de 4.3 sin hacer | Se verifican en la Fase 0 y bloquean el arranque de la 8 |
| El navegador integrado cae en nivel B (tableta) y oculta la rejilla | Forzar 1440 px y nivel C; anotarlo como nota operativa, no como defecto |
| Cobros con error transitorio de SQL (visto en la venta #74) | Tras cada cobro verificar que exista su `CobroAplicado`; si falta, es hallazgo |
| Cajas abiertas antes de activar `AperturaCaja` no regularizan | Cerrarlas antes de la corrida y abrir cajas nuevas |
| Cobros sin recibo (venta de contado con el flag de cobro apagado) no tienen evento definido | Probar con el flag en `true`; el camino con `false` se decide con el contador |
| Dependencia de Hacienda sandbox y de datos de Qvet | Los casos F17 y Módulo Inventario se marcan «bloqueado por entorno» si el servicio no responde, sin fallar el resto |
| Clientes/DTOs escritos a mano hasta regenerar contratos NSwag | Regenerar con `./tools/actualizar-contratos.sh` y volver a correr la capa A antes de la Fase 10 |
| Mocks/pantallas fuera de alcance confunden al probador | Lista de exclusiones en la sección 2 entregada al inicio |

## 14. Entregables de la certificación

1. Este plan con los responsables completados.
2. Evidencia de cada fase (`UAT_CONTABILIDAD_EN_VIVO.md` y `UAT_EVIDENCIA_<fecha>.md`).
3. Lista de defectos con severidad, causa, commit de corrección y re-prueba.
4. Resultado del pre-cierre y del cierre del período, y los estados financieros del período.
5. Resultado de `dotnet test` del API y del Web en el commit certificado.
6. Acta de certificación firmada por área, con el commit del API y del Web certificados.
