using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Lotes;

// TEMPORAL — DTOs a mano espejo de los del API (MEJORA_LOTES_API.md). El regen
// completo de contratos NSwag no es viable en local. Al regenerar, el generado
// trae estos tipos y este archivo se borra.

// ---- Ficha de movimientos (§3.3 / A4) ----

public sealed class MovimientoInventarioFiltro
{
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("desde")] public System.DateTime? Desde { get; set; }
    [JsonPropertyName("hasta")] public System.DateTime? Hasta { get; set; }
    [JsonPropertyName("tipoMovimiento")] public int? TipoMovimiento { get; set; }
    /// <summary>Bodega a filtrar; null = todas (BODEGAS_POR_CENTRO_Y_TRASLADOS_API.md §3.3).</summary>
    [JsonPropertyName("bodega")] public int? Bodega { get; set; }
    [JsonPropertyName("pagina")] public int Pagina { get; set; } = 1;
    [JsonPropertyName("tamanoPagina")] public int TamanoPagina { get; set; } = 50;
}

public sealed class MovimientoInventarioConsulta
{
    [JsonPropertyName("idStock")] public long IdStock { get; set; }
    [JsonPropertyName("fecha")] public System.DateTime Fecha { get; set; }
    [JsonPropertyName("movimiento")] public int Movimiento { get; set; }
    [JsonPropertyName("movimientoDescripcion")] public string? MovimientoDescripcion { get; set; }
    [JsonPropertyName("tipo")] public int Tipo { get; set; }
    [JsonPropertyName("documento")] public string? Documento { get; set; }
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("codProveedor")] public int? CodProveedor { get; set; }
    [JsonPropertyName("codCliente")] public long? CodCliente { get; set; }
    [JsonPropertyName("nombreContraparte")] public string? NombreContraparte { get; set; }
    [JsonPropertyName("idBodega")] public int IdBodega { get; set; }
    [JsonPropertyName("nombreBodega")] public string? NombreBodega { get; set; }
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("numeroLote")] public string? NumeroLote { get; set; }
    [JsonPropertyName("existenciaAnterior")] public double ExistenciaAnterior { get; set; }
    [JsonPropertyName("cantidad")] public double Cantidad { get; set; }
    [JsonPropertyName("existenciaNueva")] public double ExistenciaNueva { get; set; }
    [JsonPropertyName("esReseteoToma")] public bool EsReseteoToma { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
}

public sealed class MovimientoInventarioPagina
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("pagina")] public int Pagina { get; set; }
    [JsonPropertyName("tamanoPagina")] public int TamanoPagina { get; set; }
    [JsonPropertyName("movimientos")] public List<MovimientoInventarioConsulta> Movimientos { get; set; } = new();
}

// ---- Existencia consolidada (§3.3 / A2) ----

public sealed class ExistenciaPorBodega
{
    [JsonPropertyName("idBodega")] public int IdBodega { get; set; }
    [JsonPropertyName("nombreBodega")] public string NombreBodega { get; set; } = string.Empty;
    [JsonPropertyName("existencia")] public double Existencia { get; set; }
}

public sealed class LoteExistencia
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("numero")] public string Numero { get; set; } = string.Empty;
    [JsonPropertyName("vencimiento")] public System.DateTime? Vencimiento { get; set; }
    [JsonPropertyName("esUnico")] public bool EsUnico { get; set; }
    [JsonPropertyName("bloqueado")] public bool Bloqueado { get; set; }
    [JsonPropertyName("vencido")] public bool Vencido { get; set; }
    [JsonPropertyName("existenciaTotal")] public double ExistenciaTotal { get; set; }
    [JsonPropertyName("porBodega")] public List<ExistenciaPorBodega> PorBodega { get; set; } = new();
}

public sealed class ExistenciaConsolidada
{
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("tipoArticulo")] public int TipoArticulo { get; set; }
    [JsonPropertyName("manejaLote")] public bool ManejaLote { get; set; }
    [JsonPropertyName("loteUnico")] public bool LoteUnico { get; set; }
    [JsonPropertyName("loteUnicoFijado")] public bool LoteUnicoFijado { get; set; }
    [JsonPropertyName("existenciaTotal")] public double ExistenciaTotal { get; set; }
    [JsonPropertyName("porBodega")] public List<ExistenciaPorBodega> PorBodega { get; set; } = new();
    [JsonPropertyName("lotes")] public List<LoteExistencia> Lotes { get; set; } = new();
}

public sealed class ReporteAlertasInventario
{
    [JsonPropertyName("generadoEn")] public DateTime GeneradoEn { get; set; }
    [JsonPropertyName("desdeMovimientosSinDocumento")] public DateTime DesdeMovimientosSinDocumento { get; set; }
    [JsonPropertyName("alertas")] public List<AlertaInventarioOperativa> Alertas { get; set; } = new();
}

