using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Facturas pendientes de cobro, por cliente.</summary>
public interface ICuentasPorCobrar
{
    Task<ResponseGeneric<ICollection<BuscarClientesPendientesDTO>>> ObtenerPendientes();
}
