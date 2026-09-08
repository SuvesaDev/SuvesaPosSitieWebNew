using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ImpuestosFiscales : ProxyBase, IImpuestosFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public ImpuestosFiscales(IHttpClientFactory clientes, IContextoSesion sesion, ILogger<ImpuestosFiscales> log) : base(sesion, log) => _api = clientes.CreateClient("SeePosApi");
    public Task<ResponseGeneric<ICollection<ImpuestoFiscalDTO>>> Obtener() => Ejecutar(async () => await LeerAsync<ICollection<ImpuestoFiscalDTO>>(await _api.PostAsync("impuesto/ObtenerImpuestos", null)), "consultar los impuestos");
    public Task<ResponseGeneric<ImpuestoFiscalDTO>> Crear(ImpuestoFiscalDTO impuesto) => Enviar("impuesto/Crear", impuesto, "crear el impuesto");
    public Task<ResponseGeneric<ImpuestoFiscalDTO>> Actualizar(ImpuestoFiscalDTO impuesto) => Enviar("impuesto/Actualizar", impuesto, "actualizar el impuesto");
    public Task<ResponseGeneric<ImpuestoFiscalDTO>> Deshabilitar(int idImpuesto, string? usuario) => Ejecutar(async () => await LeerAsync<ImpuestoFiscalDTO>(await _api.PostAsync($"impuesto/Deshabilitar?idImpuesto={idImpuesto}&usuario={Uri.EscapeDataString(usuario ?? string.Empty)}", null)), "deshabilitar el impuesto");
    private Task<ResponseGeneric<ImpuestoFiscalDTO>> Enviar(string ruta, ImpuestoFiscalDTO impuesto, string accion) => Ejecutar(async () => await LeerAsync<ImpuestoFiscalDTO>(await _api.PostAsJsonAsync(ruta, impuesto, Json)), accion);
    private static async Task<ResponseGeneric<T>> LeerAsync<T>(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); if (!respuesta.IsSuccessStatusCode) return new ResponseGeneric<T>($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); var e = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("El API devolvió una respuesta vacía."); return e.Status == 0 ? new ResponseGeneric<T>(e.Responses) : new ResponseGeneric<T>(e.CurrentException ?? "El API devolvió un error sin detalle.", e.ValidationErrors ?? Array.Empty<string>()); }
    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