public sealed class AlertaInventarioOperativa
{
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = string.Empty;
    [JsonPropertyName("severidad")] public string Severidad { get; set; } = string.Empty;
    [JsonPropertyName("idStock")] public long? IdStock { get; set; }
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("codigoArticulo")] public string CodigoArticulo { get; set; } = string.Empty;
    [JsonPropertyName("descripcionArticulo")] public string DescripcionArticulo { get; set; } = string.Empty;
    [JsonPropertyName("idBodega")] public int? IdBodega { get; set; }
    [JsonPropertyName("nombreBodega")] public string? NombreBodega { get; set; }
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("numeroLote")] public string? NumeroLote { get; set; }
    [JsonPropertyName("vencimiento")] public DateTime? Vencimiento { get; set; }
    [JsonPropertyName("existencia")] public double Existencia { get; set; }
    [JsonPropertyName("minimo")] public double? Minimo { get; set; }
    [JsonPropertyName("fechaMovimiento")] public DateTime? FechaMovimiento { get; set; }
    [JsonPropertyName("tipoMovimiento")] public int? TipoMovimiento { get; set; }
}

public sealed class ActualizarExistencia
{
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("bodega")] public int Bodega { get; set; }
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("cantidad")] public double Cantidad { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
}

public sealed class MovimientoInventarioResultado
{
    [JsonPropertyName("existenciaAnterior")] public double ExistenciaAnterior { get; set; }
    [JsonPropertyName("existenciaNueva")] public double ExistenciaNueva { get; set; }
    [JsonPropertyName("existenciaTotalArticulo")] public double ExistenciaTotalArticulo { get; set; }
}

// ---- Consumo de lote en venta / ingreso en compra (§5 / §6) ----

public sealed class LoteConsumoVenta
{
    [JsonPropertyName("idStockLote")] public long IdStockLote { get; set; }
    [JsonPropertyName("cantidad")] public double Cantidad { get; set; }
    [JsonPropertyName("cantidadDevuelta")] public double CantidadDevuelta { get; set; }
}

public sealed class LoteIngresoCompra
{
    [JsonPropertyName("numero")] public string? Numero { get; set; }
    [JsonPropertyName("vencimiento")] public System.DateOnly? Vencimiento { get; set; }
    [JsonPropertyName("cantidad")] public double Cantidad { get; set; }
}

// ---- Toma física (§3.7 / T1-T4) ----

public sealed class TomaFisicaFiltro
{
    [JsonPropertyName("bodega")] public int Bodega { get; set; }
    [JsonPropertyName("familia")] public int? Familia { get; set; }
    [JsonPropertyName("texto")] public string? Texto { get; set; }
}

public sealed class TomaFisicaArticulo
{
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("codArticulo")] public string CodArticulo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("bodega")] public int Bodega { get; set; }
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("numeroLote")] public string? NumeroLote { get; set; }
    [JsonPropertyName("vencimiento")] public System.DateTime? Vencimiento { get; set; }
    [JsonPropertyName("esLoteUnico")] public bool EsLoteUnico { get; set; }
    [JsonPropertyName("manejaLote")] public bool ManejaLote { get; set; }
    [JsonPropertyName("existenciaSistema")] public double ExistenciaSistema { get; set; }
    [JsonPropertyName("costo")] public double Costo { get; set; }
    // sólo cliente: lo que teclea el usuario
    [JsonIgnore] public double? Contado { get; set; }
}

public sealed class TomaFisicaGuardarLinea
{
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("contado")] public double Contado { get; set; }
}

public sealed class TomaFisicaGuardar
{
    [JsonPropertyName("bodega")] public int Bodega { get; set; }
    [JsonPropertyName("fecha")] public System.DateTime? Fecha { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("lineas")] public List<TomaFisicaGuardarLinea> Lineas { get; set; } = new();
}

public sealed class TomaFisicaReporteLinea
{
    [JsonPropertyName("codArticulo")] public string CodArticulo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("numeroLote")] public string? NumeroLote { get; set; }
    [JsonPropertyName("existenciaSistema")] public double ExistenciaSistema { get; set; }
    [JsonPropertyName("contado")] public double Contado { get; set; }
    [JsonPropertyName("diferencia")] public double Diferencia { get; set; }
    [JsonPropertyName("costoUnitario")] public double CostoUnitario { get; set; }
    [JsonPropertyName("costoDiferencia")] public double CostoDiferencia { get; set; }
}

public sealed class TomaFisicaReporte
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("fecha")] public System.DateTime Fecha { get; set; }
    [JsonPropertyName("bodega")] public int Bodega { get; set; }
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("articulosAjustados")] public int ArticulosAjustados { get; set; }
    [JsonPropertyName("unidadesGanadas")] public double UnidadesGanadas { get; set; }
    [JsonPropertyName("unidadesPerdidas")] public double UnidadesPerdidas { get; set; }
    [JsonPropertyName("costoPerdidas")] public double CostoPerdidas { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("lineas")] public List<TomaFisicaReporteLinea> Lineas { get; set; } = new();
}

// ---- Bodegas operativas por centro (BODEGAS_POR_CENTRO_Y_TRASLADOS_API.md §3.1) ----

