using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Órdenes de compra a proveedor: alta con consecutivo, impresión, envío por
/// correo y seguimiento (entregado / cancelado / baja por proveedor / facturado).
/// </summary>
public interface IOrdenesCompraFlujo
{
    Task<ResponseGeneric<long>> SiguienteConsecutivo(int idEmisor, int idSucursal);

    Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Crear(CrearOrdenCompraWebDTO cmd);

    Task<ResponseGeneric<IReadOnlyList<OrdenCompraFlujoWebDTO>>> Listar(
        int? idProveedor = null, int? estado = null, bool incluirAnuladas = false,
        DateTime? desde = null, DateTime? hasta = null, long? consecutivo = null, int limite = 200);

    Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Obtener(long orden);

    Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Entregar(long orden, DateTime? fecha);
    Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Cancelar(long orden, string? motivo);
    Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> BajaProveedor(long orden, string? motivo);
    Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> VincularFactura(long orden, long idFacturaCompra);

    Task<ResponseGeneric<ResultadoEnvioOrdenCompraWebDTO>> EnviarCorreo(long orden, string? destino);
}
