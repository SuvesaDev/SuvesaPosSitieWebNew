using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.DTOs.Fiscal;

// Contratos a mano de los comandos atómicos de facturación del API
// (api/facturacion/{preventas/contado,creditos,tiquetes}) y del estado de cuenta
// (api/cobros/estado-cuenta/{idCliente}). PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md W3/W4/W6.
// Espejo de los esquemas publicados por el API; nombres JSON en camelCase (Web defaults).

/// <summary>Sobre común de los tres comandos: clave de idempotencia + la venta.</summary>
public sealed class ComandoFacturacionDTO
{
    public string? ClaveIdempotencia { get; set; }
    public FacturaDTO Venta { get; set; } = new();
}

/// <summary>Resultado uniforme de un comando de facturación.</summary>
public sealed class ResultadoOperacionFacturacionDTO
{
    public long IdVenta { get; set; }
    public string? NumeroOperativo { get; set; }
    public double Total { get; set; }
    /// <summary>PreventaPendiente | Confirmada | ... — estado comercial.</summary>
    public string? EstadoComercial { get; set; }
    /// <summary>SinPago | Pagada | Parcial — estado del cobro.</summary>
    public string? EstadoPago { get; set; }
    /// <summary>NoAplica | Pendiente | Aceptado | Rechazado — estado fiscal.</summary>
    public string? EstadoFiscal { get; set; }
    public int? IdSerie { get; set; }
    public System.DateTime? Vencimiento { get; set; }
    public double SaldoPendiente { get; set; }
    /// <summary>El API devolvió una respuesta ya guardada para esta clave de idempotencia.</summary>
    public bool FueReintento { get; set; }
}

/// <summary>Factura de crédito con saldo abierto (detalle del estado de cuenta).</summary>
public sealed class FacturaCreditoConSaldoDTO
{
    public long IdVenta { get; set; }
    public double NumFactura { get; set; }
    public string? ConsecutivoMh { get; set; }
    public string? ClaveMh { get; set; }
    public System.DateTime Fecha { get; set; }
    public System.DateTime? Vence { get; set; }
    public int CodMoneda { get; set; }
    public double MontoOriginal { get; set; }
    public double NotasCreditoAplicadas { get; set; }
    public double PagosAplicados { get; set; }
    public double SaldoActual { get; set; }
    public string? EstadoMh { get; set; }
}

/// <summary>Estado de cuenta del cliente a una fecha de corte, con antigüedad de saldos.</summary>
public sealed class EstadoCuentaClienteDTO
{
    public long IdCliente { get; set; }
    public string? Nombre { get; set; }
    public System.DateTime FechaCorte { get; set; }
    public int CodMonedaBase { get; set; }
    public double LimiteAprobado { get; set; }
    public double SaldoTotal { get; set; }
    public double CreditoAFavor { get; set; }
    public double Disponible { get; set; }
    public double PorVencer { get; set; }
    public double Vencido1a30 { get; set; }
    public double Vencido31a60 { get; set; }
    public double Vencido61a90 { get; set; }
    public double Vencido91oMas { get; set; }
    public List<FacturaCreditoConSaldoDTO> Detalle { get; set; } = new();
}
