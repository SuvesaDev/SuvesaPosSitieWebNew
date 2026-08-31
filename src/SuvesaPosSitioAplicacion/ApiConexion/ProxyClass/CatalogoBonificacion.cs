using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICatalogoBonificacion" />
public sealed class CatalogoBonificacion : ProxyBase, ICatalogoBonificacion
{
    private readonly IConfiguracionBonificacionApiCliente _api;

    public CatalogoBonificacion(IConfiguracionBonificacionApiCliente api, IContextoSesion sesion, ILogger<CatalogoBonificacion> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<ConfiguracionBonificacion>>> Disponibles()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerConfiguracionesDisponiblesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los tipos de bonificación");
}
