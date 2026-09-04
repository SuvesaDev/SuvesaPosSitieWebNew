namespace SuvesaPosSitioAplicacion.DTOs.Compras;

/// <summary>Factura pendiente candidata a incluirse en una boleta de trámite de cobro.</summary>
public sealed class FacturaTramiteCobroWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public string? ConsecutivoMh { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? Vence { get; set; }
    public int CodMoneda { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal SaldoActual { get; set; }
    public string? EstadoMh { get; set; }
}

public sealed class CrearTramiteCobroWebDTO
{
    public long IdCliente { get; set; }
    public int IdSucursal { get; set; }
    public int? IdEmisor { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? Entrega { get; set; }
    public string Recibe { get; set; } = "";
    public string? Observaciones { get; set; }
    public List<LineaTramiteCobroComandoWebDTO> Facturas { get; set; } = new();
}

public sealed class LineaTramiteCobroComandoWebDTO
{
    public long IdVenta { get; set; }
    public DateTime FechaPagoComprometida { get; set; }
}

public sealed class TramiteCobroWebDTO
{
    public long Id { get; set; }
    public long? Consecutivo { get; set; }
    public long IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int IdSucursal { get; set; }
    public int? IdEmisor { get; set; }
    public DateTime FechaEntrega { get; set; }
    public string Entrega { get; set; } = "";
    public string Recibe { get; set; } = "";
    public string? Observaciones { get; set; }
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacionUtc { get; set; }
    public string? MotivoAnulacion { get; set; }
    public DateTime? CorreoEnviadoUtc { get; set; }
    public string? CorreoDestino { get; set; }
    public string Usuario { get; set; } = "";
    public decimal Total { get; set; }
    public List<TramiteCobroLineaWebDTO> Lineas { get; set; } = new();
}

public sealed class TramiteCobroLineaWebDTO
{
    public long Id { get; set; }
    public long IdVenta { get; set; }
    public string NumFactura { get; set; } = "";
    public string? ConsecutivoMh { get; set; }
    public int CodMoneda { get; set; }
    public decimal MontoComprometido { get; set; }
    public DateTime? FechaVenceFactura { get; set; }
    public DateTime FechaPagoComprometida { get; set; }
}

public sealed class ResultadoEnvioTramiteCobroWebDTO
{
    public bool Enviado { get; set; }
    public string? Destino { get; set; }
    public string? Error { get; set; }
}
