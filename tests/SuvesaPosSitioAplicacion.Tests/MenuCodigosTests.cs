using System.Reflection;
using System.Text.Json;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El menu del sitio y el catalogo de seguridad del API se derivan del MISMO arbol
/// (tools/anotar_codigos_menu.py y tools/generar_semilla_seguridad.py usan el mismo
/// algoritmo de slug). Aqui se comprueba que no han divergido: todo Codigo del menu
/// tiene que existir como Modulo.Codigo o Funcion.Codigo en la semilla del API.
///
/// Cuando el sitio consuma el contrato nuevo, esta prueba pasa a ser la asercion que
/// hoy solo informa (ver CLAUDE.md, "Verificacion contra el API real").
/// </summary>
public class MenuCodigosTests
{
    private static IEnumerable<ItemMenu> Aplanar(IEnumerable<ItemMenu> items)
    {
        foreach (var i in items)
        {
            yield return i;
            foreach (var h in Aplanar(i.Hijos))
                yield return h;
        }
    }

    private static readonly ItemMenu[] Todos = Aplanar(MenuSeePos.Items).ToArray();

    [Fact]
    public void Todo_nodo_del_menu_tiene_codigo()
    {
        var sinCodigo = Todos.Where(i => string.IsNullOrWhiteSpace(i.Codigo)).Select(i => i.Titulo).ToArray();
        Assert.True(sinCodigo.Length == 0, "sin Codigo: " + string.Join(", ", sinCodigo));
    }

    [Fact]
    public void Codigos_con_formato_valido_y_unicos()
    {
        foreach (var i in Todos)
            Assert.Matches("^[A-Z0-9_]+(\\.[A-Z0-9_]+)*$", i.Codigo!);

        var dups = Todos.GroupBy(i => i.Codigo).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
        Assert.True(dups.Length == 0, "duplicados: " + string.Join(", ", dups));
    }

    [Fact]
    public void Cada_codigo_del_menu_existe_en_la_semilla_del_api()
    {
        var asm = Assembly.GetExecutingAssembly();
        var recurso = asm.GetManifestResourceNames().Single(n => n.EndsWith("seed-seguridad.json"));
        using var s = asm.GetManifestResourceStream(recurso)!;
        using var r = new StreamReader(s);
        var doc = JsonDocument.Parse(r.ReadToEnd());

        var conocidos = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in doc.RootElement.GetProperty("modulos").EnumerateArray())
        {
            conocidos.Add(m.GetProperty("codigo").GetString()!);
            foreach (var f in m.GetProperty("funciones").EnumerateArray())
                conocidos.Add(f.GetProperty("codigo").GetString()!);
        }

        var faltan = Todos.Select(i => i.Codigo!).Where(c => !conocidos.Contains(c)).Distinct().ToArray();
        Assert.True(faltan.Length == 0,
            "codigos del menu que no estan en seed-seguridad.json (regenera la semilla o corre " +
            "tools/anotar_codigos_menu.py): " + string.Join(", ", faltan));
    }
}
