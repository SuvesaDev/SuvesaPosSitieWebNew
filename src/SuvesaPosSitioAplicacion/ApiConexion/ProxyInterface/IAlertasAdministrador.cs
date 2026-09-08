using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Alertas del administrador (MOTOR_CORREO_COMPROBANTES_WEB.md §5).</summary>
public interface IAlertasAdministrador
{
    Task<ResponseGeneric<PaginaAlertasAdministradorDTO>> Listar(bool soloNoLeidas, int? idEmisor, int pagina, int tamano);

    Task<ResponseGeneric<int>> Conteo(int? idEmisor);

    Task<ResponseGeneric<bool>> MarcarLeida(long id);

    /// <summary>Marca todas las no leídas en una sola llamada; devuelve cuántas se marcaron.</summary>
    Task<ResponseGeneric<int>> MarcarTodasLeidas(int? idEmisor = null);
}
