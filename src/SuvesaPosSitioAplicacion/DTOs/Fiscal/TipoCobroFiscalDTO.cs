namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;
public sealed class TipoCobroFiscalDTO { public int IdTipoCobro { get; set; } public string? Descripcion { get; set; } public bool Activo { get; set; } = true; public bool EsEntregaCuenta { get; set; } public bool EsUsoCuenta { get; set; } }
