using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Panel de metas (cumplimiento, proyección, cartera vencida) y atribución de las ventas de Servicio al Cliente.</summary>
public interface IPanelMetas
{
    Task<ResponseGeneric<PanelMetasDTO>> Consultar(int anio, int mes);
    Task<ResponseGeneric<IReadOnlyList<ColaboradorAtribucionDTO>>> ListarAtribucion(int idRuta);
    Task<ResponseGeneric<ColaboradorAtribucionDTO>> GuardarAtribucion(int idRuta, string idUsuario, bool sumaAMetaRuta);
}
