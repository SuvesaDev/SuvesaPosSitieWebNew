# Módulo de comisiones — guía de trabajo Web

## Auditoría contra `Especificacion_Funcional_Comisiones_TI (2).docx` (14 sep 2026)

Se comparó este diseño (ya construido) contra la especificación funcional oficial de
Administración/Ventas/SAC/TI. El detalle completo y la evidencia de código están en
`../../DevSuvesaPosWeb/ApiSuvesaPos/docs/PLAN_COMISIONES_ESPECIFICACION_TI_API.md` —
léase antes de tocar este módulo. Resumen de lo que cambia respecto al resto de este
documento (que sigue vigente salvo lo que se corrige acá):

- **CRÍTICO — el flujo "Funcionario SAC emite" no funcionaba — ✅ corregido 14 sep 2026.**
  `CargarRutasComerciales` filtraba `_rutasComerciales` a las rutas donde `usuarioValidado`
  **es agente** (`r.Agentes.Any(a => a.IdUsuario == usuarioValidado)`). Un Funcionario SAC
  nunca es agente de ninguna ruta, así que la lista siempre quedaba vacía y **el selector
  de ruta nunca aparecía para SAC**. Se corrigió consultando primero un endpoint nuevo del
  API (`GET api/mantenimientos/rutas/capacidad-comercial/{idUsuario}`, sin permiso de
  administración de seguridad) para saber si el usuario validado es Agente (rutas propias,
  solo lectura, como antes) o Funcionario SAC (todas las rutas activas de la sucursal,
  selección obligatoria, agente resultante no sustituible). El API tenía el mismo hueco en
  espejo (`ValidarRutaComercial` sin rama para `EsServicioAlCliente`) — se corrigió en el
  mismo cambio, ver plan API.
- **ALTO — no había pantalla para el "Módulo de Comisión" por ruta que pide la spec §2.3
  — ✅ corregido 14 sep 2026.** Confirmado por Producto que el porcentaje de ruta aplica
  tanto a Agente como a SAC (no exclusivo de Agente). API: campos
  `PorcentajeComisionAgente`/`PorcentajeComisionServicioCliente` en `RutaComercial` +
  motor de cálculo con precedencia absoluta de ruta sobre artículo (ver plan API,
  Hallazgo 2). Web: sección "Comisión por ruta" en el modal de edición de
  `Views/Parametros/Rutas.razor`.
- **MEDIO — conflicto de regla sobre "elegibilidad" vs. el punto 1 de "Regla confirmada"
  más abajo en este documento — ✅ resuelto 14 sep 2026, Opción B.** Se generó el
  movimiento en cuanto la venta es Hacienda-aceptada/Interna, con `EstadoCobro`
  ("Pendiente"/"Cancelada") separado, y el **corte** (no la generación) es el que exige
  cobro. Ver plan API, Hallazgo 3, para el detalle. El punto 1 de "Regla confirmada" más
  abajo queda desactualizado por este cambio — no aplica más.
- Confirmado sin cambios: cálculo, bonificaciones a $0, reversiones de devolución,
  consulta/detalle/resumen, cierre y exportación — todo lo demás en este documento sigue
  describiendo lo que ya existe y funciona.

## Propósito y alcance

Este documento es el contexto operativo para continuar el módulo de Comisiones en
el proyecto Blazor `SuvesaPosSitieWebNew`. Complementa la especificación funcional
de Administración; no sustituye las reglas fiscales ni autoriza cambios en el API
de facturación electrónica.

El módulo debe calcular y consultar comisiones de ventas sin alterar la emisión,
firma, envío a Hacienda, numeración, impuestos, bonificaciones ni los flujos
existentes de factura, tiquete, devolución, preventa y proforma. Una comisión solo
se muestra como generada o liquidable cuando la factura aceptada esté **totalmente
cancelada**, tanto si es de contado como si es de crédito.

## Estado previo que se debe preservar

- Los perfiles exponen las capacidades `EsAgente` y `EsServicioAlCliente`. El
  nombre legado `AgenteCostaPets` fue retirado del modelo funcional; no debe
  reintroducirse en Views, DTOs manuales ni reglas nuevas.
- Ya existe el catálogo de rutas comerciales. Una ruta puede tener varios agentes
  y exactamente un agente predeterminado para esa ruta.
- En Inventario ya existen los porcentajes por artículo para Agente y Servicio al
  Cliente. Sus controles deben ser porcentajes de 0 a 100 y estar disponibles para
  todos los centros, no solo CostaPets.
- Facturación ya transporta metadatos de ruta y agente. No se deben volver a usar
  los campos globales/legados de agente ni permitir que el navegador determine un
  agente que el servidor no haya validado.

## Flujo de facturación que debe quedar en la interfaz

| Usuario que emite | Ruta | Agente |
| --- | --- | --- |
| `EsAgente` | Se resuelve desde su ruta predeterminada de emisor. Es solo lectura. | Se resuelve automáticamente. Es solo lectura. |
| `EsServicioAlCliente` | Obligatoria; la persona la selecciona. | Se carga automáticamente desde el agente predeterminado de la ruta. Es solo lectura. |
| Ninguna capacidad | Sin configuración de comisión salvo decisión posterior de negocio. | No mostrar selector manual. |

No se debe dejar en la pantalla un combo libre de agentes. Si el emisor agente
puede pertenecer a varias rutas, el modelo debe tener una **ruta predeterminada por
agente y sucursal** (`EsRutaPredeterminada`) o se debe bloquear la emisión con un
mensaje claro. El agente predeterminado de una ruta no resuelve esa ambigüedad.

