using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Rutas;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class RutasComerciales : ProxyBase, IRutasComerciales
{
    private readonly HttpClient _api;
    public RutasComerciales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<RutasComerciales> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<RutaComercialDTO>>> Listar(int? idSucursal = null)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<RutaComercialDTO>>(
            await _api.GetAsync($"api/mantenimientos/rutas{(idSucursal is > 0 ? $"?idSucursal={idSucursal}" : string.Empty)}")), "consultar las rutas");

    public Task<ResponseGeneric<IReadOnlyList<RutaAgenteDTO>>> Agentes(int idRuta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<RutaAgenteDTO>>(
            await _api.GetAsync($"api/mantenimientos/rutas/{idRuta}/agentes")), "consultar los agentes de la ruta");

    public Task<ResponseGeneric<IReadOnlyList<RutaAgenteDTO>>> AgentesElegibles()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<RutaAgenteDTO>>(
            await _api.GetAsync("api/mantenimientos/rutas/agentes-elegibles")), "consultar los agentes elegibles");

    public Task<ResponseGeneric<CapacidadComercialDTO>> CapacidadComercial(string idUsuario)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CapacidadComercialDTO>(
            await _api.GetAsync($"api/mantenimientos/rutas/capacidad-comercial/{Uri.EscapeDataString(idUsuario)}")), "consultar la capacidad comercial del usuario");

    public Task<ResponseGeneric<RutaComercialDTO>> Crear(RutaComercialDTO ruta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<RutaComercialDTO>(
            await _api.PostAsJsonAsync("api/mantenimientos/rutas", ruta, LecturaEnvelope.Json)), "crear la ruta");

    public Task<ResponseGeneric<RutaComercialDTO>> Actualizar(int idRuta, RutaComercialDTO ruta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<RutaComercialDTO>(
            await _api.PutAsJsonAsync($"api/mantenimientos/rutas/{idRuta}", ruta, LecturaEnvelope.Json)), "actualizar la ruta");

    public Task<ResponseGeneric<bool>> Desactivar(int idRuta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/mantenimientos/rutas/{idRuta}")), "desactivar la ruta");
}
