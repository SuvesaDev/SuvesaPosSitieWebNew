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
}
