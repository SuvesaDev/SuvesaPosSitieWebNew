using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IInstrumentosConfirmacion
{
    Task<ResponseGeneric<ICollection<InstrumentoConfirmacionWebDTO>>> Buscar(FiltroInstrumentosConfirmacionWebDTO filtro);
    Task<ResponseGeneric<InstrumentoConfirmacionWebDTO>> CambiarEstado(long id, CambioEstadoInstrumentoWebDTO cambio);
    Task<ResponseGeneric<ResumenInstrumentosCajaWebDTO>> ResumenCaja(long apertura);
}
