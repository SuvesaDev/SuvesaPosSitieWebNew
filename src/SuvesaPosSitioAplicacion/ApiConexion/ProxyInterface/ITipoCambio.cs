using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Tipo de cambio oficial CRC/USD (SANEAMIENTO §6 / D9).</summary>
public interface ITipoCambio
{
    Task<ResponseGeneric<TipoCambioOficialDTO>> Oficial(DateTime? fecha = null);
}
