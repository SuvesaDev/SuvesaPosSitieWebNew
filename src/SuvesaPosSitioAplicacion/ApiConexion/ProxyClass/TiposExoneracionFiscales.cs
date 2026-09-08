using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
public sealed class TiposExoneracionFiscales : ProxyBase, ITiposExoneracionFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public TiposExoneracionFiscales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<TiposExoneracionFiscales> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<TipoExoneracionFiscalDTO>>> Obtener() =>
        Ejecutar(async () => await Leer<ICollection<TipoExoneracionFiscalDTO>>(await _api.GetAsync("TipoExoneracion")), "consultar los tipos de exoneración");

    public Task<ResponseGeneric<TipoExoneracionFiscalDTO>> Crear(TipoExoneracionFiscalDTO tipo) =>
        Enviar("TipoExoneracion/Crear", tipo, "crear el tipo de exoneración");

    public Task<ResponseGeneric<TipoExoneracionFiscalDTO>> Actualizar(TipoExoneracionFiscalDTO tipo) =>
        Enviar("TipoExoneracion/Actualizar", tipo, "actualizar el tipo de exoneración");

    private Task<ResponseGeneric<TipoExoneracionFiscalDTO>> Enviar(string ruta, TipoExoneracionFiscalDTO tipo, string accion) =>
        Ejecutar(async () => await Leer<TipoExoneracionFiscalDTO>(await _api.PostAsJsonAsync(ruta, tipo, Json)), accion);

    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");

        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json)
            ?? throw new InvalidOperationException("Respuesta vacía.");
        return envelope.Status == 0
            ? new(envelope.Responses)
            : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T>
    {
        public int Status { get; init; }
        public string? CurrentException { get; init; }
        public IReadOnlyList<string>? ValidationErrors { get; init; }
        public T? Responses { get; init; }
    }
}
