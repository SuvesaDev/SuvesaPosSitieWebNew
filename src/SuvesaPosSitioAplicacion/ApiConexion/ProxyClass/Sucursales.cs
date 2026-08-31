using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ISucursales" />
public sealed class Sucursales : ProxyBase, ISucursales
{
    private readonly IIdentificacionApiCliente _identificacion;
    private readonly ICentrosApiCliente _centros;
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Sucursales(
        IIdentificacionApiCliente identificacion,
        ICentrosApiCliente centros,
        IContextoSesion sesion,
        ILogger<Sucursales> log,
        IHttpClientFactory factory)
        : base(sesion, log)
    {
        _identificacion = identificacion;
        _centros = centros;
        _api = factory.CreateClient("SeePosApi");
    }

    public Task<ResponseGeneric<ICollection<TipoIdentificacionDTO>>> TiposIdentificacion()
        => Ejecutar(async () =>
        {
            var r = await _identificacion.ObtenerAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los tipos de documento");

    public Task<ResponseGeneric<SucursalFiscalDTO>> Crear(SucursalFiscalDTO sucursal)
        => Ejecutar(async () => await Leer<SucursalFiscalDTO>(await _api.PostAsJsonAsync("Centros/crearSucursal", sucursal, Json)), "crear la sucursal");

    public Task<ResponseGeneric<ICollection<SucursalFiscalDTO>>> Obtener()
        => Ejecutar(async () => await Leer<ICollection<SucursalFiscalDTO>>(await _api.PostAsync("Centros/ObtenerSucursal", null)), "consultar las sucursales");

    public Task<ResponseGeneric<SucursalFiscalDTO>> Actualizar(SucursalFiscalDTO sucursal)
        => Ejecutar(async () => await Leer<SucursalFiscalDTO>(await _api.PostAsJsonAsync("Centros/ActualizarSucursal", sucursal, Json)), "actualizar la sucursal");

    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");
        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía.");
        return envelope.Status == 0 ? new(envelope.Responses) : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
