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
    public static bool EsVisible(ItemMenu item, IContextoSesion sesion)
    {
        if (sesion.EsSuperAdministrador)
        {
            return true;
        }

        if (item.EsGrupo)
        {
            return item.Hijos.Any(h => EsVisible(h, sesion));
        }

        return !string.IsNullOrWhiteSpace(item.Codigo) && sesion.PuedeVer(item.Codigo);
    }

    public static IEnumerable<ItemMenu> Visibles(IEnumerable<ItemMenu> items, IContextoSesion sesion)
        => items.Where(i => EsVisible(i, sesion));
}
