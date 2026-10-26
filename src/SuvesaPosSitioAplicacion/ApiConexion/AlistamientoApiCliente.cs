using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Bodega;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Cliente HTTP a mano para alistamiento / empaque / despacho.
/// TEMPORAL: se borra al regenerar NSwag.
/// </summary>
public interface IAlistamientoApiCliente
{
    Task<LoteEnvelope<List<AlistamientoResumen>>> ColaAsync(AlistamientoFiltro filtro);
    Task<LoteEnvelope<AlistamientoOrden>> OrdenAsync(long id);
    Task<LoteEnvelope<AlistamientoOrden>> AsegurarAsync(long idVenta, bool forzar, string? numeroDocumento = null);
    Task<LoteEnvelope<AlistamientoOrden>> AceptarAsync(long idOrden);
    Task<LoteEnvelope<AlistamientoOrden>> EscanearAsync(long idOrden, string codigo, long? idBulto, bool manual = false, string? claveSupervisor = null);
    Task<LoteEnvelope<AlistamientoOrden>> CerrarBultoAsync(long idOrden, long? idBulto, decimal pesoKg);
    Task<LoteEnvelope<AlistamientoOrden>> EmpacarAsync(long idOrden, bool autorizarFaltante, string? motivo);
    Task<LoteEnvelope<AlistamientoOrden>> FaltanteAsync(long idOrden);
    Task<LoteEnvelope<AlistamientoOrden>> AnularAsync(long idOrden, string motivo);
    Task<LoteEnvelope<AlistamientoOrden>> EntregarAsync(long idOrden, string? recibidoPor, bool noEntregado, bool merma = false, string? observaciones = null, string? firmaPng = null);

    Task<LoteEnvelope<List<RutaDespacho>>> RutasAsync(bool soloActivas = false);
    Task<LoteEnvelope<RutaDespacho>> GuardarRutaAsync(RutaDespacho dto);
    Task<LoteEnvelope<RutaDespacho>> ActualizarRutaAsync(RutaDespacho dto);
    Task<LoteEnvelope<List<VehiculoDespacho>>> VehiculosAsync();
    Task<LoteEnvelope<VehiculoDespacho>> GuardarVehiculoAsync(VehiculoDespacho dto);
    Task<LoteEnvelope<List<ChoferDespacho>>> ChoferesAsync();
    Task<LoteEnvelope<ChoferDespacho>> GuardarChoferAsync(ChoferDespacho dto);

    Task<LoteEnvelope<List<BultoDespacho>>> BultosEmpacadosAsync();
    Task<LoteEnvelope<List<ManifiestoDespacho>>> ManifiestosAsync();
    Task<LoteEnvelope<ManifiestoDespacho>> ManifiestoAsync(long id);
    Task<LoteEnvelope<ManifiestoDespacho>> CrearManifiestoAsync(int? idRuta, int? idVehiculo, int? idChofer, List<long> idsBultos);
    Task<LoteEnvelope<ManifiestoDespacho>> CargarBultoAsync(long idManifiesto, string codigoBulto);
    Task<LoteEnvelope<ManifiestoDespacho>> SalirRutaAsync(long idManifiesto);
    Task<LoteEnvelope<ManifiestoDespacho>> EditarManifiestoAsync(long idManifiesto, int? idRuta, int? idVehiculo, int? idChofer, string? observaciones);
    Task<LoteEnvelope<ManifiestoDespacho>> QuitarBultoAsync(long idManifiesto, long idBulto);
    Task<LoteEnvelope<ManifiestoDespacho>> AnularManifiestoAsync(long idManifiesto, string motivo);
}

