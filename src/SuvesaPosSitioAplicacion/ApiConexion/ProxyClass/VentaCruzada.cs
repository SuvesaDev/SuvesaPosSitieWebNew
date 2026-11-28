using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class VentaCruzada : ProxyBase, IVentaCruzada
{
    private readonly HttpClient _api;
    public VentaCruzada(IHttpClientFactory factory, IContextoSesion sesion, ILogger<VentaCruzada> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<ReglaVentaCruzadaDTO>>> Reglas()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<ReglaVentaCruzadaDTO>>(
            await _api.GetAsync("api/comercial/venta-cruzada/reglas")), "consultar las reglas de venta cruzada");

    public Task<ResponseGeneric<ReglaVentaCruzadaDTO>> Crear(GuardarReglaVentaCruzadaRequest regla)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ReglaVentaCruzadaDTO>(
            await _api.PostAsJsonAsync("api/comercial/venta-cruzada/reglas", regla, LecturaEnvelope.Json)), "crear la regla");

    public Task<ResponseGeneric<ReglaVentaCruzadaDTO>> CambiarActivo(int id, bool activo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ReglaVentaCruzadaDTO>(
            await _api.PutAsJsonAsync($"api/comercial/venta-cruzada/reglas/{id}/activo", new CambiarActivoReglaRequest { Activo = activo }, LecturaEnvelope.Json)), "cambiar la regla");

    public Task<ResponseGeneric<bool>> Eliminar(int id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/comercial/venta-cruzada/reglas/{id}")), "eliminar la regla");

    public Task<ResponseGeneric<OportunidadesVentaCruzadaDTO>> Oportunidades(int? idRuta = null)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OportunidadesVentaCruzadaDTO>(
            await _api.GetAsync("api/comercial/venta-cruzada/oportunidades" + (idRuta is int r ? "?idRuta=" + r.ToString(CultureInfo.InvariantCulture) : string.Empty))), "consultar las oportunidades de venta cruzada");
}
