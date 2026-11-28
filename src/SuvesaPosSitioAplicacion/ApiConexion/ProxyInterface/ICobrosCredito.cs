using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Cobro de facturas de crédito con mayor de CxC y recibo atómico
/// (SANEAMIENTO Fase 2 / Fase 8). Un comando por operación.
/// </summary>
public interface ICobrosCredito
{
    Task<ResponseGeneric<CreditoClienteWebDTO>> Credito(long idCliente);

    Task<ResponseGeneric<ICollection<FacturaCreditoWebDTO>>> Facturas(long idCliente);

    Task<ResponseGeneric<CobroCreditoResultadoWebDTO>> Cobrar(CobroCreditoComandoWebDTO comando);

    Task<ResponseGeneric<bool>> Anular(long idCobro, string? motivo);

    /// <summary>Descuento por pronto pago de cada factura de crédito con saldo del cliente.</summary>
    Task<ResponseGeneric<ICollection<EvaluacionProntoPagoWebDTO>>> ProntoPago(long idCliente);

    /// <summary>Aplica el crédito a favor del cliente a una de sus facturas de crédito (con recibo).</summary>
    Task<ResponseGeneric<AplicarCreditoAFavorResultadoWebDTO>> AplicarCreditoAFavor(AplicarCreditoAFavorWebDTO comando);

    /// <summary>Devuelve en efectivo el crédito a favor del cliente (salida de caja).</summary>
    Task<ResponseGeneric<DevolverCreditoAFavorResultadoWebDTO>> DevolverCreditoAFavor(DevolverCreditoAFavorWebDTO comando);
}
