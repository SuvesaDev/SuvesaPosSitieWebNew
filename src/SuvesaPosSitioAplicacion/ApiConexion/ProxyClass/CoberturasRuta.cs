using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class CoberturasRuta : ProxyBase, ICoberturasRuta
{
    private readonly HttpClient _api;
    public CoberturasRuta(IHttpClientFactory factory, IContextoSesion sesion, ILogger<CoberturasRuta> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<CoberturaRutaDTO>>> Listar(FiltroCoberturasDTO filtro)
    {
        // El API asume "solo vigentes" si no se le dice nada, así que los dos valores del interruptor se envían siempre.
        var consulta = $"?SoloVigentes={(filtro.SoloVigentes ? "true" : "false")}&IncluirAnuladas={(filtro.IncluirAnuladas ? "true" : "false")}";
        if (filtro.IdRuta is int idRuta) consulta += $"&IdRuta={idRuta.ToString(CultureInfo.InvariantCulture)}";
        return Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<CoberturaRutaDTO>>(
            await _api.GetAsync("api/comercial/coberturas" + consulta)), "consultar las coberturas");
    }

    public Task<ResponseGeneric<IReadOnlyList<UsuarioCubridorDTO>>> UsuariosElegibles()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<UsuarioCubridorDTO>>(
            await _api.GetAsync("api/comercial/coberturas/usuarios-elegibles")), "consultar quién puede cubrir");

    public Task<ResponseGeneric<CoberturaRutaDTO>> Crear(CoberturaRutaDTO cobertura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CoberturaRutaDTO>(
            await _api.PostAsJsonAsync("api/comercial/coberturas", cobertura, LecturaEnvelope.Json)), "crear la cobertura");

    public Task<ResponseGeneric<CoberturaRutaDTO>> Actualizar(long idCobertura, CoberturaRutaDTO cobertura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CoberturaRutaDTO>(
            await _api.PutAsJsonAsync($"api/comercial/coberturas/{idCobertura}", cobertura, LecturaEnvelope.Json)), "actualizar la cobertura");

    public Task<ResponseGeneric<bool>> Anular(long idCobertura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/comercial/coberturas/{idCobertura}")), "anular la cobertura");
}
