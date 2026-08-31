namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class ConfiguracionPlazoFiscalDTO
{
    public int IdPlazo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadDias { get; set; }
    public bool Consignacion { get; set; }
    public bool Activo { get; set; }
}
