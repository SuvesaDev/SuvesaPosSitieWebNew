using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Reportes de consulta.</summary>
public interface IReportes
{
    Task<ResponseGeneric<ICollection<ReporteComprasDTO>>> Compras();
}
