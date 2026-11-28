using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Comisiones;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class Comisiones : ProxyBase, IComisiones
{
    private readonly HttpClient _api;
    public Comisiones(IHttpClientFactory factory, IContextoSesion sesion, ILogger<Comisiones> log) : base(sesion, log) => _api = factory.CreateClient("SeePosApi");
    private static List<string> ArmarQuery(FiltroComisionesDTO f)
    {
        var q = new List<string>();
        if (f.Desde.HasValue) q.Add($"Desde={Uri.EscapeDataString(f.Desde.Value.ToString("O"))}");
        if (f.Hasta.HasValue) q.Add($"Hasta={Uri.EscapeDataString(f.Hasta.Value.ToString("O"))}");
        if (f.IdSucursal.HasValue) q.Add($"IdSucursal={f.IdSucursal}");
        if (!string.IsNullOrWhiteSpace(f.EstadoLiquidacion)) q.Add($"EstadoLiquidacion={Uri.EscapeDataString(f.EstadoLiquidacion)}");
        if (f.IdRutaComercial.HasValue) q.Add($"IdRutaComercial={f.IdRutaComercial}");
        if (!string.IsNullOrWhiteSpace(f.IdUsuarioBeneficiario)) q.Add($"IdUsuarioBeneficiario={Uri.EscapeDataString(f.IdUsuarioBeneficiario)}");
        if (f.IdCorte.HasValue) q.Add($"IdCorte={f.IdCorte}");
        return q;
    }

    public Task<ResponseGeneric<ComisionConsultaDTO>> Consultar(FiltroComisionesDTO f)
    {
        var q = ArmarQuery(f);
        var url = "api/comisiones" + (q.Count == 0 ? string.Empty : "?" + string.Join("&", q));
        return Ejecutar(async () => await LecturaEnvelope.Leer<ComisionConsultaDTO>(await _api.GetAsync(url)), "consultar las comisiones");
    }
    public Task<ResponseGeneric<CorteComisionesDTO>> CrearCorte(CrearCorteComisionesDTO corte)
        => Ejecutar(async () => await LecturaEnvelope.Leer<CorteComisionesDTO>(await _api.PostAsJsonAsync("api/comisiones/cortes", corte, LecturaEnvelope.Json)), "cerrar el período de comisiones");

    public Task<ResponseGeneric<ArchivoComisionesDTO>> ExportarXlsx(FiltroComisionesDTO f)
        => Ejecutar(async () =>
        {
            var q = new List<string> { "formato=xlsx" };
            q.AddRange(ArmarQuery(f));
            using var respuesta = await _api.GetAsync("api/comisiones/exportar?" + string.Join("&", q));
            if (!respuesta.IsSuccessStatusCode)
                return new ResponseGeneric<ArchivoComisionesDTO>($"No se pudo exportar el Excel ({(int)respuesta.StatusCode}).");
            var disposition = respuesta.Content.Headers.ContentDisposition;
            return new ResponseGeneric<ArchivoComisionesDTO>(new ArchivoComisionesDTO
            {
                Contenido = await respuesta.Content.ReadAsByteArrayAsync(),
                Nombre = disposition?.FileNameStar?.Trim('"') ?? disposition?.FileName?.Trim('"') ?? "comisiones.xlsx",
                TipoContenido = respuesta.Content.Headers.ContentType?.MediaType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            });
        }, "exportar las comisiones a Excel");
}
