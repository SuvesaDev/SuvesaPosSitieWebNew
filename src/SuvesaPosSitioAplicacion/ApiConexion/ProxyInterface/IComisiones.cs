using SuvesaPosSitioAplicacion.DTOs.Comisiones;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IComisiones
{
    Task<ResponseGeneric<ComisionConsultaDTO>> Consultar(FiltroComisionesDTO filtro);
    /// <summary>Lo que el corte liquidaría con ese rango, sin liquidar nada.</summary>
    Task<ResponseGeneric<PrevisualizacionCorteComisionesDTO>> PrevisualizarCorte(CrearCorteComisionesDTO corte);
    Task<ResponseGeneric<CorteComisionesDTO>> CrearCorte(CrearCorteComisionesDTO corte);
    Task<ResponseGeneric<ArchivoComisionesDTO>> ExportarXlsx(FiltroComisionesDTO filtro);
}
