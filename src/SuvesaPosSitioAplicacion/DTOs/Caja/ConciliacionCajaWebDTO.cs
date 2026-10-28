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
    /// <summary>Tarjetas, depósitos y cheques por moneda. Informativo: no entra al total esperado ni a la diferencia.</summary>
    public List<ResumenBancarioWebDTO> ResumenBancario { get; set; } = new();
    /// <summary>Ventas del período — informativo, NO entra en la fórmula monetaria.</summary>
    public decimal VentasComercialInformativo { get; set; }
    /// <summary>Total declarado por el cajero en el arqueo físico. Null si aún no hay arqueo.</summary>
    public decimal? TotalCajero { get; set; }
    public bool TieneArqueo { get; set; }
    /// <summary>TotalCajero − TotalEsperado. Positivo = sobrante, negativo = faltante. Null si no hay arqueo.</summary>
    public decimal? Diferencia { get; set; }
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
    public decimal? TotalCajero { get; set; }
    public decimal? Diferencia { get; set; }
}

public sealed class LineaConciliacionCajaWebDTO
{
    public string CodigoFormaPago { get; set; } = "";
    public int CodMoneda { get; set; }
    public decimal Ingresos { get; set; }
    public decimal Egresos { get; set; }
    public decimal SaldoEsperado { get; set; }
    public int Movimientos { get; set; }
    /// <summary>Cheques y depósitos: se confirman en el módulo de Depósitos y no cuentan en el total esperado.</summary>
    public bool Informativa { get; set; }
}

public sealed class ResumenBancarioWebDTO
{
    public int CodMoneda { get; set; }
    /// <summary>Tarjetas declaradas por el cajero. Null si la apertura no tiene arqueo.</summary>
    public decimal? TarjetasCajero { get; set; }
    public decimal TarjetasSistema { get; set; }
    /// <summary>Depósitos, transferencias o SINPE hechos directo por el cliente y ya confirmados.</summary>
    public decimal DepositosDirectos { get; set; }
    public decimal DepositosDirectosPendientes { get; set; }
    /// <summary>Efectivo que el cajero envió al banco.</summary>
    public decimal PreDepositos { get; set; }
    public decimal TotalDepositos { get; set; }
    public decimal ChequesConfirmados { get; set; }
    public decimal ChequesPendientes { get; set; }
}
