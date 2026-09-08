using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IImagenesArticulo
{
    Task<ResponseGeneric<ArticulosImagenesDTO>> Obtener(long idArticulo);
    Task<ResponseGeneric<ICollection<ArticulosImagenesCatalogoDTO>>> Catalogo();
    Task<ResponseGeneric<ArticulosImagenesDTO>> Guardar(ArticulosImagenesDTO imagen);
    Task<ResponseGeneric<ArticulosImagenesDTO>> Actualizar(ArticulosImagenesDTO imagen);
    Task<ResponseGeneric<ArticulosImagenesDTO>> Eliminar(long idImagen);
}
