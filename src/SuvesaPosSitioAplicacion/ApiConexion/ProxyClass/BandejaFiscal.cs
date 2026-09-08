using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
public sealed class BandejaFiscal : ProxyBase, IBandejaFiscal
{
    private readonly HttpClient _api; private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public BandejaFiscal(IHttpClientFactory factory, IContextoSesion sesion, ILogger<BandejaFiscal> logger) : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");
    public Task<ResponseGeneric<ResultadoBandejaFiscalDTO>> Consultar(FiltroBandejaFiscalDTO f) => Ejecutar(async () => await LeerJson<ResultadoBandejaFiscalDTO>(await _api.GetAsync($"api/comprobantes-electronicos/v44/bandeja?clave={Uri.EscapeDataString(f.Clave ?? string.Empty)}&estado={Uri.EscapeDataString(f.Estado ?? string.Empty)}&pagina={f.Pagina}&tamanoPagina={f.TamanoPagina}")), "consultar la bandeja fiscal");
    public Task<ResponseGeneric<DetalleBandejaFiscalDTO>> Detalle(string clave) => Ejecutar(async () => await LeerJson<DetalleBandejaFiscalDTO>(await _api.GetAsync($"api/comprobantes-electronicos/v44/bandeja/{Uri.EscapeDataString(clave)}")), "consultar el detalle fiscal");
    public Task<ResponseGeneric<string>> XmlFirmado(string clave) => Ejecutar(async () => await LeerTexto(await _api.GetAsync($"api/comprobantes-electronicos/v44/bandeja/{Uri.EscapeDataString(clave)}/xml-firmado")), "obtener el XML firmado");
    public Task<ResponseGeneric<string>> RespuestaHacienda(string clave) => Ejecutar(async () => await LeerTexto(await _api.GetAsync($"api/comprobantes-electronicos/v44/bandeja/{Uri.EscapeDataString(clave)}/respuesta-hacienda")), "obtener la respuesta de Hacienda");
    public Task<ResponseGeneric<bool>> Reintentar(string clave) => Ejecutar(async () => { var respuesta = await _api.PostAsync($"api/comprobantes-electronicos/v44/bandeja/{Uri.EscapeDataString(clave)}/reintentar", null); return respuesta.IsSuccessStatusCode ? new ResponseGeneric<bool>(true) : new ResponseGeneric<bool>($"El API respondió {(int)respuesta.StatusCode}: {await respuesta.Content.ReadAsStringAsync()}"); }, "reintentar la emisión fiscal");
    public Task<ResponseGeneric<bool>> ConsultarEstado(string clave) => Ejecutar(async () => { var respuesta = await _api.PostAsync($"api/comprobantes-electronicos/v44/emisiones/{Uri.EscapeDataString(clave)}/consultar-hacienda", null); return respuesta.IsSuccessStatusCode ? new ResponseGeneric<bool>(true) : new ResponseGeneric<bool>($"El API respondió {(int)respuesta.StatusCode}: {await respuesta.Content.ReadAsStringAsync()}"); }, "consultar el estado en Hacienda");
    private static async Task<ResponseGeneric<T>> LeerJson<T>(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); return new(JsonSerializer.Deserialize<T>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía.")); }
    private static async Task<ResponseGeneric<string>> LeerTexto(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); return respuesta.IsSuccessStatusCode ? new(cuerpo) : new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); }
}
