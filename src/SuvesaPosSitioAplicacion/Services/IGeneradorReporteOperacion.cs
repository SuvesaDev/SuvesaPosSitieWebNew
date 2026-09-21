using SuvesaPosSitioAplicacion.DTOs.Reportes;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>Genera las versiones PDF y Excel de un reporte operativo ya consultado.</summary>
public interface IGeneradorReporteOperacion
{
    byte[] Pdf(string tipoReporte, ReporteOperacionWebDTO reporte, DateTime generadoEnEquipo);
    byte[] Excel(string tipoReporte, ReporteOperacionWebDTO reporte, DateTime generadoEnEquipo);
}
