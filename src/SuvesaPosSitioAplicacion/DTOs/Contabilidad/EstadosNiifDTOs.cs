using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Contabilidad;

public sealed class LineaEstadoNiifDTO
{
    [JsonPropertyName("idCuentaContable")] public long IdCuentaContable { get; set; }
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("monto")] public decimal Monto { get; set; }
}

public sealed class GrupoEstadoNiifDTO
{
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("lineas")] public List<LineaEstadoNiifDTO> Lineas { get; set; } = new();
}

public sealed class SeccionEstadoNiifDTO
{
    [JsonPropertyName("clave")] public string Clave { get; set; } = string.Empty;
    [JsonPropertyName("titulo")] public string Titulo { get; set; } = string.Empty;
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("grupos")] public List<GrupoEstadoNiifDTO> Grupos { get; set; } = new();
}

public sealed class EstadoResultadosNiifDTO
{
    [JsonPropertyName("desde")] public DateOnly Desde { get; set; }
    [JsonPropertyName("hasta")] public DateOnly Hasta { get; set; }
    [JsonPropertyName("reexpresado")] public bool Reexpresado { get; set; }
    [JsonPropertyName("secciones")] public List<SeccionEstadoNiifDTO> Secciones { get; set; } = new();
    [JsonPropertyName("ingresosOrdinarios")] public decimal IngresosOrdinarios { get; set; }
    [JsonPropertyName("costoVentas")] public decimal CostoVentas { get; set; }
    [JsonPropertyName("utilidadBruta")] public decimal UtilidadBruta { get; set; }
    [JsonPropertyName("gastosAdministrativos")] public decimal GastosAdministrativos { get; set; }
    [JsonPropertyName("gastosVenta")] public decimal GastosVenta { get; set; }
    [JsonPropertyName("resultadoOperacion")] public decimal ResultadoOperacion { get; set; }
    [JsonPropertyName("otrosIngresos")] public decimal OtrosIngresos { get; set; }
    [JsonPropertyName("ingresosFinancieros")] public decimal IngresosFinancieros { get; set; }
    [JsonPropertyName("gastosFinancieros")] public decimal GastosFinancieros { get; set; }
    [JsonPropertyName("otrosGastos")] public decimal OtrosGastos { get; set; }
    [JsonPropertyName("resultadoAntesImpuesto")] public decimal ResultadoAntesImpuesto { get; set; }
    [JsonPropertyName("impuestoRenta")] public decimal ImpuestoRenta { get; set; }
    [JsonPropertyName("resultadoNeto")] public decimal ResultadoNeto { get; set; }
    [JsonPropertyName("notas")] public List<string> Notas { get; set; } = new();
}

public sealed class SituacionFinancieraNiifDTO
{
    [JsonPropertyName("fechaCorte")] public DateOnly FechaCorte { get; set; }
    [JsonPropertyName("reexpresado")] public bool Reexpresado { get; set; }
    [JsonPropertyName("activoCorriente")] public SeccionEstadoNiifDTO ActivoCorriente { get; set; } = new();
    [JsonPropertyName("activoNoCorriente")] public SeccionEstadoNiifDTO ActivoNoCorriente { get; set; } = new();
    [JsonPropertyName("pasivoCorriente")] public SeccionEstadoNiifDTO PasivoCorriente { get; set; } = new();
    [JsonPropertyName("pasivoNoCorriente")] public SeccionEstadoNiifDTO PasivoNoCorriente { get; set; } = new();
    [JsonPropertyName("patrimonio")] public SeccionEstadoNiifDTO Patrimonio { get; set; } = new();
    [JsonPropertyName("totalActivos")] public decimal TotalActivos { get; set; }
    [JsonPropertyName("totalPasivos")] public decimal TotalPasivos { get; set; }
    [JsonPropertyName("totalPatrimonio")] public decimal TotalPatrimonio { get; set; }
    [JsonPropertyName("cuadra")] public bool Cuadra { get; set; }
    [JsonPropertyName("notas")] public List<string> Notas { get; set; } = new();
}

public sealed class LineaFlujoNiifDTO
{
    [JsonPropertyName("concepto")] public string Concepto { get; set; } = string.Empty;
    [JsonPropertyName("monto")] public decimal Monto { get; set; }
}

