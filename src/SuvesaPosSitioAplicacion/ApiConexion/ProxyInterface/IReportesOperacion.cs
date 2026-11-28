using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consultas reales de reporte, separadas del contrato NSwag histórico.</summary>
public interface IReportesOperacion
{
    Task<ResponseGeneric<ReporteOperacionWebDTO>> Consultar(string reporte, FiltroReporteOperacionWebDTO filtro);
}
