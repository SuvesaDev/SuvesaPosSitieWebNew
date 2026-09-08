using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
public sealed class GeografiaFiscal : ProxyBase, IGeografiaFiscal
{
    private readonly HttpClient _api; private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public GeografiaFiscal(IHttpClientFactory factory, IContextoSesion sesion, ILogger<GeografiaFiscal> logger) : base(sesion, logger) => _api=factory.CreateClient("SeePosApi");
    public Task<ResponseGeneric<ICollection<ProvinciaFiscalDTO>>> Provincias()=>Ejecutar(async()=>await Leer<ICollection<ProvinciaFiscalDTO>>(await _api.GetAsync("Geografia/Mantenimiento/Provincias")),"consultar las provincias");
    public Task<ResponseGeneric<ICollection<CantonFiscalDTO>>> Cantones(int? id)=>Ejecutar(async()=>await Leer<ICollection<CantonFiscalDTO>>(await _api.GetAsync($"Geografia/Mantenimiento/Cantones{(id.HasValue ? $"?idProvincia={id}" : string.Empty)}")),"consultar los cantones");
    public Task<ResponseGeneric<ICollection<DistritoFiscalDTO>>> Distritos(int? id)=>Ejecutar(async()=>await Leer<ICollection<DistritoFiscalDTO>>(await _api.GetAsync($"Geografia/Mantenimiento/Distritos{(id.HasValue ? $"?idCanton={id}" : string.Empty)}")),"consultar los distritos");
    public Task<ResponseGeneric<ProvinciaFiscalDTO>> Crear(ProvinciaFiscalDTO x)=>Enviar("Geografia/CrearProvincia",x,"crear la provincia"); public Task<ResponseGeneric<ProvinciaFiscalDTO>> Actualizar(ProvinciaFiscalDTO x)=>Enviar("Geografia/ActualizarProvincia",x,"actualizar la provincia"); public Task<ResponseGeneric<CantonFiscalDTO>> Crear(CantonFiscalDTO x)=>Enviar("Geografia/CrearCanton",x,"crear el cantón"); public Task<ResponseGeneric<CantonFiscalDTO>> Actualizar(CantonFiscalDTO x)=>Enviar("Geografia/ActualizarCanton",x,"actualizar el cantón"); public Task<ResponseGeneric<DistritoFiscalDTO>> Crear(DistritoFiscalDTO x)=>Enviar("Geografia/CrearDistrito",x,"crear el distrito"); public Task<ResponseGeneric<DistritoFiscalDTO>> Actualizar(DistritoFiscalDTO x)=>Enviar("Geografia/ActualizarDistrito",x,"actualizar el distrito");
    private Task<ResponseGeneric<T>> Enviar<T>(string ruta,T item,string accion)=>Ejecutar(async()=>await Leer<T>(await _api.PostAsJsonAsync(ruta,item,Json)),accion);
    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage r){var b=await r.Content.ReadAsStringAsync();if(!r.IsSuccessStatusCode)return new($"El API respondió {(int)r.StatusCode}: {b}");var e=JsonSerializer.Deserialize<Envelope<T>>(b,Json)??throw new InvalidOperationException("Respuesta vacía.");return e.Status==0?new(e.Responses):new(e.CurrentException??"Error sin detalle.",e.ValidationErrors??Array.Empty<string>());} private sealed class Envelope<T>{public int Status{get;init;}public string? CurrentException{get;init;}public IReadOnlyList<string>? ValidationErrors{get;init;}public T? Responses{get;init;}}
}