Si una persona tiene ambos perfiles, aplicar primero el flujo `EsAgente`, salvo que
negocio defina expresamente otra prioridad.

## Pantallas a construir o extender

1. **Inventario**: mantener una pestaña “Comisiones” con “Porcentaje de comisión
   de agentes” y “Porcentaje de comisión de servicio al cliente”; indicar rango y
   que un valor cero significa que ese artículo no genera comisión para ese rol.
2. **Perfiles/usuarios**: mostrar las capacidades heredadas del perfil, sin una
   copia editable independiente por usuario.
3. **Rutas**: además de los agentes por ruta y su agente predeterminado, permitir
   configurar la ruta predeterminada de cada agente cuando el API lo exponga.
4. **Facturación**: presentar ruta y agente resultantes como información de
   contexto; validar antes de emitir y mostrar por qué no se puede continuar.
5. **Comisiones**: nueva consulta con filtros de período, sucursal, ruta, agente,
   funcionario SAC, estado fiscal, estado de cobro y estado de liquidación. Debe
   ofrecer detalle auditable por documento/línea y resumen por beneficiario, sin
   recalcular ni editar movimientos históricos desde la pantalla.
6. **Cierre de período**: habilitar solo a permisos explícitos; mostrar conteos,
   importes y advertencias antes de cerrar. Exportaciones CSV/XLSX deben salir del
   servidor con el mismo conjunto de filtros y un identificador de cierre.

## Contrato que la interfaz debe esperar del API

El API es la fuente de verdad. La Web solo presenta datos y envía la intención del
usuario. Debe recibir movimientos de comisión inmutables con, como mínimo:

- documento, línea, fecha, sucursal, ruta, artículo y tipo de documento;
- agente, funcionario SAC y sus porcentajes aplicados;
- base sin IVA, descuento, cantidad e importe calculado;
- estados fiscal, de cobro y de liquidación; y la referencia de una reversión;
- instantáneas de texto y porcentajes para que los históricos no cambien si se
  modifica después una ruta, perfil, usuario o artículo.

La consulta debe diferenciar al menos “pendiente de pago”, “cancelada elegible” y
“liquidada”. Una factura de crédito con abonos parciales permanece pendiente; una
factura de contado solo será elegible cuando su pago quede confirmado.

No modificar DTOs o clientes NSwag generados. Mientras el contrato no sea
regenerado, crear DTOs y proxies manuales aislados según el patrón del proyecto;
reemplazarlos al actualizar contratos.

## Reglas visuales y técnicas Web

- Usar `AppRejilla`, `AppFiltros`, `AppModal`, `AppCampo*`,
  `IServicioDialogos` e `IManejadorRespuestas`; no crear una tabla, modal o manejo
  de error paralelo.
- Mantener `decimal` para importes expuestos y no repetir fórmulas de negocio en
  Razor o JavaScript.
- Proteger rutas y acciones con los códigos de permiso del menú. Al agregar la
  pantalla, actualizar `MenuSeePos.cs`, ejecutar los scripts de códigos/semilla y
  actualizar las pruebas asociadas.
- Cualquier error al obtener o consultar comisiones debe informar al usuario, pero
  nunca impedir que la factura electrónica ya válida termine su flujo fiscal.
- Antes de cambios visuales globales, revisar `wwwroot/css/tema.css`: evitar
  selectores duplicados, bloques `@media` sin cerrar y reglas de pantallas locales
  que afecten las demás.

## Casos que deben probarse

- Factura de contado pagada y no pagada; factura de crédito con abonos parciales y
  totalmente cancelada; rechazo Hacienda, anulación y proforma/borrador sin
  comisión.
- Emisión por agente, por SAC y por usuario sin ambos perfiles.
- Artículo con porcentaje cero, descuento por línea, IVA y línea de regalo con
  precio cero.
- Devolución total y parcial: debe mostrarse la reversión asociada, nunca un nuevo
  movimiento positivo.
- Cambio posterior de artículo, usuario, perfil, ruta o porcentaje: el histórico
  no cambia.
- Corte repetido, reintento de persistencia y exportación: no deben duplicar
  movimientos ni totales.

## Regla confirmada y decisiones pendientes antes de habilitar producción

1. ~~Las facturas de crédito y contado solo generan comisión al estar totalmente
   canceladas.~~ **Superado 14 sep 2026** — Producto confirmó la lectura de
   `Especificacion_Funcional_Comisiones_TI (2).docx` §4.1: el movimiento se genera en
   cuanto la venta es Hacienda-aceptada/Interna (`EstadoCobro = "Pendiente"` si aún no está
   cobrada); es el **corte** el que exige `EstadoCobro == "Cancelada"`. Ver la auditoría al
   inicio de este documento y plan API, Hallazgo 3.
2. Confirmar la regla para un emisor `EsAgente` con varias rutas: se recomienda una
   única ruta predeterminada activa por sucursal.
3. Confirmar prioridad para usuarios con ambos perfiles; la recomendación actual es
   `EsAgente`.
4. Definir si un usuario sin esos perfiles puede emitir documentos sin comisión;
   recomendación: sí, sin movimientos de comisión.

## Secuencia de implementación

1. Acordar las cuatro decisiones pendientes y el contrato API.
2. Implementar y probar la persistencia/calculadora en API antes de exponer la UI.
3. Ajustar ruta/agente automático de Facturación contra ese contrato, sin tocar el
   pipeline de Hacienda.
4. Crear la consulta, detalle, exportación y cierre de Comisiones.
5. Ejecutar build, pruebas unitarias/E2E y una prueba manual con facturas y
   devoluciones controladas.
