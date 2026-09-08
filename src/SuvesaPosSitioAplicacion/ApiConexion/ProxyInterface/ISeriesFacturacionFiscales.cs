using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface ISeriesFacturacionFiscales
{
    Task<ResponseGeneric<ICollection<SerieFacturacionFiscalDTO>>> Obtener();
    Task<ResponseGeneric<SeriesFacturacionCatalogosFiscalDTO>> Catalogos();
    Task<ResponseGeneric<SerieFacturacionFiscalDTO>> Crear(SerieFacturacionFiscalDTO serie);
    Task<ResponseGeneric<SerieFacturacionFiscalDTO>> Actualizar(SerieFacturacionFiscalDTO serie);
}
