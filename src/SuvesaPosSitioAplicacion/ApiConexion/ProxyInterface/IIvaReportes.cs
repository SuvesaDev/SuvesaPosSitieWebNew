using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Reportes de IVA por emisor y período (IVA repercutido; soportado y prorrata en fases siguientes).</summary>
public interface IIvaReportes
{
    Task<ResponseGeneric<IvaRepercutidoResultado>> Repercutido(int idEmpresa, DateTime desde, DateTime hasta);
    /// <summary>Artículos de una factura, tiquete o nota de crédito del reporte (costo y utilidad solo para Super Administración).</summary>
    Task<ResponseGeneric<IvaDocumentoDetalle>> DocumentoRepercutido(bool notaCredito, long id);
    /// <summary>Todas las líneas del período, para la hoja «Líneas» del Excel.</summary>
    Task<ResponseGeneric<IvaLineasPeriodo>> LineasRepercutido(int idEmpresa, DateTime desde, DateTime hasta);
    Task<ResponseGeneric<IvaSoportadoResultado>> Soportado(int idEmpresa, DateTime desde, DateTime hasta);
    /// <summary>Artículos de una compra, importación o devolución de compra del IVA soportado.</summary>
    Task<ResponseGeneric<IvaSoportadoDocumento>> DocumentoSoportado(string clase, long id);
    /// <summary>Todas las líneas del período, para la hoja «Líneas» del Excel.</summary>
    Task<ResponseGeneric<IvaSoportadoLineasPeriodo>> LineasSoportado(int idEmpresa, DateTime desde, DateTime hasta);
    Task<ResponseGeneric<IvaProrrataResultado>> Prorrata(int idEmpresa, int anio, int hastaMes);
    Task<ResponseGeneric<IvaProrrataParametro>> ParametrosProrrata(int idEmpresa, int anio);
    Task<ResponseGeneric<IvaProrrataParametro>> GuardarParametrosProrrata(IvaProrrataParametro parametro);
    Task<ResponseGeneric<IvaProrrataDeclarados>> DeclaradosProrrata(int idEmpresa, int anio);
    Task<ResponseGeneric<IvaProrrataDeclarados>> GuardarDeclaradosProrrata(IvaProrrataDeclarados declarados);
    Task<ResponseGeneric<IvaProrrataLiquidacion>> CongelarLiquidacionProrrata(CongelarLiquidacion datos);
    Task<ResponseGeneric<List<IvaProrrataLiquidacion>>> LiquidacionesProrrata(int idEmpresa, int anio);
    /// <summary>Porcentaje de prorrata provisional aplicable a una compra, para sugerir el crédito del Mensaje Receptor (condición 05).</summary>
    Task<ResponseGeneric<PorcentajeProrrata>> PorcentajeProrrataDeCompra(long idCompra);
}
