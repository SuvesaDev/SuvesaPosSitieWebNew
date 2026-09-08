using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IConfiguracion" />
public sealed class Configuracion : ProxyBase, IConfiguracion
{
    private readonly IConfiguracionCostaPetsApiCliente _api;

    public Configuracion(IConfiguracionCostaPetsApiCliente api, IContextoSesion sesion, ILogger<Configuracion> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ConfiguracionCostaPet>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerConfiguracionAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la configuracion");

    public Task<ResponseGeneric<ConfiguracionCostaPet>> Guardar(float porcentajeProntoPago)
        => Ejecutar(async () =>
        {
            var r = await _api.PutConfigurationAsync(porcentajeProntoPago);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la configuracion");
}
