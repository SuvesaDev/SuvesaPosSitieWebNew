using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class CalendarioHabil : ProxyBase, ICalendarioHabil
{
    private readonly HttpClient _api;
    public CalendarioHabil(IHttpClientFactory factory, IContextoSesion sesion, ILogger<CalendarioHabil> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<CalendarioHabilAnioDTO>> ObtenerAnio(int anio)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CalendarioHabilAnioDTO>(
            await _api.GetAsync($"api/comercial/calendario/{anio}")), "consultar el calendario hábil");

    public Task<ResponseGeneric<DiaNoHabilDTO>> GuardarDia(DiaNoHabilDTO dia)
        => Ejecutar(async () => await LecturaEnvelope.Leer<DiaNoHabilDTO>(
            await _api.PutAsJsonAsync("api/comercial/calendario", dia, LecturaEnvelope.Json)), "guardar el día");

    public Task<ResponseGeneric<bool>> QuitarDia(DateTime fecha)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.DeleteAsync($"api/comercial/calendario/{fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}")), "quitar el día");

    public Task<ResponseGeneric<IReadOnlyList<DiaNoHabilDTO>>> GenerarFeriados(int anio)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<DiaNoHabilDTO>>(
            await _api.PostAsync($"api/comercial/calendario/{anio}/generar-feriados", null)), "generar los feriados");
}
