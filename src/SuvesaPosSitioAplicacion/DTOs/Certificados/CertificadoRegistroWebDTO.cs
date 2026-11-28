namespace SuvesaPosSitioAplicacion.DTOs.Certificados;

/// <summary>
/// Espejo de <c>CertificadoRegistroDTO</c> del API (api/certificados-registro). A mano mientras los contratos
/// NSwag no se regeneren contra el API que ya lo trae.
/// </summary>
public sealed class CertificadoRegistroWebDTO
{
    public long IdCertificado { get; set; }
    public long? IdArticulo { get; set; }
    public int? CodigoProv { get; set; }
    public string? Descripcion { get; set; }
    public string? NumeroRegistro { get; set; }
    public bool TieneArchivo { get; set; }
    public string? NombreArchivo { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public DateOnly? FechaAviso { get; set; }
    /// <summary>Vigente, Renovado o Baja.</summary>
    public string Estado { get; set; } = "Vigente";
    public string? Notas { get; set; }
    /// <summary>Solo al guardar: PDF en base64. Vacío = conservar el que ya tiene.</summary>
    public string? ArchivoBase64 { get; set; }
    /// <summary>Solo al renovar: certificado que este reemplaza.</summary>
    public long? Reemplaza { get; set; }

    /// <summary>Lo que ve el usuario: Vencido, Por vencer o Vigente (según las fechas) o el estado del historial.</summary>
    public string Situacion(DateOnly hoy)
    {
        if (Estado != "Vigente") return Estado;
        if (FechaVencimiento < hoy) return "Vencido";
        return FechaAviso is { } aviso && aviso <= hoy ? "Por vencer" : "Vigente";
    }
}

public sealed class CertificadoArchivoWebDTO
{
    public string NombreArchivo { get; set; } = "";
    public string ArchivoBase64 { get; set; } = "";
}
