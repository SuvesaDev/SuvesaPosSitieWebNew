namespace SuvesaPosSitioAplicacion.Models;

/// <summary>
/// Nodo del menu lateral. Arbol de hasta cuatro niveles, igual que el sistema actual.
///
/// <para><b>Titulo</b> es tambien la llave de permisos: el API devuelve los permisos por
/// <c>Menu</c> (titulo de la raiz) y <c>NombrePantalla</c> (titulo de la hoja), no por ruta.</para>
/// </summary>
public sealed class ItemMenu
{
    public required string Titulo { get; init; }

    /// <summary>Ruta que abre. Nula en los nodos que solo agrupan.</summary>
    public string? Ruta { get; init; }

    /// <summary>Clase de Bootstrap Icons. Solo se usa en las raices.</summary>
    public string? Icono { get; init; }

    public IReadOnlyList<ItemMenu> Hijos { get; init; } = Array.Empty<ItemMenu>();

    public bool EsGrupo => Hijos.Count > 0;
}
