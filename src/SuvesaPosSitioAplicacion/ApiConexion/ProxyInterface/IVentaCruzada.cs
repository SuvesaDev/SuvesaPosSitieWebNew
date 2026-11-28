using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Reglas de venta cruzada (qué se complementa con qué) y las oportunidades que de ellas salen.</summary>
public interface IVentaCruzada
{
    Task<ResponseGeneric<IReadOnlyList<ReglaVentaCruzadaDTO>>> Reglas();
    Task<ResponseGeneric<ReglaVentaCruzadaDTO>> Crear(GuardarReglaVentaCruzadaRequest regla);
    Task<ResponseGeneric<ReglaVentaCruzadaDTO>> CambiarActivo(int id, bool activo);
    Task<ResponseGeneric<bool>> Eliminar(int id);
    Task<ResponseGeneric<OportunidadesVentaCruzadaDTO>> Oportunidades(int? idRuta = null);
}
