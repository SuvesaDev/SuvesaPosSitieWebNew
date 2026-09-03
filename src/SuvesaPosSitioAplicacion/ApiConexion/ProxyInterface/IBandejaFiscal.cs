using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface IBandejaFiscal { Task<ResponseGeneric<ResultadoBandejaFiscalDTO>> Consultar(FiltroBandejaFiscalDTO filtro); Task<ResponseGeneric<DetalleBandejaFiscalDTO>> Detalle(string clave); Task<ResponseGeneric<string>> XmlFirmado(string clave); Task<ResponseGeneric<string>> RespuestaHacienda(string clave); Task<ResponseGeneric<bool>> Reintentar(string clave); Task<ResponseGeneric<bool>> ConsultarEstado(string clave); }
