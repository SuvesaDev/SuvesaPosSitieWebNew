namespace SuvesaPosSitioAplicacion.DTOs.Cobranza;

/// <summary>Espejo de <c>SolicitudDesbloqueoDTO</c> del API (api/cobranza/desbloqueo-credito).</summary>
public sealed class SolicitudDesbloqueoWebDTO
{
    public long Id { get; set; }
    public long IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public string Solicitante { get; set; } = string.Empty;
    public DateTime FechaSolicitudUtc { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal? MontoEstimado { get; set; }
    /// <summary>Pendiente, Aprobada, Rechazada, Consumida o Vencida.</summary>
    public string Estado { get; set; } = string.Empty;
    public string? Resolutor { get; set; }
    public DateTime? FechaResolucionUtc { get; set; }
    public string? ComentarioResolucion { get; set; }
    public DateTime? FechaVencimientoUtc { get; set; }
    public bool Vigente { get; set; }
    public int FacturasVencidas { get; set; }
    public int DiasMoraMaximo { get; set; }
    public decimal SaldoVencido { get; set; }
    public decimal LimiteAprobado { get; set; }
    public decimal Disponible { get; set; }
}

public sealed class SolicitarDesbloqueoWebComando
{
    public long IdCliente { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal? MontoEstimado { get; set; }
}

public sealed class ResolverDesbloqueoWebComando
{
    public string? Comentario { get; set; }
}
