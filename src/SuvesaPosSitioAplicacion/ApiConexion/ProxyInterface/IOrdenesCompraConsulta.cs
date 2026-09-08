using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta de pedidos de compra. No crea, anula ni modifica pedidos.</summary>
public interface IOrdenesCompraConsulta
{
    /// <summary>Obtiene los pedidos recientes publicados por el API.</summary>
    Task<ResponseGeneric<ICollection<OrdenCompraDTO>>> Obtener();

    /// <summary>Busca un pedido por su numero, opcionalmente incluyendo anulados.</summary>
    Task<ResponseGeneric<ICollection<OrdenCompraDTO>>> Buscar(long numero, bool anuladas);
}
