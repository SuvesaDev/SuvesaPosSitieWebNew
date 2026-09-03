# Abono Cobrar — cobro de preventas pendientes (Sitio Web)

> Pantalla `/sales/collect` ("Abono Cobrar", `VENTAS.ABONO_COBRAR`). El cajero
> cobra las **preventas pendientes de pago** de un cliente (una o varias), contra
> su **caja abierta** y con varias **formas de pago**; al cobrar cada preventa se
> **factura**, se **pregunta si imprimir** (plantilla ligada a la serie) y se
> **emite a Hacienda**. Los documentos salen en la **Bandeja de documentos**.

---

## 1. De dónde venimos

| | React (`FrontEndPos2650App`) | Blazor (antes de esta entrega) |
|---|---|---|
| `/sales/collect` | `components/Collect/*` — "Recibo de Dinero": abono a **cuentas por cobrar** (crédito). No factura ni va a Hacienda. | `Views/Ventas/CuentasPorCobrar.razor` — **solo lista de lectura** (Cliente/Cédula/Factura/Fecha/Monto/Saldo). |
| Preventa → factura | `components/Charge/*` — cobra 1 preventa por ficha, formas de pago, y abre modal de tiquete/recibo. | `Views/Ventas/Cobrar.razor` (`/initial/charge`) — cobra 1 preventa, formas de pago contra caja, `FacturarPreventa`; sin impresión ni llamada explícita a Hacienda. |

## 2. Requerimiento

1. Cliente con 1+ preventas pendientes → **elegir cuáles cancelar**.
2. **Formas de pago** + **apertura de caja** del cajero.
3. Tras cobrar: **preguntar si imprimir** → imprimir según la **plantilla ligada a
   la serie**.
4. Tras cobrar: **enviar a Hacienda** → aparecer en la **Bandeja de documentos**.

## 3. Diseño

### 3.1 API (repo `DevSuvesaPosWeb`, rama `feature/bonificaciones`)
- **`POST /venta/PreventasPendientesPorCliente?codCliente=`** → `List<PreventaResumenDTO>`
  (id, consecutivo, fecha, moneda, totales, `IdSerie`, `Tipo`,
  `TipoFacturaDescripcion`, **`CodigoFe` de la serie** — `01`/`04`/null —,
  `EsCredito`, `EmisionV44Habilitada`, `Ficha`).
- El resto ya existía:
  - `POST /Cobros/InsertarCobro` (formas de pago contra `Numapertura`).
  - `POST /venta/PreventaFacturada` (marca `EsPreventa=false`, libera reservas,
    descuenta inventario).
  - `POST api/comprobantes-electronicos/v44/pos/ventas/{id}/facturas/emitir` y
    `.../tiquetes/emitir` — reserva numeración + firma + envía a Hacienda de forma
    **síncrona**. Si la serie tiene `EmisionV44Habilitada` pero no se llama, el
    `ReservaAutomaticaV44HostedService` lo toma solo (~30 s).
  - La Bandeja (`INICIO.DOCUMENTOS_EMITIDOS`) ya lee `Venta` con el estado fiscal
    proyectado.

### 3.2 Sitio (rama `feature/ola-0-cimientos`)
- Proxy `IAbonoCobrarPreventas` (`: ProxyBase`, cliente `SeePosApi`):
  `PreventasPendientes(codCliente)`, `EmitirFactura(idVenta)`,
  `EmitirTiquete(idVenta)`.
- `Views/Ventas/CuentasPorCobrar.razor(.cs)` reescrita:
  1. **Desbloqueo + caja**: clave interna → `ValidarClaveInterna` →
     `CajerosConCajaAbierta` → `numApertura` (portado de `Cobrar.razor`).
  2. **Buscar cliente** por cédula → `CodigoClientePorCedula` →
     `PreventasPendientes`.
  3. **Tabla de preventas con checkbox** (+ "todas") y subtotal seleccionado.
  4. **Formas de pago** (de `FormasPago(codCliente)`) para el **total
     seleccionado**; cálculo de entregado/cambio.
  5. **Cobrar y facturar**: reparto **en cascada** del pago entre las preventas
     por orden de fecha; por cada una: `Cobrar` → `FacturarPreventa` →
     `EmitirFactura`/`EmitirTiquete` si la serie es V4.4 (`01`/`04`), si no
     "Automático (worker)".
  6. **¿Imprimir?** → abre `/documentos/{slug}/{id}/pdf` por documento
     (`factura-electronica` / `tiquete-electronico` según `CodigoFe`), que usa la
     plantilla resuelta por serie.
  7. Panel de **Resultado** con estado por documento + enlace a la Bandeja.

## 4. Vacíos / decisiones
- **Reparto multi-documento**: el pago se ingresa una vez para el total
  seleccionado y se reparte en cascada (la última preventa absorbe el vuelto del
  efectivo). Alternativa (no elegida): un pago por documento.
- **Impresión inmediata vs. clave**: si la emisión es síncrona, el PDF ya sale
  con consecutivo y clave. Si la serie usa el worker, el PDF inmediato sale sin
  clave; se reimprime desde la Bandeja cuando Hacienda acepta.
- **"Recibo de Dinero" (abono a CxC de crédito)** del React `Collect` queda
  **fuera** de esta pantalla; si se necesita, va aparte (`AbonoCobrar` clásico
  con `Abonoccobrar`).
- La emisión de **tiquete** exige que la venta tenga formas de pago registradas
  (regla del `ReservaAutomaticaV44HostedService`); al cobrar antes de emitir, se
  cumple.

## 5. Checklist
- [x] API: `PreventasPendientesPorCliente` + DTO + interfaces + delegación BL.
- [x] Web: proxy `IAbonoCobrarPreventas` + DTOs + registro en `Program.cs`.
- [x] Web: `/sales/collect` reescrita (caja, selección, pago en cascada,
      facturar, emitir, imprimir, resultado).
- [ ] Pruebas manuales: preventa contado en serie V4.4 → factura + Hacienda +
      PDF; varias preventas mixtas; serie sin V4.4 (worker).
- [ ] Afinar: si el negocio quiere **un pago por documento** en vez de cascada.
