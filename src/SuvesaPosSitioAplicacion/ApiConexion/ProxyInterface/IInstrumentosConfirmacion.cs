using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IInstrumentosConfirmacion
{
    // Cada cajero ve y confirma solo los de sus cajas (clave interna); el administrador, cualquiera.
    Task<ResponseGeneric<ICollection<InstrumentoConfirmacionWebDTO>>> Buscar(FiltroInstrumentosConfirmacionWebDTO filtro, string? claveInterna = null);
    Task<ResponseGeneric<InstrumentoConfirmacionWebDTO>> CambiarEstado(long id, CambioEstadoInstrumentoWebDTO cambio, string? claveInterna = null);
    Task<ResponseGeneric<ResumenInstrumentosCajaWebDTO>> ResumenCaja(long apertura);
}
