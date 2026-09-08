using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Entrega a cuenta: adelanto de dinero de un cliente contra futuras facturas.
///
/// El sistema actual tiene un boton "Anular" (startDisableDownPayment) que llama
/// al MISMO endpoint de creacion con un "id" en la URL, marcado en el propio
/// codigo fuente con "//TODO: CAMBIAR URL" — nunca se corrigio. No hay endpoint
/// real de anulacion que llamar, asi que no se replica ese boton.
/// </summary>
public interface IEntregasCuenta
{
    Task<ResponseGeneric<ICollection<FormasPagoDTO>>> FormasPago();

    Task<ResponseGeneric<ClienteBuscarNombreCedulaDTO>> BuscarClientePorCedula(long cedula);

    Task<ResponseGeneric<ICollection<EntregaCuentaDTO>>> Buscar(BuscarEntregaCuentaDTO filtro);

    Task<ResponseGeneric<EntregaCuentaDTO>> Obtener(long id);

    Task<ResponseGeneric<EntregaCuentaDTO>> Crear(EntregaCuentaDTO entrega);
}
