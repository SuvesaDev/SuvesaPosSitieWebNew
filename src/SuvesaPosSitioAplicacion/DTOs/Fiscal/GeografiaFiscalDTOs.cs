namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;
public sealed class ProvinciaFiscalDTO { public int IdProvincia { get; set; } public string Descripcion { get; set; } = string.Empty; public string CodigoFE { get; set; } = string.Empty; }
public sealed class CantonFiscalDTO { public int IdCanton { get; set; } public int IdProvincia { get; set; } public string Descripcion { get; set; } = string.Empty; public string CodigoFE { get; set; } = string.Empty; }
public sealed class DistritoFiscalDTO { public int IdDistrito { get; set; } public int IdCanton { get; set; } public string Descripcion { get; set; } = string.Empty; public string CodigoFE { get; set; } = string.Empty; }
