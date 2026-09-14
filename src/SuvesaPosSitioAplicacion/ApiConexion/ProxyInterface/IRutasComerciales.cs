using SuvesaPosSitioAplicacion.DTOs.Rutas;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IRutasComerciales
{
    Task<ResponseGeneric<IReadOnlyList<RutaComercialDTO>>> Listar(int? idSucursal = null);
    Task<ResponseGeneric<IReadOnlyList<RutaAgenteDTO>>> Agentes(int idRuta);
    Task<ResponseGeneric<IReadOnlyList<RutaAgenteDTO>>> AgentesElegibles();
    Task<ResponseGeneric<CapacidadComercialDTO>> CapacidadComercial(string idUsuario);
    Task<ResponseGeneric<RutaComercialDTO>> Crear(RutaComercialDTO ruta);
    Task<ResponseGeneric<RutaComercialDTO>> Actualizar(int idRuta, RutaComercialDTO ruta);
    Task<ResponseGeneric<bool>> Desactivar(int idRuta);
}
