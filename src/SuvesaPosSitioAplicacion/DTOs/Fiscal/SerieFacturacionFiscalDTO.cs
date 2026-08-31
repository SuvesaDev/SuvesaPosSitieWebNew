namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class SerieFacturacionFiscalDTO
{
    public int IdSerie { get; set; }
    public long Secuencia { get; set; }
    public int NumeroTerminal { get; set; }
    public int IdSucursal { get; set; }
    public int IdEmisor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? IdTipoFactura { get; set; }
    public bool? EsCredito { get; set; }
    public bool? EsRecibo { get; set; }
    public bool? EsPago { get; set; }
    public bool? EsConsignacion { get; set; }
    public bool EmisionV44Habilitada { get; set; }
    public string? CodigoFE { get; set; }
    public string? NumeroSucursalFE { get; set; }
}
