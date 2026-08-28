using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ImagenesArticulo : ProxyBase, IImagenesArticulo
{
    private readonly IArticulosImagenesApiCliente _api;
    public ImagenesArticulo(IArticulosImagenesApiCliente api, IContextoSesion sesion, ILogger<ImagenesArticulo> log) : base(sesion, log) => _api = api;
    public Task<ResponseGeneric<ArticulosImagenesDTO>> Obtener(long idArticulo) => Ejecutar(async () => { var r = await _api.ObtenerArticuloImagenAsync(idArticulo); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "consultar la imagen del artículo");
    public Task<ResponseGeneric<ArticulosImagenesDTO>> Guardar(ArticulosImagenesDTO imagen) => Ejecutar(async () => { var r = await _api.InsertarArticuloImagenAsync(imagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "guardar la imagen del artículo");
    public Task<ResponseGeneric<ArticulosImagenesDTO>> Eliminar(long idImagen) => Ejecutar(async () => { var r = await _api.EliminarArticuloImagenAsync(idImagen); return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses); }, "eliminar la imagen del artículo");
}
