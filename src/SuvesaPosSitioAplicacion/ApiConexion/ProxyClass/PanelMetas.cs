using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class PanelMetas : ProxyBase, IPanelMetas
{
    private readonly HttpClient _api;
    public PanelMetas(IHttpClientFactory factory, IContextoSesion sesion, ILogger<PanelMetas> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<PanelMetasDTO>> Consultar(int anio, int mes)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PanelMetasDTO>(
            await _api.GetAsync($"api/comercial/panel-metas?anio={anio}&mes={mes}")), "consultar el panel de metas");

    public Task<ResponseGeneric<IReadOnlyList<ColaboradorAtribucionDTO>>> ListarAtribucion(int idRuta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<ColaboradorAtribucionDTO>>(
            await _api.GetAsync($"api/comercial/atribucion-sac/{idRuta}")), "consultar la atribución de ventas");

    public Task<ResponseGeneric<ColaboradorAtribucionDTO>> GuardarAtribucion(int idRuta, string idUsuario, bool sumaAMetaRuta)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ColaboradorAtribucionDTO>(
            await _api.PutAsJsonAsync($"api/comercial/atribucion-sac/{idRuta}/{Uri.EscapeDataString(idUsuario)}",
                new GuardarAtribucionRequest { SumaAMetaRuta = sumaAMetaRuta }, LecturaEnvelope.Json)), "guardar la atribución de ventas");
}
