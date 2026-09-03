using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Consignacion;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Cliente HTTP a mano para el módulo de consignación (CONSIGNACION_API.md §3).
/// Mismo patrón que <see cref="LotesApiCliente"/> / <see cref="ProduccionApiCliente"/>:
/// <see cref="LoteEnvelope{T}"/>, y setea <c>ContextoLlamada.Token</c> él mismo
/// porque se llama directo desde componentes. TEMPORAL: se borra al regenerar NSwag.
/// </summary>
public interface IConsignacionInvApiCliente
{
    Task<LoteEnvelope<BodegaConsignacionResumen>> AbrirBodegaAsync(AbrirBodegaConsignacion req);
    Task<LoteEnvelope<List<BodegaConsignacionResumen>>> BodegasAsync(BodegasConsignacionFiltro req);

    Task<LoteEnvelope<BoletaConsignacion>> RegistrarBoletaAsync(BoletaConsignacionRequest req);
    Task<LoteEnvelope<BoletaConsignacion>> BoletaAsync(long id);
    Task<LoteEnvelope<BoletaConsignacion>> AnularBoletaAsync(AnularBoletaConsignacion req);

    Task<LoteEnvelope<ConteoConsignacion>> RegistrarConteoAsync(ConteoConsignacionRequest req);
    Task<LoteEnvelope<ConteoConsignacion>> ConteoAsync(long id);

    Task<LoteEnvelope<KardexConsignacion>> KardexAsync(KardexConsignacionFiltro req);

    Task<LoteEnvelope<PrefacturaConsignacion>> GenerarPrefacturaAsync(GenerarPrefacturaConsignacion req);
    Task<LoteEnvelope<PrefacturaConsignacion>> EditarPrefacturaAsync(EditarPrefacturaConsignacion req);
    Task<LoteEnvelope<PrefacturaConsignacion>> AprobarPrefacturaAsync(long idPrefactura);
    Task<LoteEnvelope<PrefacturaConsignacion>> FacturarPrefacturaAsync(FacturarPrefacturaConsignacion req);
    Task<LoteEnvelope<PrefacturaConsignacion>> AnularPrefacturaAsync(AnularPrefacturaConsignacion req);
    Task<LoteEnvelope<PrefacturaConsignacion>> PrefacturaAsync(long id);
    Task<LoteEnvelope<List<PrefacturaConsignacionResumen>>> PrefacturasAsync(PrefacturasConsignacionFiltro req);
}

public sealed class ConsignacionInvApiCliente : IConsignacionInvApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly IContextoSesion _sesion;

    public ConsignacionInvApiCliente(HttpClient http, IContextoSesion sesion)
    {
        _http = http;
        _sesion = sesion;
    }

    private async Task<LoteEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        await _sesion.CargarAsync();
        ContextoLlamada.Token = _sesion.Token;
        try
        {
            using var req = new HttpRequestMessage(metodo, ruta);
            if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, options: Json);
            using var resp = await _http.SendAsync(req);
            var texto = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(texto))
                return new() { Status = ResponseStatus._1, CurrentException = $"El API respondió {(int)resp.StatusCode} sin cuerpo." };
            try
            {
                return JsonSerializer.Deserialize<LoteEnvelope<T>>(texto, Json)
                       ?? new() { Status = ResponseStatus._1, CurrentException = "Respuesta vacía." };
            }
            catch (JsonException)
            {
                return new() { Status = ResponseStatus._1, CurrentException = $"Respuesta no reconocida del API ({(int)resp.StatusCode})." };
            }
        }
        finally
        {
            ContextoLlamada.Token = null;
        }
    }

    public Task<LoteEnvelope<BodegaConsignacionResumen>> AbrirBodegaAsync(AbrirBodegaConsignacion req)
        => EnviarAsync<BodegaConsignacionResumen>(HttpMethod.Post, "ConsignacionInventario/AbrirBodega", req);

    public Task<LoteEnvelope<List<BodegaConsignacionResumen>>> BodegasAsync(BodegasConsignacionFiltro req)
        => EnviarAsync<List<BodegaConsignacionResumen>>(HttpMethod.Post, "ConsignacionInventario/Bodegas", req);

    public Task<LoteEnvelope<BoletaConsignacion>> RegistrarBoletaAsync(BoletaConsignacionRequest req)
        => EnviarAsync<BoletaConsignacion>(HttpMethod.Post, "ConsignacionInventario/RegistrarBoleta", req);

    public Task<LoteEnvelope<BoletaConsignacion>> BoletaAsync(long id)
        => EnviarAsync<BoletaConsignacion>(HttpMethod.Get, $"ConsignacionInventario/Boleta?id={id}");

    public Task<LoteEnvelope<BoletaConsignacion>> AnularBoletaAsync(AnularBoletaConsignacion req)
        => EnviarAsync<BoletaConsignacion>(HttpMethod.Post, "ConsignacionInventario/AnularBoleta", req);

    public Task<LoteEnvelope<ConteoConsignacion>> RegistrarConteoAsync(ConteoConsignacionRequest req)
        => EnviarAsync<ConteoConsignacion>(HttpMethod.Post, "ConsignacionInventario/RegistrarConteo", req);

    public Task<LoteEnvelope<ConteoConsignacion>> ConteoAsync(long id)
        => EnviarAsync<ConteoConsignacion>(HttpMethod.Get, $"ConsignacionInventario/Conteo?id={id}");

    public Task<LoteEnvelope<KardexConsignacion>> KardexAsync(KardexConsignacionFiltro req)
        => EnviarAsync<KardexConsignacion>(HttpMethod.Post, "ConsignacionInventario/Kardex", req);

    public Task<LoteEnvelope<PrefacturaConsignacion>> GenerarPrefacturaAsync(GenerarPrefacturaConsignacion req)
        => EnviarAsync<PrefacturaConsignacion>(HttpMethod.Post, "ConsignacionInventario/GenerarPrefactura", req);

    public Task<LoteEnvelope<PrefacturaConsignacion>> EditarPrefacturaAsync(EditarPrefacturaConsignacion req)
        => EnviarAsync<PrefacturaConsignacion>(HttpMethod.Put, "ConsignacionInventario/EditarPrefactura", req);

    public Task<LoteEnvelope<PrefacturaConsignacion>> AprobarPrefacturaAsync(long idPrefactura)
        => EnviarAsync<PrefacturaConsignacion>(HttpMethod.Post, $"ConsignacionInventario/AprobarPrefactura?idPrefactura={idPrefactura}");

    public Task<LoteEnvelope<PrefacturaConsignacion>> FacturarPrefacturaAsync(FacturarPrefacturaConsignacion req)
        => EnviarAsync<PrefacturaConsignacion>(HttpMethod.Post, "ConsignacionInventario/FacturarPrefactura", req);

    public Task<LoteEnvelope<PrefacturaConsignacion>> AnularPrefacturaAsync(AnularPrefacturaConsignacion req)
        => EnviarAsync<PrefacturaConsignacion>(HttpMethod.Post, "ConsignacionInventario/AnularPrefactura", req);

    public Task<LoteEnvelope<PrefacturaConsignacion>> PrefacturaAsync(long id)
        => EnviarAsync<PrefacturaConsignacion>(HttpMethod.Get, $"ConsignacionInventario/Prefactura?id={id}");

    public Task<LoteEnvelope<List<PrefacturaConsignacionResumen>>> PrefacturasAsync(PrefacturasConsignacionFiltro req)
        => EnviarAsync<List<PrefacturaConsignacionResumen>>(HttpMethod.Post, "ConsignacionInventario/Prefacturas", req);
}
