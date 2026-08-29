using Microsoft.Playwright;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Models;
using Xunit.Abstractions;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Toda entrada del menu tiene que llevar a algo: una pantalla migrada o un aviso
/// claro de que esta pendiente. **Nunca a una pantalla en blanco.**
///
/// Existe por un fallo real: tres pantallas migradas declaraban una ruta propia
/// (<c>/consulta/clientes</c>) mientras el menu apuntaba a la del sistema actual
/// (<c>/initial/customers</c>). Las pruebas por pantalla pasaban en verde, porque
/// visitaban cada una por SU ruta; nadie comprobaba que el menu llegara.
///
/// Una prueba puede confirmar que una pantalla funciona y no decir nada sobre si
/// alguien puede llegar a ella.
/// </summary>
[Collection(ColeccionE2E.Nombre)]
public class MenuAlcanzableTests
{
    private readonly AplicacionEnPruebas _app;
    private readonly NavegadorEnPruebas _navegador;
    private readonly ITestOutputHelper _salida;

    public MenuAlcanzableTests(
        AplicacionEnPruebas app, NavegadorEnPruebas navegador, ITestOutputHelper salida)
    {
        _app = app;
        _navegador = navegador;
        _salida = salida;
    }

    [HechoConCredenciales]
    public async Task NingunaEntradaDelMenuAcabaEnBlanco()
    {
        var rutas = RutasDelMenu().ToList();
        Assert.NotEmpty(rutas);

        var pagina = await EntrarAsync();
        var enBlanco = new List<string>();
        var migradas = 0;
        var pendientes = 0;

        foreach (var (titulo, ruta) in rutas)
        {
            await pagina.GotoAsync(ruta);

            // Se comprueba el invariante de verdad —que el area de contenido tenga
            // texto visible— y no la presencia de un elemento concreto. Intentarlo
            // con selectores fallo cuatro veces: en AppPantalla hay un .alert oculto
            // ANTES del h1, asi que cualquier .First cogia el elemento invisible y
            // daba por vacia una pantalla perfectamente pintada.
            var texto = await EsperarTextoAsync(pagina);

            if (string.IsNullOrWhiteSpace(texto))
            {
                enBlanco.Add($"{ruta}  ({titulo})");
                continue;
            }

            // Una pantalla migrada abre con su titulo; una pendiente, con el aviso.
            if (texto.StartsWith(titulo, StringComparison.OrdinalIgnoreCase))
            {
                migradas++;
            }
            else
            {
                pendientes++;
            }
        }

        _salida.WriteLine($"rutas del menu   : {rutas.Count}");
        _salida.WriteLine($"llevan a pantalla: {migradas}");
        _salida.WriteLine($"avisan pendiente : {pendientes}");
        _salida.WriteLine($"EN BLANCO        : {enBlanco.Count}");

        foreach (var r in enBlanco)
        {
            _salida.WriteLine("   " + r);
        }

        Assert.True(enBlanco.Count == 0,
            "Estas entradas del menu no muestran nada:\n" + string.Join("\n", enBlanco));
    }

    /// <summary>Texto visible del area de contenido, esperando a que el circuito pinte.</summary>
    private static async Task<string> EsperarTextoAsync(IPage pagina)
    {
        var limite = DateTime.UtcNow.AddSeconds(15);

        while (DateTime.UtcNow < limite)
        {
            var texto = (await pagina.Locator("main").InnerTextAsync()).Trim();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }

            await pagina.WaitForTimeoutAsync(250);
        }

        return string.Empty;
    }

    /// <summary>Hojas del menu: las que tienen ruta y por tanto se pueden abrir.</summary>
    private static IEnumerable<(string Titulo, string Ruta)> RutasDelMenu()
    {
        static IEnumerable<(string, string)> Recorrer(IEnumerable<ItemMenu> items)
        {
            foreach (var i in items)
            {
                if (!string.IsNullOrWhiteSpace(i.Ruta))
                {
                    yield return (i.Titulo, i.Ruta);
                }

                foreach (var h in Recorrer(i.Hijos))
                {
                    yield return h;
                }
            }
        }

        // Distintas por ruta: varias entradas comparten destino.
        return Recorrer(MenuSeePos.Items)
            .GroupBy(x => x.Item2, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First());
    }

    private async Task<IPage> EntrarAsync()
    {
        var ctx = await _navegador.ContextoNuevoAsync(_app.Url);
        var p = await ctx.NewPageAsync();

        await p.GotoAsync("/cuenta/ingresar");
        await p.GetByLabel("Usuario").FillAsync(CredencialesPrueba.Usuario!);
        await p.GetByLabel("Contrasena").FillAsync(CredencialesPrueba.Password!);
        await p.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesion" }).ClickAsync();
        await p.WaitForURLAsync("**/cuenta/sucursal", new() { Timeout = 60_000 });

        var op = p.Locator("select option");
        await Assertions.Expect(op.Nth(1)).ToBeAttachedAsync(new() { Timeout = 60_000 });
        await p.Locator("select").SelectOptionAsync((await op.Nth(1).GetAttributeAsync("value"))!);
        await p.GetByRole(AriaRole.Button, new() { Name = "Continuar" }).ClickAsync();

        await p.WaitForURLAsync(u => !u.Contains("/cuenta/"), new() { Timeout = 60_000 });
        await Assertions.Expect(p.GetByRole(AriaRole.Button, new() { Name = "Salir" }))
            .ToBeVisibleAsync(new() { Timeout = 60_000 });

        return p;
    }
}
