namespace SuvesaPosSitioAplicacion.DTOs.Correo;

/// <summary>Configuración SMTP del emisor tal como la devuelve el API (sin contraseña).</summary>
public sealed class ConfiguracionCorreoVistaDTO
{
    public int IdEmisor { get; set; }
    public string SmtpHost { get; set; } = "";
    public int SmtpPuerto { get; set; } = 587;
    public bool UsaSsl { get; set; } = true;
    public string Usuario { get; set; } = "";
    public bool ContrasenaAsignada { get; set; }
    public string RemitenteNombre { get; set; } = "";
    public string RemitenteCorreo { get; set; } = "";
    public string? CopiaOculta { get; set; }
    public bool Habilitado { get; set; }
    public bool AlertarPorCorreo { get; set; }
    public string? AsuntoPlantilla { get; set; }
    public string? CuerpoPlantilla { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

/// <summary>Datos para guardar la configuración. Contraseña vacía = no cambiar.</summary>
public sealed class ConfiguracionCorreoGuardarDTO
{
    public int IdEmisor { get; set; }
    public string SmtpHost { get; set; } = "";
    public int SmtpPuerto { get; set; } = 587;
    public bool UsaSsl { get; set; } = true;
    public string Usuario { get; set; } = "";
    public string? Contrasena { get; set; }
    public string RemitenteNombre { get; set; } = "";
    public string RemitenteCorreo { get; set; } = "";
    public string? CopiaOculta { get; set; }
    public bool Habilitado { get; set; }
    public bool AlertarPorCorreo { get; set; }
    public string? AsuntoPlantilla { get; set; }
    public string? CuerpoPlantilla { get; set; }
}

/// <summary>Cuerpo de la prueba de envío.</summary>
public sealed class ProbarCorreoDTO
{
    public string Destino { get; set; } = "";
}

/// <summary>Fila de la bandeja de envíos de correo.</summary>
public sealed class EnvioCorreoDTO
{
    public long Id { get; set; }
    public string Clave { get; set; } = "";
    public int IdEmisor { get; set; }
    public string TipoComprobante { get; set; } = "";
    public string? Destinatarios { get; set; }
    public string Estado { get; set; } = "";
    public int Intentos { get; set; }
    public int MaxIntentos { get; set; }
    public string? UltimoError { get; set; }
    public DateTime? ProximoIntentoUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaEnvioUtc { get; set; }
    public bool AdjuntoXmlFirmado { get; set; }
    public bool AdjuntoRespuesta { get; set; }
    public bool AdjuntoPdf { get; set; }
}

/// <summary>Página de la bandeja de envíos.</summary>
public sealed class PaginaEnviosCorreoDTO
{
    public IReadOnlyList<EnvioCorreoDTO> Items { get; set; } = Array.Empty<EnvioCorreoDTO>();
    public int Total { get; set; }
}

/// <summary>Alerta para el administrador.</summary>
public sealed class AlertaAdministradorDTO
{
    public long Id { get; set; }
    public string Tipo { get; set; } = "";
    public string? Clave { get; set; }
    public int? IdEmisor { get; set; }
    public string Titulo { get; set; } = "";
    public string Detalle { get; set; } = "";
    public DateTime FechaCreacionUtc { get; set; }
    public bool Leida { get; set; }
    public string? LeidaPorUsuario { get; set; }
    public DateTime? FechaLeidaUtc { get; set; }
}

/// <summary>Página de alertas.</summary>
public sealed class PaginaAlertasAdministradorDTO
{
    public IReadOnlyList<AlertaAdministradorDTO> Items { get; set; } = Array.Empty<AlertaAdministradorDTO>();
    public int Total { get; set; }
}
