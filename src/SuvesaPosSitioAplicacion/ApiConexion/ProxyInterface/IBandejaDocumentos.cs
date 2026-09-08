using SuvesaPosSitioAplicacion.DTOs.Bandeja;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Bandeja unificada de documentos: Preventas / Facturas / Notas de Crédito /
/// Consignaciones. Solo consulta y detalle; las acciones fiscales van por
/// <see cref="IBandejaFiscal"/> y la devolución por la pantalla de Devoluciones.
/// </summary>
public interface IBandejaDocumentos
{
    Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoBandeja>>> Preventas(BandejaDocumentosFiltro filtro);

    Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoFiscalBandeja>>> Facturas(BandejaDocumentosFiltro filtro);

    Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoFiscalBandeja>>> NotasCredito(BandejaDocumentosFiltro filtro);

    Task<ResponseGeneric<BandejaDocumentosResultado<DocumentoBandeja>>> Consignaciones(BandejaDocumentosFiltro filtro);

    Task<ResponseGeneric<FacturaBandejaDetalle>> DetalleFactura(long id);

    Task<ResponseGeneric<NotaCreditoBandejaDetalle>> DetalleNotaCredito(long id);
}
