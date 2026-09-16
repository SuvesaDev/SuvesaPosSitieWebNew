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
    // "Recatalogar" es un endpoint nuevo, todavía sin cliente generado (ver nota en
    // DTOs/Generated/SeePosDtos.cs sobre por qué esta sesión no pudo regenerar contra un
    // API vivo) — se llama a mano, mismo patrón que ApiConexion/ProxyClass/RutasComerciales.cs.
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly IContextoSesion _sesion;

    public ImagenesArticulo(
        IArticulosImagenesApiCliente api,
        IHttpClientFactory factory,
        IMemoryCache cache,
        IContextoSesion sesion,
        ILogger<ImagenesArticulo> log) : base(sesion, log)
    {
        _api = api;
        _http = factory.CreateClient("SeePosApi");
        _cache = cache;
        _sesion = sesion;
    }

    public Task<ResponseGeneric<ArticulosImagenesDTO>> Obtener(long idArticulo) => Ejecutar(async () => { var r = await _api.ObtenerArticuloImagenAsync(idArticulo); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "consultar la imagen del artículo");

    // Refresco oportunista, sin cron ni hora fija: la llave incluye el día de hoy, así
    // que el primer request del día (de cualquier cajero o vendedor de la sucursal, en
    // cualquier dispositivo) es el único que paga el costo de ir al API — normalmente
    // eso ocurre temprano, con buena señal (en la tienda, antes de salir a la calle), no
    // a media venta con el cliente delante. El resto del día reutiliza ese mismo
    // resultado; nadie vuelve a pagar el costo hasta que cambie la fecha (o alguien
    // presione "Actualizar", ver InvalidarCatalogo). AbsoluteExpiration es solo higiene de
    // memoria para las llaves de días viejos, no dispara ningún refresco por sí sola.
    private string LlaveCatalogo => $"catalogo-imagenes:sucursal:{_sesion.IdSucursal}:dia:{DateTime.UtcNow:yyyyMMdd}";

    public void InvalidarCatalogo() => _cache.Remove(LlaveCatalogo);

    public Task<ResponseGeneric<ICollection<ArticulosImagenesCatalogoDTO>>> Catalogo()
        => Ejecutar(async () =>
        {
            var llave = LlaveCatalogo;
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
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(30),
                });
            }
            return respuesta;
        }, "consultar el catálogo visual de artículos");

    public Task<ResponseGeneric<RecatalogoImagenesResultadoDTO>> Recatalogar()
        => Ejecutar(async () => await LecturaEnvelope.Leer<RecatalogoImagenesResultadoDTO>(
            await _http.PostAsync("ArticulosImagenes/RecatalogarImagenes", null)), "recatalogar las imágenes");

    public Task<ResponseGeneric<ArticulosImagenesDTO>> Guardar(ArticulosImagenesDTO imagen) => Ejecutar(async () => { var r = await _api.InsertarArticuloImagenAsync(imagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "guardar la imagen del artículo");
    public Task<ResponseGeneric<ArticulosImagenesDTO>> Actualizar(ArticulosImagenesDTO imagen) => Ejecutar(async () => { var r = await _api.ActualizarImagenArticuloAsync(imagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "actualizar la imagen del artículo");
    public Task<ResponseGeneric<ArticulosImagenesDTO>> Eliminar(long idImagen) => Ejecutar(async () => { var r = await _api.EliminarArticuloImagenAsync(idImagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "eliminar la imagen del artículo");
}
