namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Resuelve y cachea si Contabilidad esta habilitada para el emisor activo (W1).
///
/// A diferencia de <see cref="IContextoSesion.HabilitaImportaciones"/> (un claim de
/// Identity fijo, resuelto una sola vez al iniciar sesion), el emisor activo puede
/// cambiar dentro de la misma sesion de Facturacion — asi que aqui no hay un claim
/// fijo posible. Cada pantalla Contabilidad-aware llama a <see cref="ResolverAsync"/>
/// con su propio emisor de contexto (empezando por Facturacion, en el mismo punto
/// donde ya recarga al cambiar de emisor); el resultado queda cacheado por emisor
/// para que el menu (<see cref="FiltroMenu"/>) lo lea de forma sincrona.
/// </summary>
public interface IContextoContabilidad
{
    /// <summary>Ultimo resultado resuelto por <see cref="ResolverAsync"/> — false por
    /// defecto (oculto) hasta que algo lo resuelva, mismo criterio ya decidido de
    /// "oculto completamente" cuando el flag no esta confirmado.</summary>
    bool HabilitadaEmisorActual { get; }

    /// <summary>Consulta el API (o el cache interno, si ya se resolvio este mismo
    /// emisor) y actualiza <see cref="HabilitadaEmisorActual"/>.</summary>
    Task ResolverAsync(long idEmpresa, int idEmisor, CancellationToken ct = default);
}
