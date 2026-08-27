using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Decide que se ve del menu segun el rol.
///
/// Vive aparte del componente por dos motivos: se puede probar, y hace falta tambien
/// fuera de la barra lateral (atajos, y desde la Ola 1 la guarda de cada pantalla).
///
/// MEJORA respecto al sistema actual: alli el menu solo se filtra en la variante
/// CostaPets; el camino normal muestra todas las pantallas aunque el rol no las tenga.
/// </summary>
public static class FiltroMenu
{
    /// <summary>
    /// Un nodo hoja se ve si el rol tiene la accion Ver sobre su titulo.
    /// Un grupo se ve si algun descendiente se ve, para no dejar menus vacios.
    /// </summary>
    public static bool EsVisible(ItemMenu item, IContextoSesion sesion)
    {
        if (sesion.EsAdministrador)
        {
            return true;
        }

        if (item.EsGrupo)
        {
            return item.Hijos.Any(h => EsVisible(h, sesion));
        }

        return sesion.PuedeVer(item.Titulo);
    }

    public static IEnumerable<ItemMenu> Visibles(IEnumerable<ItemMenu> items, IContextoSesion sesion)
        => items.Where(i => EsVisible(i, sesion));
}
