using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IEmisoresFiscales
{
    Task<ResponseGeneric<ICollection<EmisorFiscalDTO>>> Obtener();
    Task<ResponseGeneric<EmisorFiscalDTO>> Actualizar(EmisorFiscalDTO emisor);
    Task<ResponseGeneric<bool>> ActualizarCredenciales(CredencialesHaciendaFiscalDTO credenciales);
}
