namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class TipoIdentificacionFiscalDTO
{
    public int Id { get; set; }
    public string? Descripcion { get; set; }
    public int CodigoFe { get; set; }
    public bool Activo { get; set; } = true;
}
