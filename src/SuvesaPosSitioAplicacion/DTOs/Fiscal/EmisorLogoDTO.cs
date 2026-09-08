namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

/// <summary>Metadatos ligeros del único logo oficial de un emisor.</summary>
public sealed class EmisorLogoResumenDTO
{
    public bool TieneLogo { get; set; }
    public string? MimeType { get; set; }
    public string? NombreArchivo { get; set; }
    public string? HashSha256 { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

/// <summary>Contenido que recibe el API al cargar/reemplazar el logo.</summary>
public sealed class EmisorLogoActualizarDTO
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string ContenidoBase64 { get; set; } = string.Empty;
}

/// <summary>Resultado interno del BFF para transmitir el logo al navegador.</summary>
public sealed class EmisorLogoArchivoDTO
{
    public byte[] Contenido { get; init; } = Array.Empty<byte>();
    public string MimeType { get; init; } = "application/octet-stream";
}