public sealed class AlistamientoApiCliente : IAlistamientoApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly IContextoSesion _sesion;

    public AlistamientoApiCliente(HttpClient http, IContextoSesion sesion)
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
                var envelope = JsonSerializer.Deserialize<LoteEnvelope<T>>(texto, Json)
                       ?? new() { Status = ResponseStatus._1, CurrentException = "Respuesta vacía." };
                if (!resp.IsSuccessStatusCode && envelope.Status == ResponseStatus._0)
                {
                    envelope.Status = ResponseStatus._1;
                    envelope.CurrentException ??= $"El API respondió {(int)resp.StatusCode}.";
                }
                return envelope;
            }
            catch (JsonException ex)
            {
                var denegado = (int)resp.StatusCode == 403
                    ? "No tiene permiso para esta operación de bodega."
                    : $"Respuesta no reconocida del API ({(int)resp.StatusCode}): {ex.Message}";
                return new() { Status = ResponseStatus._1, CurrentException = denegado };
            }
        }
        catch (Exception ex)
        {
            return new() { Status = ResponseStatus._1, CurrentException = ex.Message };
        }
        finally
        {
            ContextoLlamada.Token = null;
        }
    }

    public Task<LoteEnvelope<List<AlistamientoResumen>>> ColaAsync(AlistamientoFiltro filtro)
        => EnviarAsync<List<AlistamientoResumen>>(HttpMethod.Post, "Alistamiento/Cola", filtro);

    public Task<LoteEnvelope<AlistamientoOrden>> OrdenAsync(long id)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Get, $"Alistamiento/Orden?id={id}");

    public Task<LoteEnvelope<AlistamientoOrden>> AsegurarAsync(long idVenta, bool forzar, string? numeroDocumento = null)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Asegurar", new { idVenta, forzar, numeroDocumento });

    public Task<LoteEnvelope<AlistamientoOrden>> AceptarAsync(long idOrden)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Aceptar", new { idOrden });

    public Task<LoteEnvelope<AlistamientoOrden>> EscanearAsync(long idOrden, string codigo, long? idBulto, bool manual = false, string? claveSupervisor = null)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Escanear", new { idOrden, codigo, idBulto, manual, claveSupervisor });

    public Task<LoteEnvelope<AlistamientoOrden>> CerrarBultoAsync(long idOrden, long? idBulto, decimal pesoKg)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/CerrarBulto", new { idOrden, idBulto, pesoKg });

    public Task<LoteEnvelope<AlistamientoOrden>> EmpacarAsync(long idOrden, bool autorizarFaltante, string? motivo)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Empacar", new { idOrden, autorizarFaltante, motivoFaltante = motivo });

    public Task<LoteEnvelope<AlistamientoOrden>> FaltanteAsync(long idOrden)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Faltante", new { idOrden });

    public Task<LoteEnvelope<AlistamientoOrden>> AnularAsync(long idOrden, string motivo)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Anular", new { idOrden, motivo });

    public Task<LoteEnvelope<AlistamientoOrden>> EntregarAsync(long idOrden, string? recibidoPor, bool noEntregado, bool merma = false, string? observaciones = null, string? firmaPng = null)
        => EnviarAsync<AlistamientoOrden>(HttpMethod.Post, "Alistamiento/Entregar", new { idOrden, recibidoPor, noEntregado, merma, observaciones, firmaPng });

    public Task<LoteEnvelope<List<RutaDespacho>>> RutasAsync(bool soloActivas = false)
        => EnviarAsync<List<RutaDespacho>>(HttpMethod.Get, soloActivas ? "Alistamiento/Rutas?soloActivas=true" : "Alistamiento/Rutas");

    public Task<LoteEnvelope<RutaDespacho>> GuardarRutaAsync(RutaDespacho dto)
        => EnviarAsync<RutaDespacho>(HttpMethod.Post, "Alistamiento/GuardarRuta", dto);

    public Task<LoteEnvelope<RutaDespacho>> ActualizarRutaAsync(RutaDespacho dto)
        => EnviarAsync<RutaDespacho>(HttpMethod.Post, "Alistamiento/ActualizarRuta", dto);

    public Task<LoteEnvelope<List<VehiculoDespacho>>> VehiculosAsync()
        => EnviarAsync<List<VehiculoDespacho>>(HttpMethod.Get, "Alistamiento/Vehiculos");

    public Task<LoteEnvelope<VehiculoDespacho>> GuardarVehiculoAsync(VehiculoDespacho dto)
        => EnviarAsync<VehiculoDespacho>(HttpMethod.Post, "Alistamiento/GuardarVehiculo", dto);

    public Task<LoteEnvelope<List<ChoferDespacho>>> ChoferesAsync()
        => EnviarAsync<List<ChoferDespacho>>(HttpMethod.Get, "Alistamiento/Choferes");

    public Task<LoteEnvelope<ChoferDespacho>> GuardarChoferAsync(ChoferDespacho dto)
        => EnviarAsync<ChoferDespacho>(HttpMethod.Post, "Alistamiento/GuardarChofer", dto);

    public Task<LoteEnvelope<List<BultoDespacho>>> BultosEmpacadosAsync()
        => EnviarAsync<List<BultoDespacho>>(HttpMethod.Get, "Alistamiento/BultosEmpacados");

    public Task<LoteEnvelope<List<ManifiestoDespacho>>> ManifiestosAsync()
        => EnviarAsync<List<ManifiestoDespacho>>(HttpMethod.Get, "Alistamiento/Manifiestos");

    public Task<LoteEnvelope<ManifiestoDespacho>> ManifiestoAsync(long id)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Get, $"Alistamiento/Manifiesto?id={id}");

    public Task<LoteEnvelope<ManifiestoDespacho>> CrearManifiestoAsync(int? idRuta, int? idVehiculo, int? idChofer, List<long> idsBultos)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Post, "Alistamiento/CrearManifiesto",
            new { idRutaDespacho = idRuta, idVehiculo, idChofer, idsBultos });

    public Task<LoteEnvelope<ManifiestoDespacho>> CargarBultoAsync(long idManifiesto, string codigoBulto)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Post, "Alistamiento/CargarBulto", new { idManifiesto, codigoBulto });

    public Task<LoteEnvelope<ManifiestoDespacho>> SalirRutaAsync(long idManifiesto)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Post, "Alistamiento/SalirRuta", new { idManifiesto });

    public Task<LoteEnvelope<ManifiestoDespacho>> EditarManifiestoAsync(long idManifiesto, int? idRuta, int? idVehiculo, int? idChofer, string? observaciones)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Post, "Alistamiento/EditarManifiesto",
            new { idManifiesto, idRutaDespacho = idRuta, idVehiculo, idChofer, observaciones });

    public Task<LoteEnvelope<ManifiestoDespacho>> QuitarBultoAsync(long idManifiesto, long idBulto)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Post, "Alistamiento/QuitarBulto", new { idManifiesto, idBulto });

    public Task<LoteEnvelope<ManifiestoDespacho>> AnularManifiestoAsync(long idManifiesto, string motivo)
        => EnviarAsync<ManifiestoDespacho>(HttpMethod.Post, "Alistamiento/AnularManifiesto", new { idManifiesto, motivo });
}
