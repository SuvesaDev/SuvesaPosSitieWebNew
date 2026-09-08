using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Piezas que la pantalla "Abono Cobrar" necesita y que no están en el contrato
/// NSwag: preventas pendientes por cliente y emisión síncrona a Hacienda
/// (ABONO_COBRAR_PREVENTAS_WEB.md).
/// </summary>
public interface IAbonoCobrarPreventas
{
    Task<ResponseGeneric<ICollection<PreventaResumenWebDTO>>> PreventasPendientes(long codCliente);

    /// <summary>Reserva numeración + firma + envía a Hacienda una factura desde la venta POS.</summary>
    Task<ResponseGeneric<ResultadoEmisionWebDTO>> EmitirFactura(long idVenta);

    /// <summary>Igual, para tiquete electrónico.</summary>
    Task<ResponseGeneric<ResultadoEmisionWebDTO>> EmitirTiquete(long idVenta);
}
