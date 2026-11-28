using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IImagenesArticulo
{
    Task<ResponseGeneric<ArticulosImagenesDTO>> Obtener(long idArticulo);
    Task<ResponseGeneric<ICollection<ArticulosImagenesCatalogoDTO>>> Catalogo();
    /// <summary>Descarta el caché del listado del catálogo visual (5 min, por sucursal) para
    /// que la próxima llamada a <see cref="Catalogo"/> vuelva a consultar al API — botón
    /// "Actualizar" del catálogo, para cuando se sabe que se subió/cambió una imagen.</summary>
    void InvalidarCatalogo();

    /// <summary>Botón "Recatalogar imágenes" (Inventario): genera la miniatura para las
    /// imágenes que se subieron antes de que existiera esa columna.</summary>
    Task<ResponseGeneric<RecatalogoImagenesResultadoDTO>> Recatalogar();
    Task<ResponseGeneric<ArticulosImagenesDTO>> Guardar(ArticulosImagenesDTO imagen);
    Task<ResponseGeneric<ArticulosImagenesDTO>> Actualizar(ArticulosImagenesDTO imagen);
    Task<ResponseGeneric<ArticulosImagenesDTO>> Eliminar(long idImagen);
}
