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
