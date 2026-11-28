using SuvesaPosSitioAplicacion.DTOs.Clientes;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Unificar clientes duplicados y eliminar fichas sin historial (api/clientes/unificacion).</summary>
public interface IClienteUnificacion
{
    /// <summary>Cuántas filas de cada tabla pasarían al otro cliente. No cambia nada.</summary>
    Task<ResponseGeneric<VistaUnificacionWebDTO>> VistaPrevia(long idOrigen, long idDestino);

    /// <summary>Pasa todo el historial y la ficha de <paramref name="idOrigen"/> a <paramref name="idDestino"/> y desactiva el origen.</summary>
    Task<ResponseGeneric<VistaUnificacionWebDTO>> Unificar(long idOrigen, long idDestino);

    /// <summary>Si el cliente se puede eliminar (no tiene historial) o por qué no.</summary>
    Task<ResponseGeneric<EstadoEliminacionClienteWebDTO>> PuedeEliminar(long idCliente);

    Task<ResponseGeneric<EstadoEliminacionClienteWebDTO>> Eliminar(long idCliente);
}
