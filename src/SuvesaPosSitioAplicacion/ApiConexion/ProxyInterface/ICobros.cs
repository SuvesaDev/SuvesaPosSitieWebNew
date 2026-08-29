using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// "Cobrar": cobro de una preventa (ficha o cliente) ya facturada a crédito o
/// pendiente de facturar. El sistema actual, tras cobrar, abre un modal de
/// tiquete/recibo para imprimir — fuera de alcance (decisión de negocio: no se
/// usa tiquete ni báscula, no se imprime nada). Por eso no se llaman
/// ObetenerAbonoCobrar ni ObtenerDatosParaImpresionFactura: ambas solo existen
/// para alimentar ese tiquete.
/// </summary>
public interface ICobros
{
    Task<ResponseGeneric<ICollection<FormasPagoDTO>>> FormasPago(long codCliente);

    Task<ResponseGeneric<PreventaDTO>> BuscarPorFicha(int ficha, DateTime fecha);

    Task<ResponseGeneric<long>> CodigoClientePorCedula(string cedula);

    Task<ResponseGeneric<PreventaDTO>> BuscarPorCliente(long codCliente);

    Task<ResponseGeneric<ICollection<CobroDocumentosDTO>>> Cobrar(ICollection<CobroDocumentosDTO> cobros);

    /// <summary>Solo aplica cuando el documento no es de crédito: convierte la preventa en factura.</summary>
    Task<ResponseGeneric<FacturaDTO>> FacturarPreventa(long idPreventa);
}
