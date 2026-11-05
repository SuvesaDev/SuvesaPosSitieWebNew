using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Reportes de IVA por emisor y período (IVA repercutido; soportado y prorrata en fases siguientes).</summary>
public interface IIvaReportes
{
    Task<ResponseGeneric<IvaRepercutidoResultado>> Repercutido(int idEmpresa, DateTime desde, DateTime hasta);
    Task<ResponseGeneric<IvaSoportadoResultado>> Soportado(int idEmpresa, DateTime desde, DateTime hasta);
    Task<ResponseGeneric<IvaProrrataResultado>> Prorrata(int idEmpresa, int anio, int hastaMes);
    Task<ResponseGeneric<IvaProrrataParametro>> ParametrosProrrata(int idEmpresa, int anio);
    Task<ResponseGeneric<IvaProrrataParametro>> GuardarParametrosProrrata(IvaProrrataParametro parametro);
    /// <summary>Porcentaje de prorrata provisional aplicable a una compra, para sugerir el crédito del Mensaje Receptor (condición 05).</summary>
    Task<ResponseGeneric<PorcentajeProrrata>> PorcentajeProrrataDeCompra(long idCompra);
}
