using SuvesaPosSitioAplicacion.DTOs.Bonificacion;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Catalogo de "tipos de bonificacion" (ej. compra 3 lleva 1 gratis), compartido
/// entre Clientes y Articulos. Se llama <c>ICatalogoBonificacion</c> y no
/// <c>IConfiguracionBonificacion</c> porque el DTO generado ya se llama
/// <c>ConfiguracionBonificacion</c> — un proxy con el mismo nombre que un DTO
/// generado rompe la compilacion (ya paso una vez con Devoluciones de venta).
/// </summary>
public interface ICatalogoBonificacion
{
    /// <summary>Tipos activos — lo que se ofrece al facturar. (Contrato existente.)</summary>
    Task<ResponseGeneric<ICollection<ConfiguracionBonificacion>>> Disponibles();

    /// <summary>Todos los tipos, activos e inactivos — para mantenimiento (§3.1).</summary>
    Task<ResponseGeneric<ICollection<ConfiguracionBonificacionDTO>>> Todas();

    Task<ResponseGeneric<ConfiguracionBonificacionDTO>> Crear(ConfiguracionBonificacionDTO configuracion);
    Task<ResponseGeneric<ConfiguracionBonificacionDTO>> Editar(ConfiguracionBonificacionDTO configuracion);
    Task<ResponseGeneric<bool>> Habilitar(int idConfiguracion);
    Task<ResponseGeneric<bool>> Deshabilitar(int idConfiguracion);
    Task<ResponseGeneric<bool>> Eliminar(int idConfiguracion);
}
