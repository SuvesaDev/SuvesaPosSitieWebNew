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
    /// Estado de la sucursal abierta. Si se omite, el módulo sigue oculto. Si ya se
    /// resolvió y la sucursal está apagada, solo se muestra Configuración emisor.
    /// </param>
    public static bool EsVisible(ItemMenu item, IContextoSesion sesion, IContextoContabilidad? contabilidad = null)
    {
        if (string.Equals(item.Codigo, "COMPRAS.IMPORTACIONES", StringComparison.OrdinalIgnoreCase) && !sesion.HabilitaImportaciones)
            return false;
        // Sin resolver, el módulo sigue oculto. Ya resuelto y apagado, solo queda
        // Configuración emisor: ahí se enciende la sucursal. El resto aparece después.
        if (ContabilidadOculta(item, contabilidad))
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

    private static bool ContabilidadOculta(ItemMenu item, IContextoContabilidad? contabilidad)
    {
        if (item.Codigo is null || !item.Codigo.StartsWith("CONTABILIDAD", StringComparison.OrdinalIgnoreCase))
            return false;
        if (contabilidad is null)
            return string.Equals(item.Codigo, "CONTABILIDAD", StringComparison.OrdinalIgnoreCase);
        if (contabilidad.HabilitadaEmisorActual)
            return false;
        return !string.Equals(item.Codigo, "CONTABILIDAD", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(item.Codigo, "CONTABILIDAD.CONFIGURACION_EMISOR", StringComparison.OrdinalIgnoreCase);
    }
}
