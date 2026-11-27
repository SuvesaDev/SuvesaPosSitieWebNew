using System.Globalization;
using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class AlertasComerciales : ProxyBase, IAlertasComerciales
{
    private readonly HttpClient _api;
    public AlertasComerciales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<AlertasComerciales> log) : base(sesion, log)
        => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<PaginaAlertasComercialesDTO>> Bandeja(FiltroAlertasComercialesDTO filtro)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PaginaAlertasComercialesDTO>(
            await _api.GetAsync("api/comercial/alertas" + Consulta(filtro))), "consultar las alertas");

    public Task<ResponseGeneric<ResumenAlertasComercialesDTO>> Resumen()
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResumenAlertasComercialesDTO>(
            await _api.GetAsync("api/comercial/alertas/resumen")), "consultar el resumen de alertas");

    public Task<ResponseGeneric<AlertaComercialDTO>> Gestionar(long idAlerta, string? nota)
        => Ejecutar(async () => await LecturaEnvelope.Leer<AlertaComercialDTO>(
            await _api.PutAsJsonAsync($"api/comercial/alertas/{idAlerta}/gestionar", new GestionarAlertaComercialRequest { Nota = nota }, LecturaEnvelope.Json)), "gestionar la alerta");

    public Task<ResponseGeneric<PaginaAlertasComercialesDTO>> Supervision(FiltroAlertasComercialesDTO filtro)
        => Ejecutar(async () => await LecturaEnvelope.Leer<PaginaAlertasComercialesDTO>(
            await _api.GetAsync("api/comercial/alertas/supervision" + Consulta(filtro))), "consultar la supervisión de alertas");

    public Task<ResponseGeneric<AlertaComercialDTO>> GestionarComoSupervisor(long idAlerta, string? nota)
        => Ejecutar(async () => await LecturaEnvelope.Leer<AlertaComercialDTO>(
            await _api.PutAsJsonAsync($"api/comercial/alertas/supervision/{idAlerta}/gestionar", new GestionarAlertaComercialRequest { Nota = nota }, LecturaEnvelope.Json)), "gestionar la alerta");

    public Task<ResponseGeneric<ResultadoGeneracionAlertasDTO>> GenerarAhora()
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResultadoGeneracionAlertasDTO>(
            await _api.PostAsync("api/comercial/alertas/supervision/generar", null)), "generar las alertas");

    private static string Consulta(FiltroAlertasComercialesDTO f)
    {
        var q = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.Estado)) q.Add($"Estado={Uri.EscapeDataString(f.Estado)}");
        if (!string.IsNullOrWhiteSpace(f.Tipo)) q.Add($"Tipo={Uri.EscapeDataString(f.Tipo)}");
        q.Add($"Pagina={Math.Max(1, f.Pagina).ToString(CultureInfo.InvariantCulture)}");
        q.Add($"Tamano={Math.Clamp(f.Tamano, 1, 100).ToString(CultureInfo.InvariantCulture)}");
        return "?" + string.Join("&", q);
    }
}
