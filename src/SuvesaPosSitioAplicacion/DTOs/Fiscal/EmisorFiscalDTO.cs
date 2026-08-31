namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

public sealed class EmisorFiscalDTO
{
    public int Id { get; set; }
    public int TipoIdentificacion { get; set; }
    public string? Identificacion { get; set; }
    public string? Nombre { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Sucursal { get; set; }
    public int Distrito { get; set; }
    public string? OtrasSeñas { get; set; }
}

public sealed class CredencialesHaciendaFiscalDTO
{
    public int IdEmisor { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string Certificado { get; set; } = string.Empty;
    public string ContrasenaCertificado { get; set; } = string.Empty;
    public DateTime? VenceCertificado { get; set; }
}
