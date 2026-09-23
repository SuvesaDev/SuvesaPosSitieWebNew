using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class Importaciones : ProxyBase, IImportaciones
{
    private readonly HttpClient _api;

    public Importaciones(IHttpClientFactory factory, IContextoSesion sesion, ILogger<Importaciones> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<ImportacionResumenWebDTO>>> Listar()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<ImportacionResumenWebDTO>>(
            await _api.GetAsync("api/importaciones")), "consultar las importaciones");

    public Task<ResponseGeneric<ReporteImportacionWebDTO>> Reporte(FiltroReporteImportacionWebDTO filtro)
        => Ejecutar(async () =>
        {
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtro.Dua)) q.Add($"dua={Uri.EscapeDataString(filtro.Dua)}");
            if (filtro.IdProveedor.HasValue) q.Add($"idProveedor={filtro.IdProveedor.Value}");
            if (filtro.Desde.HasValue) q.Add($"desde={filtro.Desde.Value:yyyy-MM-dd}");
            if (filtro.Hasta.HasValue) q.Add($"hasta={filtro.Hasta.Value:yyyy-MM-dd}");
            return await LecturaEnvelope.Leer<ReporteImportacionWebDTO>(await _api.GetAsync("api/importaciones/reporte" + (q.Count == 0 ? "" : "?" + string.Join("&", q))));
        }, "consultar el reporte de importaciones");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> Crear(CrearImportacionWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PostAsJsonAsync("api/importaciones", cmd, LecturaEnvelope.Json)), "crear la importación");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> AgregarCosto(long idImportacion, ImportacionCostoWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PostAsJsonAsync($"api/importaciones/{idImportacion}/costos", cmd, LecturaEnvelope.Json)), "agregar el costo de importación");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> ActualizarTributoDua(long idImportacion, ActualizarTributoDuaImportacionWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PutAsJsonAsync($"api/importaciones/{idImportacion}/tributo-dua", cmd, LecturaEnvelope.Json)), "actualizar el tributo DUA de la línea");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> AgregarDocumento(long idImportacion, ImportacionDocumentoCargaWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PostAsJsonAsync($"api/importaciones/{idImportacion}/documentos", cmd, LecturaEnvelope.Json)), "adjuntar el documento de importación");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> ActualizarEstadoFiscal(long idImportacion, long idDocumento, ActualizarEstadoFiscalImportacionWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PutAsJsonAsync($"api/importaciones/{idImportacion}/documentos/{idDocumento}/fiscal", cmd, LecturaEnvelope.Json)), "actualizar el estado fiscal");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> EnviarMensajeReceptor(long idImportacion, long idDocumento)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PostAsync($"api/importaciones/{idImportacion}/documentos/{idDocumento}/mensaje-receptor", null)), "enviar el Mensaje Receptor");

    public Task<ResponseGeneric<ImportacionResumenWebDTO>> Cerrar(long idImportacion, CerrarImportacionWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ImportacionResumenWebDTO>(
            await _api.PostAsJsonAsync($"api/importaciones/{idImportacion}/cerrar", cmd, LecturaEnvelope.Json)), "cerrar la importación");
}
