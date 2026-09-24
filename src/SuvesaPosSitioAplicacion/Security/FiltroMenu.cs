using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Decide que se ve del menu segun el rol. Casa por <see cref="ItemMenu.Codigo"/>
/// (rediseno V2), no por rotulo.
///
/// Vive aparte del componente porque se prueba y hace falta tambien fuera de la
/// barra lateral (atajos, y la guarda de cada pantalla).
/// </summary>
public static class FiltroMenu
{
    /// <summary>
    /// Una hoja se ve si el rol tiene la accion VER sobre su codigo.
    /// Un grupo se ve si algun descendiente se ve, para no dejar menus vacios.
    /// </summary>
    /// <param name="contabilidad">
    /// Estado de activacion de Contabilidad para el emisor activo (W1). Opcional para no
    /// obligar a los ~11 call sites existentes (incluidos tests no relacionados) a pasarlo
    /// — si se omite, el nodo CONTABILIDAD queda oculto por defecto, mismo criterio "oculto
    /// completamente" ya decidido cuando el flag no esta resuelto todavia.
    /// </param>
    public static bool EsVisible(ItemMenu item, IContextoSesion sesion, IContextoContabilidad? contabilidad = null)
    {
        if (string.Equals(item.Codigo, "COMPRAS.IMPORTACIONES", StringComparison.OrdinalIgnoreCase) && !sesion.HabilitaImportaciones)
            return false;
        if (string.Equals(item.Codigo, "CONTABILIDAD", StringComparison.OrdinalIgnoreCase) && contabilidad?.HabilitadaEmisorActual != true)
            return false;
        if (sesion.EsSuperAdministrador)
        {
            return true;
        }

        if (item.EsGrupo)
        {
            return item.Hijos.Any(h => EsVisible(h, sesion, contabilidad));
        }

        return !string.IsNullOrWhiteSpace(item.Codigo) && sesion.PuedeVer(item.Codigo);
    }

    public static IEnumerable<ItemMenu> Visibles(IEnumerable<ItemMenu> items, IContextoSesion sesion, IContextoContabilidad? contabilidad = null)
        => items.Where(i => EsVisible(i, sesion, contabilidad));
}
