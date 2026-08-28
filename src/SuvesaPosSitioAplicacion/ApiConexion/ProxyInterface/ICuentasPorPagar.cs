using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Deudas pendientes con proveedores.</summary>
public interface ICuentasPorPagar
{
    Task<ResponseGeneric<ICollection<BuscarProveedorPendientesDTO>>> ObtenerDeudas();
}
