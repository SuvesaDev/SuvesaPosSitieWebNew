using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Consulta y mantenimiento basico de proveedores.</summary>
public interface IProveedoresConsulta
{
    Task<ResponseGeneric<ICollection<ProveedorDTO>>> Obtener();

    /// <summary>Un proveedor completo, con sus cuentas bancarias registradas.</summary>
    Task<ResponseGeneric<ProveedorDTO>> Uno(int codigo);

    Task<ResponseGeneric<ProveedorDTO>> Crear(ProveedorDTO proveedor);

    Task<ResponseGeneric<ProveedorDTO>> Editar(ProveedorDTO proveedor);

    /// <summary>Consulta el nombre registrado en Hacienda a partir de la cédula.</summary>
    Task<ResponseGeneric<BuscarClienteFacturacionDTO>> BuscarHacienda(string cedula);

    Task<ResponseGeneric<bool>> CambiarEstado(int codigo, bool inhabilitar);

    Task<ResponseGeneric<CuentaBancariaProveedorDTO>> EliminarCuenta(int idCuenta);
}
