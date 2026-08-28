using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IGeografia" />
public sealed class Geografia : ProxyBase, IGeografia
{
    private readonly IGeografiaApiCliente _api;

    public Geografia(
        IGeografiaApiCliente api,
        IContextoSesion sesion,
        ILogger<Geografia> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<ProvinciaDTO>>> Provincias()
        => Ejecutar(async () =>
        {
            var r = await _api.GetProvinciasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las provincias");

    public Task<ResponseGeneric<ICollection<CantonDTO>>> Cantones(int provincia)
        => Ejecutar(async () =>
        {
            var r = await _api.GetCantonAsync(provincia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los cantones");

    public Task<ResponseGeneric<ICollection<DistritoDTO>>> Distritos(int canton)
        => Ejecutar(async () =>
        {
            var r = await _api.GetDistritoAsync(canton);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los distritos");
}
