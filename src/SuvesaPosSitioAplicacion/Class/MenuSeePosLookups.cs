using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Class;

/// <summary>
/// Indices sobre <see cref="MenuSeePos.Items"/>. Sirven para que las Views que aun
/// pasan el TITULO de pantalla a las comprobaciones de permiso sigan funcionando
/// tras el rediseno V2 (que casa por codigo): el titulo se resuelve aqui a su
/// <see cref="ItemMenu.Codigo"/>.
/// </summary>
public static partial class MenuSeePos
{
    private static readonly Lazy<IReadOnlyList<ItemMenu>> _planos = new(() =>
    {
        var lista = new List<ItemMenu>();
        void Recorrer(IEnumerable<ItemMenu> items)
        {
            foreach (var i in items)
            {
                lista.Add(i);
                Recorrer(i.Hijos);
            }
        }
        Recorrer(Items);
        return lista;
    });

    private static readonly Lazy<IReadOnlyDictionary<string, string>> _codigoPorTitulo = new(() =>
    {
        var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var i in _planos.Value)
        {
            // primera aparicion gana; los titulos ambiguos ("Devoluciones", "Toma")
            // deben resolverse en la View pasando el codigo explicito.
            if (i.Titulo is { Length: > 0 } t && i.Codigo is { Length: > 0 } c && !mapa.ContainsKey(t))
            {
                mapa[t] = c;
            }
        }
        return mapa;
    });

    private static readonly Lazy<IReadOnlyDictionary<string, string>> _codigoPorRuta = new(() =>
    {
        var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var i in _planos.Value)
        {
            if (i.Ruta is { Length: > 0 } r && i.Codigo is { Length: > 0 } c)
            {
                mapa[r.Trim('/')] = c;
            }
        }
        return mapa;
    });

    /// <summary>Todos los nodos del menu en pre-orden.</summary>
    public static IReadOnlyList<ItemMenu> Planos => _planos.Value;

    /// <summary>Codigo de funcion para un titulo de pantalla. Null si no esta o es ambiguo sin resolver.</summary>
    public static string? CodigoDeTitulo(string? titulo)
        => titulo is { Length: > 0 } && _codigoPorTitulo.Value.TryGetValue(titulo, out var c) ? c : null;

    /// <summary>Codigo de funcion para una ruta Blazor.</summary>
    public static string? CodigoDeRuta(string? ruta)
        => ruta is { Length: > 0 } && _codigoPorRuta.Value.TryGetValue(ruta.Trim('/'), out var c) ? c : null;

    private static readonly Lazy<IReadOnlyDictionary<string, string>> _iconoPorModulo = new(() =>
        Items.Where(m => !string.IsNullOrWhiteSpace(m.Codigo) && !string.IsNullOrWhiteSpace(m.Icono))
             .ToDictionary(m => m.Codigo!, m => m.Icono!, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Icono (clase Bootstrap Icons) del modulo al que pertenece una ruta. Sirve para
    /// que cada pestana del espacio de trabajo lleve el icono de su modulo.
    /// </summary>
    public static string IconoDeRuta(string? ruta)
    {
        var codigo = CodigoDeRuta(ruta);
        if (codigo is null)
        {
            return "bi-window";
        }

        var modulo = codigo.Split('.', 2)[0];
        return _iconoPorModulo.Value.TryGetValue(modulo, out var icono) ? icono : "bi-window";
    }

    /// <summary>
    /// Normaliza una "clave de pantalla" a codigo de funcion: si ya trae un punto se
    /// asume que es un codigo (MODULO.SLUG); si no, se resuelve como titulo.
    /// </summary>
    public static string ResolverCodigo(string clave)
        => string.IsNullOrWhiteSpace(clave) ? clave
           : clave.Contains('.') ? clave
           : CodigoDeTitulo(clave) ?? clave;
}
