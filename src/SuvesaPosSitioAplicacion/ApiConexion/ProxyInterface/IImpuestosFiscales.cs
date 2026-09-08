using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IImpuestosFiscales
{
    Task<ResponseGeneric<ICollection<ImpuestoFiscalDTO>>> Obtener();
    Task<ResponseGeneric<ImpuestoFiscalDTO>> Crear(ImpuestoFiscalDTO impuesto);
    Task<ResponseGeneric<ImpuestoFiscalDTO>> Actualizar(ImpuestoFiscalDTO impuesto);
    Task<ResponseGeneric<ImpuestoFiscalDTO>> Deshabilitar(int idImpuesto, string? usuario);
}
