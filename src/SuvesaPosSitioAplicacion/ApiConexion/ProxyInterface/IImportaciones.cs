using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IImportaciones
{
    Task<ResponseGeneric<IReadOnlyList<ImportacionResumenWebDTO>>> Listar();
    Task<ResponseGeneric<ImportacionResumenWebDTO>> Obtener(long idImportacion);
    Task<ResponseGeneric<ResultadoBandejaImportacionesWebDTO>> Bandeja(FiltroBandejaImportacionesWebDTO filtro);
    Task<ResponseGeneric<ResultadoExtraccionFacturaPdfWebDTO>> ExtraerPdf(ExtraerLineasFacturaPdfWebDTO cmd);
    Task<ResponseGeneric<ReporteImportacionWebDTO>> Reporte(FiltroReporteImportacionWebDTO filtro);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> Crear(CrearImportacionWebDTO cmd);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> AgregarCosto(long idImportacion, ImportacionCostoWebDTO cmd);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> ActualizarTributoDua(long idImportacion, ActualizarTributoDuaImportacionWebDTO cmd);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> AgregarDocumento(long idImportacion, ImportacionDocumentoCargaWebDTO cmd);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> ActualizarEstadoFiscal(long idImportacion, long idDocumento, ActualizarEstadoFiscalImportacionWebDTO cmd);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> EnviarMensajeReceptor(long idImportacion, long idDocumento);
    Task<ResponseGeneric<ImportacionResumenWebDTO>> Cerrar(long idImportacion, CerrarImportacionWebDTO cmd);
}
