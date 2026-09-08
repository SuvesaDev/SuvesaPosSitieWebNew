namespace SuvesaPosSitioAplicacion.Models;

/// <summary>
/// Nodo del menu lateral. Arbol de hasta cuatro niveles, igual que el sistema actual.
///
/// <para><b>Codigo</b> es la llave estable de permisos contra el catalogo de seguridad
/// (rediseno V2): <c>MODULO</c> en las raices, <c>MODULO.SLUG[.SLUG...]</c> en las hojas.
/// Lo genera <c>tools/anotar_codigos_menu.py</c> con el mismo algoritmo que la semilla
/// del API. El <b>Titulo</b> queda solo como rotulo visible.</para>
/// </summary>
public sealed class ItemMenu
{
    public required string Titulo { get; init; }

    /// <summary>
    /// Llave estable de permisos. Nula solo en un nodo que agrupa sin pantalla propia
    /// (hoy no hay ninguno). Coincide con <c>Funcion.Codigo</c> / <c>Modulo.Codigo</c> del API.
    /// </summary>
    public string? Codigo { get; init; }

    /// <summary>Ruta que abre. Nula en los nodos que solo agrupan.</summary>
    public string? Ruta { get; init; }

    /// <summary>Clase de Bootstrap Icons. Solo se usa en las raices.</summary>
    public string? Icono { get; init; }

    public IReadOnlyList<ItemMenu> Hijos { get; init; } = Array.Empty<ItemMenu>();

    public bool EsGrupo => Hijos.Count > 0;
}
