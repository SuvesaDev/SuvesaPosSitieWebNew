using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Comandos atómicos de facturación del API (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md W3/W4).
/// Cada uno es una sola llamada idempotente: la pantalla no encadena Cobrar → Facturar →
/// Emitir. La misma <paramref name="claveIdempotencia"/> debe reutilizarse ante un reintento
/// real (recarga, timeout) para no cobrar/emitir dos veces.
/// </summary>
public interface IComandosFacturacion
{
    /// <summary>No tiquete + contado: crea la preventa pendiente. Sin recibo ni emisión.</summary>
    Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> CrearPreventaContado(FacturaDTO venta, string? claveIdempotencia);

    /// <summary>No tiquete + crédito: confirma la factura con vencimiento y saldo CxC.</summary>
    Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> ConfirmarVentaCredito(FacturaDTO venta, string? claveIdempotencia);

    /// <summary>Tiquete: cobra el 100% y confirma en la misma operación.</summary>
    Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> CobrarVentaTiquete(FacturaDTO venta, string? claveIdempotencia);

    /// <summary>W5: cobra y factura una preventa de contado existente en una sola llamada
    /// idempotente (reemplaza Cobrar → FacturarPreventa → Emitir de la vista).</summary>
    Task<ResponseGeneric<FacturarPreventaContadoResultadoDTO>> FacturarPreventaContado(FacturarPreventaContadoComandoDTO comando);

    /// <summary>Estado de cuenta del cliente a una fecha de corte (null = hoy).</summary>
    Task<ResponseGeneric<EstadoCuentaClienteDTO>> EstadoCuenta(long idCliente, DateTime? corte = null);
}
