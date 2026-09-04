# Matriz de pruebas: operación diaria de abarrotes y carnes

Esta matriz enlaza los casos automatizados del sitio y de la API con el flujo de
operación. Los importes se expresan en CRC y las cantidades de carne se manejan por
peso, para comprobar que no se pierdan decimales ni se redondee antes de aplicar el
impuesto.

| Flujo | Sitio web | API |
|---|---|---|
| Series | `ContratosOperacionDiariaTests.Series_CobroCreditoYOrdenCompra_ConservanSusComandosDeNegocio` verifica la serie operativa y la fiscal. | `ReglasOperacionDiariaTests.SeriesOperativas_NoMezclanLosConsecutivosDeLaOperacionDiaria` protege los diez tipos de consecutivo. |
| Factura electrónica / tiquete | `EscenariosAbarrotesCarnesTests.FacturaElectronica_MezclaAbarrotesYCarnePorPeso...` calcula subtotal, descuento e impuesto. | `ImpresionAbarrotesCarnesTests.VentaMixta...` renderiza factura A4 y tiquete térmico. |
| Consignación | `ContratosOperacionDiariaTests.Consignacion_DeCarnePorCliente_UsaElCicloDeBoletaConteoYPrefactura` valida boleta, conteo, prefactura, aprobación y facturación. | `ImpresionRenderTests` cubre la boleta de consignación en ambos formatos. |
| Producción | `ContratosOperacionDiariaTests.Produccion_DeHamburguesaRespetaLotesYConvierteSoloLaCantidadSolicitada` valida cálculo y conversión de lotes de carne. | `EndpointSmokeTests` verifica que las rutas publicadas de Producción estén mapeadas. |
| Apertura, arqueo y cierre | `EscenariosAbarrotesCarnesTests.ArqueoDeCaja...` comprueba efectivo, dólares y tipo de cambio. | `ReglasOperacionDiariaTests.Conciliacion_DeCajaDeCarniceria...` comprueba fondo inicial, ingresos, egresos y saldo esperado. |
| Impresión | El sitio ya prueba sus documentos PDF en `GeneradorPdfTests`. | `ImpresionRenderTests` renderiza cada tipo en A4 y térmico; el escenario adicional cubre la venta mixta. |
| Preventa | `EscenariosAbarrotesCarnesTests.PreventaDeCarnes...` cubre precio por kg, descuento y total. | `EndpointSmokeTests` recorre los endpoints de venta/preventa publicados. |
| Compra | `EscenariosAbarrotesCarnesTests.CompraDelProveedor...` verifica descuento comercial, impuesto y total de una compra de carne. | `EndpointSmokeTests` verifica el mapeo de las rutas de Compras. |
| Pedido / orden de compra | `ContratosOperacionDiariaTests.Series_CobroCreditoYOrdenCompra_ConservanSusComandosDeNegocio` valida el comando de orden a crédito y sus líneas. | `EndpointSmokeTests` verifica el mapeo de las rutas de órdenes de compra. |

## Ejecución

```bash
# Sitio web
dotnet test tests/SuvesaPosSitioAplicacion.Tests/SuvesaPosSitioAplicacion.Tests.csproj --no-restore

# API
dotnet test ApiSuvesaPos/ApiSuvesaPos.SmokeTests/ApiSuvesaPos.SmokeTests.csproj --no-restore
```

Las pruebas de esta matriz no requieren credenciales ni una base SQL compartida:
las de sitio capturan el contrato HTTP y las de API ejercitan reglas deterministas,
impresión y el enrutado. Las transacciones que persisten en SQL (reserva atómica de
serie, descuento de lote, asiento de caja y envío a Hacienda) deben complementarse
con una suite de integración contra una base aislada, pues contienen bloqueos y
transacciones propios de SQL Server.
