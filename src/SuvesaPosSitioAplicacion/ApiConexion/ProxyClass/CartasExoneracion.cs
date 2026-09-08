using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICartasExoneracion" />
public sealed class CartasExoneracion : ProxyBase, ICartasExoneracion
{
    private readonly ICartaExoneracionApiCliente _api;

    public CartasExoneracion(
        ICartaExoneracionApiCliente api,
        IContextoSesion sesion,
        ILogger<CartasExoneracion> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<CartaExoneracionDTO>> Buscar(string cedula)
        => Ejecutar(async () =>
        {
            var r = await _api.BuscarCartaAsync(new BuscarCartaExoneracionDTO { Cedula = cedula });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la carta de exoneración");

    public Task<ResponseGeneric<CartaExoneracionDTO>> Crear(CartaExoneracionDTO carta)
        => Ejecutar(async () =>
        {
            var r = await _api.CartaExoneracionAsync(carta);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la carta de exoneración");

    public Task<ResponseGeneric<CartaExoneracionDTO>> Editar(CartaExoneracionDTO carta)
        => Ejecutar(async () =>
        {
            var r = await _api.ActualizarAsync(carta);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la carta de exoneración");
}
