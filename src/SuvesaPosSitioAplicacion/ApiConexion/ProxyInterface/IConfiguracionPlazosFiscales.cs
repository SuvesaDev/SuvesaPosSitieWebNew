using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IConfiguracionPlazosFiscales
{
    Task<ResponseGeneric<ICollection<ConfiguracionPlazoFiscalDTO>>> Obtener();
    Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Crear(ConfiguracionPlazoFiscalDTO plazo);
    Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Actualizar(ConfiguracionPlazoFiscalDTO plazo);
    Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Deshabilitar(ConfiguracionPlazoFiscalDTO plazo);
}
