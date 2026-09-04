namespace SuvesaPosSitioAplicacion.DTOs.Caja;

/// <summary>
/// Conciliación monetaria de una apertura calculada **desde el mayor de caja**
/// (<c>MovimientoCaja</c>), no desde documentos fiscales.
/// SANEAMIENTO Fase 3 — espejo de <c>GET api/caja/{napertura}/conciliacion</c>.
/// </summary>
public sealed class ConciliacionCajaWebDTO
{
    public long NumApertura { get; set; }
    public string Estado { get; set; } = "";
    public decimal FondoInicial { get; set; }
    public List<LineaConciliacionCajaWebDTO> Lineas { get; set; } = new();
    public decimal TotalEsperado { get; set; }
    /// <summary>Ventas del período — informativo, NO entra en la fórmula monetaria.</summary>
    public decimal VentasComercialInformativo { get; set; }
}

/// <summary>Resultado de cerrar una caja desde la conciliación (SANEAMIENTO Fase 8.4).</summary>
public sealed class CierreConciliadoWebDTO
{
    public long IdCierre { get; set; }
    public long NumApertura { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal TotalEsperado { get; set; }
    public string EstadoApertura { get; set; } = "";
    public bool FueReintento { get; set; }
}

public sealed class LineaConciliacionCajaWebDTO
{
    public string CodigoFormaPago { get; set; } = "";
    public int CodMoneda { get; set; }
    public decimal Ingresos { get; set; }
    public decimal Egresos { get; set; }
    public decimal SaldoEsperado { get; set; }
    public int Movimientos { get; set; }
}
