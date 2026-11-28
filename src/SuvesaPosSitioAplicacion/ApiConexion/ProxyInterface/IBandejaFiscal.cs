using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface IBandejaFiscal { Task<ResponseGeneric<ResultadoBandejaFiscalDTO>> Consultar(FiltroBandejaFiscalDTO filtro); Task<ResponseGeneric<DetalleBandejaFiscalDTO>> Detalle(string clave); Task<ResponseGeneric<string>> XmlFirmado(string clave); Task<ResponseGeneric<string>> RespuestaHacienda(string clave); Task<ResponseGeneric<bool>> Reintentar(string clave); Task<ResponseGeneric<bool>> ConsultarEstado(string clave);
    /// <summary>Reemite el comprobante RECHAZADO de una venta (clave y consecutivo nuevos; no toca inventario ni cuentas por cobrar).</summary>
    Task<ResponseGeneric<ReemisionComprobanteWeb>> Reemitir(long idVenta); }
