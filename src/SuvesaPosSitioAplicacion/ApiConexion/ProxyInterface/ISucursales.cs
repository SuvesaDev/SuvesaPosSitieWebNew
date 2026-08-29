using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Registro de sucursales (Empresas → Sucursales en el menu).
///
/// El sistema actual SOLO crea: no hay "obtener todas" ni edicion para esta
/// pantalla en el React original (BranchAction.js no tiene mas que
/// startSaveBranch). Se replica igual: formulario de alta, sin listado.
/// </summary>
public interface ISucursales
{
    Task<ResponseGeneric<ICollection<TipoIdentificacionDTO>>> TiposIdentificacion();

    Task<ResponseGeneric<SucursalDTO>> Crear(SucursalDTO sucursal);
}
