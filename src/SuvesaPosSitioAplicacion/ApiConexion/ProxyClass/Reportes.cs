using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IReportes" />
public sealed class Reportes : ProxyBase, IReportes
{
    private readonly IReportesApiCliente _api;

    public Reportes(IReportesApiCliente api, IContextoSesion sesion, ILogger<Reportes> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<ReporteComprasDTO>>> Compras()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerReporterComprasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el reporte de compras");
}
