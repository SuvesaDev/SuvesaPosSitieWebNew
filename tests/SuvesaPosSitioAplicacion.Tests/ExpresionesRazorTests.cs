using System.Text.RegularExpressions;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Dos errores de Razor que compilan y se ven en pantalla como texto de código (hallazgos 13 y 14 del UAT):
/// el formato fuera de la expresión (<c>@valor:N2</c> imprime «1234:N2») y el miembro fuera del paréntesis
/// (<c>@(a ?? b).ToString("x")</c> imprime «fecha.ToString("x")»). Razor no formatea con «:».
/// </summary>
public class ExpresionesRazorTests
{
    private static string RaizViews()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "SuvesaPosSitioAplicacion", "Views")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "SuvesaPosSitioAplicacion", "Views");
    }

    private static IEnumerable<(string Archivo, string Texto)> Vistas()
        => Directory.EnumerateFiles(RaizViews(), "*.razor", SearchOption.AllDirectories).Select(f => (f, File.ReadAllText(f)));

    // @valor:N2 · @obj.Prop:N2% · @(a + b):N2 — pero no @bind:event, @onclick:preventDefault ni @key.
    private static readonly Regex FormatoFueraDeLaExpresion = new(
        @"@(?!bind|on[a-z]|key|ref|attributes)(?:[A-Za-z_][\w\.\?\!\[\]]*|\((?:[^()]*(?:\([^()]*\)[^()]*)*)\)):[NnFfCcPpDd]\d{0,2}\b",
        RegexOptions.Compiled);

    [Fact]
    public void NingunaVistaFormateaConDosPuntosDespuesDeLaExpresion()
    {
        var malas = Vistas().SelectMany(v => FormatoFueraDeLaExpresion.Matches(v.Texto)
            .Select(m => $"{Path.GetRelativePath(RaizViews(), v.Archivo)}: {m.Value}")).ToList();

        Assert.True(malas.Count == 0, "Use Formato.Importe/Numero/Porcentaje o .ToString(\"N2\") dentro de la expresión:\n" + string.Join("\n", malas));
    }

    [Fact]
    public void NingunaVistaDejaUnMiembroFueraDelParentesisDeLaExpresion()
    {
        var malas = new List<string>();
        foreach (var (archivo, texto) in Vistas())
        {
            var k = 0;
            while ((k = texto.IndexOf("@(", k, StringComparison.Ordinal)) >= 0)
            {
                var fin = CierreDelParentesis(texto, k + 1);
                if (fin > 0 && fin + 1 < texto.Length && texto[fin] == '.' && char.IsLetter(texto[fin + 1]))
                    malas.Add($"{Path.GetRelativePath(RaizViews(), archivo)}: {texto.Substring(k, Math.Min(fin - k + 22, texto.Length - k)).Replace('\n', ' ')}");
                k += 2;
            }
        }

        Assert.True(malas.Count == 0, "Envuelva toda la expresión: @((a ?? b).ToString(\"x\")):\n" + string.Join("\n", malas));
    }

    /// <summary>Índice justo después del paréntesis que cierra al de <paramref name="abre"/>, ignorando lo que va entre comillas.</summary>
    private static int CierreDelParentesis(string s, int abre)
    {
        var profundidad = 0;
        char? cadena = null;
        for (var j = abre; j < s.Length; j++)
        {
            var c = s[j];
            if (cadena is not null)
            {
                if (c == '\\' && cadena == '"') j++;
                else if (c == cadena) cadena = null;
            }
            else if (c is '"' or '\'') cadena = c;
            else if (c == '(') profundidad++;
            else if (c == ')' && --profundidad == 0) return j + 1;
        }
        return -1;
    }

    [Fact]
    public void ElDetectorEncuentraLosDosErroresEnTextoDeEjemplo()
    {
        Assert.Matches(FormatoFueraDeLaExpresion, "<td>@item.Costo:N2</td>");
        Assert.Matches(FormatoFueraDeLaExpresion, "<td>@(a + b):N2</td>");
        Assert.DoesNotMatch(FormatoFueraDeLaExpresion, "<input @bind:event=\"onchange\" @onclick:preventDefault />");
        Assert.Equal(-1, CierreDelParentesis("@(sin cierre", 1));
        Assert.Equal(7, CierreDelParentesis("@(a(b))", 1));
    }
}
