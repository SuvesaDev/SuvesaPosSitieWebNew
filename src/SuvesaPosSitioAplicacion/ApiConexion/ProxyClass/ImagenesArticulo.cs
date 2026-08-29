using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
using Microsoft.Extensions.Caching.Memory;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ImagenesArticulo : ProxyBase, IImagenesArticulo
{
    private readonly IArticulosImagenesApiCliente _api;
    private readonly IMemoryCache _cache;
    private readonly IContextoSesion _sesion;

    public ImagenesArticulo(
        IArticulosImagenesApiCliente api,
        IMemoryCache cache,
        IContextoSesion sesion,
        ILogger<ImagenesArticulo> log) : base(sesion, log)
    {
        _api = api;
        _cache = cache;
        _sesion = sesion;
    }

    public Task<ResponseGeneric<ArticulosImagenesDTO>> Obtener(long idArticulo) => Ejecutar(async () => { var r = await _api.ObtenerArticuloImagenAsync(idArticulo); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "consultar la imagen del artículo");

    public Task<ResponseGeneric<ICollection<ArticulosImagenesCatalogoDTO>>> Catalogo()
        => Ejecutar(async () =>
        {
            // El endpoint devuelve todas las imágenes base64 de una vez. Guardarlo
            // brevemente evita repetir una descarga muy grande al abrir/cerrar el
            // catálogo o cuando varios cajeros usan la misma sucursal.
            var llave = $"catalogo-imagenes:sucursal:{_sesion.IdSucursal}";
            if (_cache.TryGetValue(llave, out ICollection<ArticulosImagenesCatalogoDTO>? catalogo))
            {
                return new ResponseGeneric<ICollection<ArticulosImagenesCatalogoDTO>>(catalogo);
            }

            var r = await _api.ObtenerArticulosImagenesDisponiblesCatalogoAsync();
            var respuesta = EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
            if (respuesta.EsCorrecta && respuesta.Responses is not null)
            {
                _cache.Set(llave, respuesta.Responses, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                });
            }
            return respuesta;
        }, "consultar el catálogo visual de artículos");

    public Task<ResponseGeneric<ArticulosImagenesDTO>> Guardar(ArticulosImagenesDTO imagen) => Ejecutar(async () => { var r = await _api.InsertarArticuloImagenAsync(imagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "guardar la imagen del artículo");
    public Task<ResponseGeneric<ArticulosImagenesDTO>> Eliminar(long idImagen) => Ejecutar(async () => { var r = await _api.EliminarArticuloImagenAsync(idImagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "eliminar la imagen del artículo");
}
