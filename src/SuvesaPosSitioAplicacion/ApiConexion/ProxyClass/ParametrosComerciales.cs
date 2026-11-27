using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ParametrosComerciales : ProxyBase, IParametrosComerciales
{
    private readonly HttpClient _api;
    public ParametrosComerciales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ParametrosComerciales> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<ParametroComercialDTO>>> Listar()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<ParametroComercialDTO>>(
            await _api.GetAsync("api/comercial/parametros")), "consultar los parámetros comerciales");

    public Task<ResponseGeneric<ParametroComercialDTO>> Actualizar(string clave, decimal? valor)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ParametroComercialDTO>(
            await _api.PutAsJsonAsync($"api/comercial/parametros/{Uri.EscapeDataString(clave)}",
                new ActualizarParametroComercialRequest { Valor = valor }, LecturaEnvelope.Json)), "guardar el parámetro");

    public Task<ResponseGeneric<IReadOnlyList<TopeCarteraRutaDTO>>> ListarTopesRuta()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<TopeCarteraRutaDTO>>(
            await _api.GetAsync("api/comercial/parametros/topes-ruta")), "consultar los topes por ruta");

    public Task<ResponseGeneric<TopeCarteraRutaDTO>> GuardarTopeRuta(int idRuta, decimal porcentaje)
        => Ejecutar(async () => await LecturaEnvelope.Leer<TopeCarteraRutaDTO>(
            await _api.PutAsJsonAsync($"api/comercial/parametros/topes-ruta/{idRuta}",
                new TopeCarteraRutaDTO { IdRuta = idRuta, Porcentaje = porcentaje }, LecturaEnvelope.Json)), "guardar el tope de la ruta");

    public Task<ResponseGeneric<bool>> QuitarTopeRuta(int idRuta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/comercial/parametros/topes-ruta/{idRuta}")), "quitar el tope de la ruta");
}
