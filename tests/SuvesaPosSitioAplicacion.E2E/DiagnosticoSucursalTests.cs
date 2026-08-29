using Microsoft.Playwright;
using Xunit.Abstractions;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>TEMPORAL: por que se atasca esperando el boton "Ingresar" en /cuenta/sucursal.</summary>
[Collection(ColeccionE2E.Nombre)]
public class DiagnosticoSucursalTests
{
    private readonly AplicacionEnPruebas _app;
    private readonly NavegadorEnPruebas _navegador;
    private readonly ITestOutputHelper _salida;

    public DiagnosticoSucursalTests(AplicacionEnPruebas app, NavegadorEnPruebas navegador, ITestOutputHelper salida)
    {
        _app = app;
        _navegador = navegador;
        _salida = salida;
    }

    [HechoConCredenciales]
    public async Task VerPaginaSucursal()
    {
        var contexto = await _navegador.ContextoNuevoAsync(_app.Url);
        var p = await contexto.NewPageAsync();

        var mensajes = new List<string>();
        p.Console += (_, m) => mensajes.Add($"[{m.Type}] {m.Text}");
        p.PageError += (_, err) => mensajes.Add($"[pageerror] {err}");

        await p.GotoAsync("/cuenta/ingresar");
        await p.GetByLabel("Usuario").FillAsync(CredencialesPrueba.Usuario!);
        await p.GetByLabel("Contrasena").FillAsync(CredencialesPrueba.Password!);
        await p.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesion" }).ClickAsync();
        await p.WaitForURLAsync("**/cuenta/sucursal", new() { Timeout = 60_000 });
        await p.WaitForTimeoutAsync(2000);

        await p.ScreenshotAsync(new() { Path = "/tmp/diagnostico-sucursal.png", FullPage = true });
        _salida.WriteLine($"URL: {p.Url}");
        _salida.WriteLine("--- texto visible ---");
        _salida.WriteLine(await p.InnerTextAsync("body"));
        _salida.WriteLine("--- consola ---");
        foreach (var m in mensajes) _salida.WriteLine(m);
    }
}
