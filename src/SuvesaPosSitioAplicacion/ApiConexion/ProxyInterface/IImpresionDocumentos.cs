using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Descarga el PDF de representación gráfica de un documento
/// (MOTOR_PLANTILLAS_IMPRESION_WEB.md §4). Lo consumen el endpoint local
/// <c>/documentos/{tipo}/{id}/pdf</c> y los botones "Imprimir" de las pantallas.
/// </summary>
public interface IImpresionDocumentos
{
    Task<ResponseGeneric<byte[]>> Pdf(string tipoSlug, long id, string? formato, bool copia);
}
