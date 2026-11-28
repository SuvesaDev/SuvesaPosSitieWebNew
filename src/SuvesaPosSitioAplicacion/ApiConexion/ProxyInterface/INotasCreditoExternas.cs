using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Notas de crédito electrónicas contra comprobantes emitidos fuera del sistema o que aquí solo son ventas internas
/// (api/notas-credito-externas). Referencia 09, sin inventario; solo administradores.
/// </summary>
public interface INotasCreditoExternas
{
    /// <summary>Lee la factura y su acuse, los valida y propone clientes y ventas internas candidatas. No guarda nada.</summary>
    Task<ResponseGeneric<ValidacionNotaCreditoExternaWebDTO>> Validar(ValidarNotaCreditoExternaWebComando comando);

    /// <summary>Crea la nota (queda pendiente de numeración y emisión). Idempotente por <c>ClaveIdempotencia</c>.</summary>
    Task<ResponseGeneric<NotaCreditoExternaWebDTO>> Crear(CrearNotaCreditoExternaWebComando comando);

    Task<ResponseGeneric<NotaCreditoExternaWebDTO>> Obtener(long id);

    Task<ResponseGeneric<PaginaNotasCreditoExternasWebDTO>> Listar(string? estado, string? texto, int pagina, int tamano);
}