public sealed class FlujoEfectivoIndirectoDTO
{
    [JsonPropertyName("desde")] public DateOnly Desde { get; set; }
    [JsonPropertyName("hasta")] public DateOnly Hasta { get; set; }
    [JsonPropertyName("utilidadNeta")] public decimal UtilidadNeta { get; set; }
    [JsonPropertyName("ajustesNoMonetarios")] public List<LineaFlujoNiifDTO> AjustesNoMonetarios { get; set; } = new();
    [JsonPropertyName("cambiosCapitalTrabajo")] public List<LineaFlujoNiifDTO> CambiosCapitalTrabajo { get; set; } = new();
    [JsonPropertyName("flujoOperacion")] public decimal FlujoOperacion { get; set; }
    [JsonPropertyName("inversion")] public List<LineaFlujoNiifDTO> Inversion { get; set; } = new();
    [JsonPropertyName("flujoInversion")] public decimal FlujoInversion { get; set; }
    [JsonPropertyName("financiamiento")] public List<LineaFlujoNiifDTO> Financiamiento { get; set; } = new();
    [JsonPropertyName("flujoFinanciamiento")] public decimal FlujoFinanciamiento { get; set; }
    [JsonPropertyName("variacionNeta")] public decimal VariacionNeta { get; set; }
    [JsonPropertyName("saldoInicial")] public decimal SaldoInicial { get; set; }
    [JsonPropertyName("saldoFinal")] public decimal SaldoFinal { get; set; }
    [JsonPropertyName("diferencia")] public decimal Diferencia { get; set; }
    [JsonPropertyName("cuadra")] public bool Cuadra { get; set; }
    [JsonPropertyName("notas")] public List<string> Notas { get; set; } = new();
}

public sealed class FilaCambiosPatrimonioDTO
{
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("saldoInicial")] public decimal SaldoInicial { get; set; }
    [JsonPropertyName("aumentos")] public decimal Aumentos { get; set; }
    [JsonPropertyName("disminuciones")] public decimal Disminuciones { get; set; }
    [JsonPropertyName("saldoFinal")] public decimal SaldoFinal { get; set; }
}

public sealed class EstadoCambiosPatrimonioDTO
{
    [JsonPropertyName("desde")] public DateOnly Desde { get; set; }
    [JsonPropertyName("hasta")] public DateOnly Hasta { get; set; }
    [JsonPropertyName("filas")] public List<FilaCambiosPatrimonioDTO> Filas { get; set; } = new();
    [JsonPropertyName("totalInicial")] public decimal TotalInicial { get; set; }
    [JsonPropertyName("totalAumentos")] public decimal TotalAumentos { get; set; }
    [JsonPropertyName("totalDisminuciones")] public decimal TotalDisminuciones { get; set; }
    [JsonPropertyName("totalFinal")] public decimal TotalFinal { get; set; }
}

public sealed class LineaBalanzaPeriodoDTO
{
    [JsonPropertyName("idCuentaContable")] public long IdCuentaContable { get; set; }
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("saldoInicial")] public decimal SaldoInicial { get; set; }
    [JsonPropertyName("debitos")] public decimal Debitos { get; set; }
    [JsonPropertyName("creditos")] public decimal Creditos { get; set; }
    [JsonPropertyName("saldoFinal")] public decimal SaldoFinal { get; set; }
}

public sealed class BalanzaPeriodoDTO
{
    [JsonPropertyName("desde")] public DateOnly Desde { get; set; }
    [JsonPropertyName("hasta")] public DateOnly Hasta { get; set; }
    [JsonPropertyName("lineas")] public List<LineaBalanzaPeriodoDTO> Lineas { get; set; } = new();
    [JsonPropertyName("totalDebitos")] public decimal TotalDebitos { get; set; }
    [JsonPropertyName("totalCreditos")] public decimal TotalCreditos { get; set; }
    [JsonPropertyName("cuadra")] public bool Cuadra { get; set; }
}

public sealed class AjusteReexpresionDTO
{
    [JsonPropertyName("idAsiento")] public long IdAsiento { get; set; }
    [JsonPropertyName("periodoAfectado")] public string PeriodoAfectado { get; set; } = string.Empty;
    [JsonPropertyName("fechaContable")] public DateOnly FechaContable { get; set; }
    [JsonPropertyName("concepto")] public string? Concepto { get; set; }
    [JsonPropertyName("impactoResultado")] public decimal ImpactoResultado { get; set; }
}

public sealed class ComparativoNiifDTO
{
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    [JsonPropertyName("hayReexpresion")] public bool HayReexpresion { get; set; }
    [JsonPropertyName("resultadosActual")] public EstadoResultadosNiifDTO ResultadosActual { get; set; } = new();
    [JsonPropertyName("resultadosAnterior")] public EstadoResultadosNiifDTO ResultadosAnterior { get; set; } = new();
    [JsonPropertyName("situacionActual")] public SituacionFinancieraNiifDTO SituacionActual { get; set; } = new();
    [JsonPropertyName("situacionAnterior")] public SituacionFinancieraNiifDTO SituacionAnterior { get; set; } = new();
    [JsonPropertyName("resultadoCerradoAnterior")] public decimal? ResultadoCerradoAnterior { get; set; }
    [JsonPropertyName("diferenciaContraCierre")] public decimal DiferenciaContraCierre { get; set; }
    [JsonPropertyName("ajustes")] public List<AjusteReexpresionDTO> Ajustes { get; set; } = new();
    [JsonPropertyName("notas")] public List<string> Notas { get; set; } = new();
}
