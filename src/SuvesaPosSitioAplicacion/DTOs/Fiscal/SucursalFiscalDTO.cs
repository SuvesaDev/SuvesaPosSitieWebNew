namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class SucursalFiscalDTO
{
    public int Id { get; set; }
    public string? NombreComercial { get; set; }
    public string? NombreFiscal { get; set; }
    public int TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Alias { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? NumeroSucursalFE { get; set; }
}
