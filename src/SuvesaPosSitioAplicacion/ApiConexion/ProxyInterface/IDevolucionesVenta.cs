using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Devoluciones de venta ("Devoluciones" en el menu, mismo componente React en dos
/// rutas: /initial/repayment y /sales/repayment).
///
/// El sistema actual tiene una pestaña "Validación" con 3 sub-formularios (Efectivo,
/// Depósito, Anticipo) que resultaron ser mockup puro: sin onChange, sin onClick, y
/// el contenido siempre muestra el mismo componente (Anticipo) sin importar cuál
/// pestaña se "elige". Por eso el usuario que recibe la devolución y las notas
/// nunca se completan de verdad en el sistema actual. Aquí sí se hacen funcionales
/// (usa ObtenerPersonal, que ya existe y ya se consulta en el original, solo que
/// nunca se conecta a un input real).
/// </summary>
public interface IDevolucionesVenta
{
    Task<ResponseGeneric<FacturaDTO>> BuscarFacturaPorId(int idFactura);

    Task<ResponseGeneric<FacturaDTO>> BuscarFacturaPorNumero(string numeroFactura);

    /// <summary>Todas las ventas con ese número (Num_Factura no es único). El sitio
    /// muestra un selector cuando hay más de una.</summary>
    Task<ResponseGeneric<ICollection<FacturaBuscarDevolucionesDTO>>> BuscarFacturasPorNumero(string numeroFactura);

    Task<ResponseGeneric<ICollection<FacturaBuscarDevolucionesDTO>>> BuscarFacturasPorFiltro(BuscarFacturaDevolucionesDTO filtro);

    Task<ResponseGeneric<ICollection<DevolucionVentaDTO>>> Buscar(FiltroFacturaDevVenta filtro);

    Task<ResponseGeneric<DevolucionVentaDTO>> ObtenerUna(long id);

    Task<ResponseGeneric<DevolucionVentaDTO>> Crear(DevolucionVentaDTO devolucion);

    Task<ResponseGeneric<ICollection<PersonalDTO>>> Personal();

    Task<ResponseGeneric<ICollection<Moneda>>> Monedas();
}
