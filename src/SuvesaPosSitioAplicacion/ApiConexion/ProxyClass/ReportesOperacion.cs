using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ReportesOperacion : ProxyBase, IReportesOperacion
{
    private readonly HttpClient _api;
    public ReportesOperacion(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ReportesOperacion> logger) : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ReporteOperacionWebDTO>> Consultar(string reporte, FiltroReporteOperacionWebDTO f)
    {
        var q = new List<string>();
        if (f.Desde.HasValue) q.Add($"desde={f.Desde.Value:yyyy-MM-dd}");
        if (f.Hasta.HasValue) q.Add($"hasta={f.Hasta.Value:yyyy-MM-dd}");
        if (f.FechaReferencia.HasValue) q.Add($"fechaReferencia={f.FechaReferencia.Value:yyyy-MM-ddTHH:mm:ss}");
        if (f.IdSucursal.HasValue) q.Add($"idSucursal={f.IdSucursal}");
        if (f.IdEmpresa.HasValue) q.Add($"idEmpresa={f.IdEmpresa}");
        if (f.IdCliente.HasValue) q.Add($"idCliente={f.IdCliente}");
        if (f.IdProveedor.HasValue) q.Add($"idProveedor={f.IdProveedor}");
        if (f.IdArticulo.HasValue) q.Add($"idArticulo={f.IdArticulo}");
        if (!string.IsNullOrWhiteSpace(f.Texto)) q.Add($"texto={Uri.EscapeDataString(f.Texto)}");
        q.Add($"pagina={Math.Max(1, f.Pagina)}");
        q.Add($"tamanoPagina={Math.Clamp(f.TamanoPagina, 1, 2000)}");
        var url = $"api/reportes-operacion/{Uri.EscapeDataString(reporte)}{(q.Count == 0 ? string.Empty : "?" + string.Join("&", q))}";
        return Ejecutar(async () => await LecturaEnvelope.Leer<ReporteOperacionWebDTO>(await _api.GetAsync(url)), "consultar el reporte");
    }
}
