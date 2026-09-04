namespace SuvesaPosSitioAplicacion.DTOs.Cobros;

/// <summary>Comando de devolución interna (no fiscal) — <c>POST api/venta-orquestada/devolucion-interna</c>.</summary>
public sealed class DevolucionInternaComandoWebDTO
{
    public string? ClaveIdempotencia { get; set; }
    public long IdVentaOrigen { get; set; }
    public string Usuario { get; set; } = "";
    public string Motivo { get; set; } = "";
    /// <summary>true = anula la venta origen (factura rechazada que se recreará).</summary>
    public bool AnularOrigen { get; set; } = true;
}

public sealed class DevolucionInternaResultadoWebDTO
{
    public long IdDevolucionInterna { get; set; }
    public decimal Total { get; set; }
    public decimal SaldoClienteNuevo { get; set; }
    public bool GeneroCreditoAFavor { get; set; }
    public bool FueReintento { get; set; }
}
