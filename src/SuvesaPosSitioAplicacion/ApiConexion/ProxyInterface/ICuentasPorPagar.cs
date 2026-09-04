using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Deudas pendientes con proveedores.</summary>
public interface ICuentasPorPagar
{
    Task<ResponseGeneric<ICollection<BuscarProveedorPendientesDTO>>> ObtenerDeudas();
    Task<ResponseGeneric<AbonoCuentaPagarReciboDTO>> CrearAbono(AbonoCuentaPagarReciboDTO abono);

    /// <summary>Recibos de pago (abonos a proveedor) emitidos — SANEAMIENTO Fase 8.5.</summary>
    Task<ResponseGeneric<ICollection<AbonoCuentaPagarReciboDTO>>> ListarAbonos();
}
