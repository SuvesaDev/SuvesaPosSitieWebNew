using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Umbrales y topes configurables del módulo comercial (alertas, cartera vencida, faltantes).</summary>
public interface IParametrosComerciales
{
    Task<ResponseGeneric<IReadOnlyList<ParametroComercialDTO>>> Listar();
    Task<ResponseGeneric<ParametroComercialDTO>> Actualizar(string clave, decimal? valor);
    Task<ResponseGeneric<IReadOnlyList<TopeCarteraRutaDTO>>> ListarTopesRuta();
    Task<ResponseGeneric<TopeCarteraRutaDTO>> GuardarTopeRuta(int idRuta, decimal porcentaje);
    Task<ResponseGeneric<bool>> QuitarTopeRuta(int idRuta);
}
