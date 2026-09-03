namespace SuvesaPosSitioAplicacion.DTOs.Ventas;

/// <summary>Panorama de crédito del cliente (SANEAMIENTO Fase 2).</summary>
public sealed class CreditoClienteWebDTO
{
    public long IdCliente { get; set; }
    public string? Nombre { get; set; }
    public decimal LimiteAprobado { get; set; }
    public int PlazoDias { get; set; }
    public decimal SaldoAbierto { get; set; }
    public decimal CreditoAFavor { get; set; }
    public decimal Disponible { get; set; }
    public bool Bloqueado { get; set; }
    public string? MotivoBloqueo { get; set; }
}

/// <summary>Factura de crédito con saldo abierto.</summary>
public sealed class FacturaCreditoWebDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public string? ConsecutivoMh { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? Vence { get; set; }
    public int CodMoneda { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal NotasCreditoAplicadas { get; set; }
    public decimal PagosAplicados { get; set; }
    public decimal SaldoActual { get; set; }
    public string? EstadoMh { get; set; }
}

/// <summary>Comando de cobro de facturas de crédito.</summary>
public sealed class CobroCreditoComandoWebDTO
{
    public string? ClaveIdempotencia { get; set; }
    public long IdCliente { get; set; }
    public long IdApertura { get; set; }
    public int IdSucursal { get; set; }
    public long? NumCaja { get; set; }
    public string? CedulaCajero { get; set; }
    public string Usuario { get; set; } = "";
    public List<CobroCreditoFacturaWebDTO> Facturas { get; set; } = new();
    public List<CobroCreditoFormaPagoWebDTO> FormasPago { get; set; } = new();
    public bool PermitirParcial { get; set; } = true;
}

public sealed class CobroCreditoFacturaWebDTO
{
    public long IdVenta { get; set; }
    public decimal? Monto { get; set; }
}

public sealed class CobroCreditoFormaPagoWebDTO
{
    public string CodigoFormaPago { get; set; } = "";
    public decimal MontoRecibido { get; set; }
    public string? Referencia { get; set; }
}

public sealed class CobroCreditoResultadoWebDTO
{
    public long IdCobro { get; set; }
    public long NumeroRecibo { get; set; }
    public decimal TotalAplicado { get; set; }
    public decimal Vuelto { get; set; }
    public bool FueReintento { get; set; }
}
