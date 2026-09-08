using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Carta de exoneración asociada a un cliente.</summary>
public interface ICartasExoneracion
{
    Task<ResponseGeneric<CartaExoneracionDTO>> Buscar(string cedula);

    Task<ResponseGeneric<CartaExoneracionDTO>> Crear(CartaExoneracionDTO carta);

    Task<ResponseGeneric<CartaExoneracionDTO>> Editar(CartaExoneracionDTO carta);
}
