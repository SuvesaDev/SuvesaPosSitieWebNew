using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IImagenesArticulo
{
    Task<ResponseGeneric<ArticulosImagenesDTO>> Obtener(long idArticulo);
    Task<ResponseGeneric<ArticulosImagenesDTO>> Guardar(ArticulosImagenesDTO imagen);
    Task<ResponseGeneric<ArticulosImagenesDTO>> Eliminar(long idImagen);
}
