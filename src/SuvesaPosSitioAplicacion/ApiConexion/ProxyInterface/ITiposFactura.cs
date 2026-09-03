using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Mantenimiento de los tipos de documento y su código fiscal V4.4.</summary>
public interface ITiposFactura
{
    Task<ResponseGeneric<ICollection<TipoFacturaFiscalDTO>>> Obtener();

    /// <summary>Tipos filtrados por pantalla: "facturacion" | "devolucion" | "compra" | "consignacion".</summary>
    Task<ResponseGeneric<ICollection<TipoFacturaFiscalDTO>>> PorContexto(string contexto);

    Task<ResponseGeneric<ICollection<CodigoFEDisponibleFiscalDTO>>> CodigosFEDisponibles();

    Task<ResponseGeneric<TipoFacturaFiscalDTO>> Crear(TipoFacturaFiscalDTO tipo);

    Task<ResponseGeneric<TipoFacturaFiscalDTO>> Actualizar(TipoFacturaFiscalDTO tipo);
}
