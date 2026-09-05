using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IEmisoresFiscales
{
    Task<ResponseGeneric<ICollection<EmisorFiscalDTO>>> Obtener();
    Task<ResponseGeneric<EmisorFiscalDTO>> Actualizar(EmisorFiscalDTO emisor);
    Task<ResponseGeneric<bool>> ActualizarCredenciales(CredencialesHaciendaFiscalDTO credenciales);
    Task<ResponseGeneric<EmisorLogoResumenDTO>> LogoMetadata(int idEmisor);
    Task<ResponseGeneric<EmisorLogoArchivoDTO>> DescargarLogo(int idEmisor);
    Task<ResponseGeneric<EmisorLogoResumenDTO>> GuardarLogo(int idEmisor, EmisorLogoActualizarDTO logo);
    Task<ResponseGeneric<bool>> EliminarLogo(int idEmisor);
}
