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
    Task<ResponseGeneric<ICollection<ConfiguracionBonificacion>>> Disponibles();
}
