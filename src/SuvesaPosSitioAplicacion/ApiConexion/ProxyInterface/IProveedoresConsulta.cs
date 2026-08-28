using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta y mantenimiento basico de proveedores.</summary>
public interface IProveedoresConsulta
{
    Task<ResponseGeneric<ICollection<ProveedorDTO>>> Obtener();

    Task<ResponseGeneric<ProveedorDTO>> Crear(ProveedorDTO proveedor);

    Task<ResponseGeneric<ProveedorDTO>> Editar(ProveedorDTO proveedor);

    Task<ResponseGeneric<bool>> CambiarEstado(int codigo, bool inhabilitar);

    Task<ResponseGeneric<CuentaBancariaProveedorDTO>> EliminarCuenta(int idCuenta);
}