public sealed class BodegaOperativa
{
    [JsonPropertyName("idBodega")] public int IdBodega { get; set; }
    [JsonPropertyName("nombreBodega")] public string NombreBodega { get; set; } = string.Empty;
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("bloqueada")] public bool? Bloqueada { get; set; }
    [JsonPropertyName("estado")] public bool? Estado { get; set; }
    [JsonPropertyName("esCostaPets")] public bool? EsCostaPets { get; set; }
    [JsonPropertyName("idSucursal")] public int? IdSucursal { get; set; }
    [JsonPropertyName("nombreSucursal")] public string? NombreSucursal { get; set; }
    [JsonPropertyName("esConsignacion")] public bool EsConsignacion { get; set; }
    [JsonPropertyName("esConsignacionCentral")] public bool EsConsignacionCentral { get; set; }
}

// ---- Traslado de bodega a bodega (§3.4) ----

public sealed class TrasladoBodegaLineaEntrada
{
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("cantidad")] public double Cantidad { get; set; }
}

public sealed class TrasladoBodegaRequest
{
    [JsonPropertyName("idBodegaOrigen")] public int IdBodegaOrigen { get; set; }
    [JsonPropertyName("idBodegaDestino")] public int IdBodegaDestino { get; set; }
    [JsonPropertyName("fecha")] public System.DateTime? Fecha { get; set; }
    [JsonPropertyName("documento")] public string? Documento { get; set; }
    [JsonPropertyName("motivo")] public string? Motivo { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("lineas")] public List<TrasladoBodegaLineaEntrada> Lineas { get; set; } = new();
}

public sealed class TrasladoBodegaLinea
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("idArticulo")] public long IdArticulo { get; set; }
    [JsonPropertyName("codArticulo")] public string CodArticulo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("idStockLote")] public long? IdStockLote { get; set; }
    [JsonPropertyName("numeroLote")] public string? NumeroLote { get; set; }
    [JsonPropertyName("cantidad")] public double Cantidad { get; set; }
    [JsonPropertyName("costoUnitario")] public double CostoUnitario { get; set; }
    [JsonPropertyName("costoLinea")] public double CostoLinea { get; set; }
}

public sealed class TrasladoBodega
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("idBodegaOrigen")] public int IdBodegaOrigen { get; set; }
    [JsonPropertyName("nombreBodegaOrigen")] public string? NombreBodegaOrigen { get; set; }
    [JsonPropertyName("idBodegaDestino")] public int IdBodegaDestino { get; set; }
    [JsonPropertyName("nombreBodegaDestino")] public string? NombreBodegaDestino { get; set; }
    [JsonPropertyName("tipo")] public int Tipo { get; set; }
    [JsonPropertyName("fecha")] public System.DateTime Fecha { get; set; }
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("documento")] public string? Documento { get; set; }
    [JsonPropertyName("motivo")] public string? Motivo { get; set; }
    [JsonPropertyName("estado")] public int Estado { get; set; }
    [JsonPropertyName("estadoDescripcion")] public string EstadoDescripcion { get; set; } = string.Empty;
    [JsonPropertyName("costoTotal")] public double CostoTotal { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("fechaAnulacion")] public System.DateTime? FechaAnulacion { get; set; }
    [JsonPropertyName("motivoAnulacion")] public string? MotivoAnulacion { get; set; }
    [JsonPropertyName("lineas")] public List<TrasladoBodegaLinea> Lineas { get; set; } = new();
}

public sealed class TrasladoBodegaFiltro
{
    [JsonPropertyName("idSucursal")] public int? IdSucursal { get; set; }
    [JsonPropertyName("idBodegaOrigen")] public int? IdBodegaOrigen { get; set; }
    [JsonPropertyName("idBodegaDestino")] public int? IdBodegaDestino { get; set; }
    [JsonPropertyName("desde")] public System.DateTime? Desde { get; set; }
    [JsonPropertyName("hasta")] public System.DateTime? Hasta { get; set; }
    [JsonPropertyName("texto")] public string? Texto { get; set; }
    [JsonPropertyName("estado")] public int? Estado { get; set; }
    [JsonPropertyName("tope")] public int Tope { get; set; } = 200;
}

public sealed class TrasladoBodegaResumen
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("fecha")] public System.DateTime Fecha { get; set; }
    [JsonPropertyName("idBodegaOrigen")] public int IdBodegaOrigen { get; set; }
    [JsonPropertyName("nombreBodegaOrigen")] public string? NombreBodegaOrigen { get; set; }
    [JsonPropertyName("idBodegaDestino")] public int IdBodegaDestino { get; set; }
    [JsonPropertyName("nombreBodegaDestino")] public string? NombreBodegaDestino { get; set; }
    [JsonPropertyName("estado")] public int Estado { get; set; }
    [JsonPropertyName("estadoDescripcion")] public string EstadoDescripcion { get; set; } = string.Empty;
    [JsonPropertyName("cantidadLineas")] public int CantidadLineas { get; set; }
    [JsonPropertyName("costoTotal")] public double CostoTotal { get; set; }
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("documento")] public string? Documento { get; set; }
}

public sealed class AnularTrasladoBodega
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("motivo")] public string? Motivo { get; set; }
}
