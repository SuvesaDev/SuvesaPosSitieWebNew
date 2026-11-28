using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Solicitudes de faltantes (demanda insatisfecha): captura, seguimiento, abastecimiento de Compras e historial de venta perdida.</summary>
public interface IFaltantes
{
    Task<ResponseGeneric<SolicitudFaltanteDTO>> Crear(CrearSolicitudFaltanteRequest solicitud);
    Task<ResponseGeneric<IReadOnlyList<SolicitudFaltanteDTO>>> Mis(bool soloAbiertas);
    Task<ResponseGeneric<SolicitudFaltanteDTO>> Concretar(long id, long idVenta);
    Task<ResponseGeneric<SolicitudFaltanteDTO>> Cancelar(long id, string? motivo);
    Task<ResponseGeneric<IReadOnlyList<GrupoAbastecimientoDTO>>> Bandeja();
    /// <summary>Devuelve cuántas solicitudes se vincularon.</summary>
    Task<ResponseGeneric<int>> VincularOrdenCompra(VincularOrdenCompraRequest solicitud);
    Task<ResponseGeneric<HistorialFaltantesDTO>> Historial(FiltroHistorialFaltantesDTO filtro);
}
