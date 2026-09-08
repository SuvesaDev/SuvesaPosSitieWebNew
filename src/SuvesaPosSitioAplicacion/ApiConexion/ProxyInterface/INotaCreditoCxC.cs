using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Invariantes de la nota de crédito sobre el mayor de CxC (SANEAMIENTO Fase 5).
/// </summary>
public interface INotaCreditoCxC
{
    /// <summary>
    /// Devuelve el IdSerie de la única serie de NC activa para (emisor, centro).
    /// Si hay 0 o más de una (viola D1), la respuesta trae el error.
    /// </summary>
    Task<ResponseGeneric<int>> SerieUnica(int idEmisor, int idSucursal);
}
