namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/coberturas (DTOs/CoberturaRutaDTOs.cs del API).

/// <summary>Quién cubre una ruta mientras otro colaborador está fuera. Solo informa; no cambia comisiones ni el titular.</summary>
public sealed class CoberturaRutaDTO
{
    public long IdCobertura { get; set; }
    public int IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public string IdUsuarioAusente { get; set; } = string.Empty;
    public string? NombreAusente { get; set; }
    public string IdUsuarioCubre { get; set; } = string.Empty;
    public string? NombreCubre { get; set; }
    public DateTime Desde { get; set; }
    /// <summary>Incluida.</summary>
    public DateTime Hasta { get; set; }
    public string? Motivo { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class FiltroCoberturasDTO
{
    public int? IdRuta { get; set; }
    public bool SoloVigentes { get; set; } = true;
    public bool IncluirAnuladas { get; set; }
}

public sealed class UsuarioCubridorDTO
{
    public string IdUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsAgente { get; set; }
    public bool EsServicioAlCliente { get; set; }
}
