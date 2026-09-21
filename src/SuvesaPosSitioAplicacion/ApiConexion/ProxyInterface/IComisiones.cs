using SuvesaPosSitioAplicacion.DTOs.Comisiones;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IComisiones
{
    Task<ResponseGeneric<ComisionConsultaDTO>> Consultar(FiltroComisionesDTO filtro);
    Task<ResponseGeneric<CorteComisionesDTO>> CrearCorte(CrearCorteComisionesDTO corte);
    Task<ResponseGeneric<ArchivoComisionesDTO>> ExportarXlsx(FiltroComisionesDTO filtro);
}
