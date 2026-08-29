using Microsoft.Playwright;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Los fallos que costaron la Ola 0, convertidos en pruebas.
///
/// Los cuatro eran de integracion —politicas de autorizacion, modos de render,
/// ambitos de inyeccion— y ninguna prueba unitaria los habria visto. Todos se
/// manifestaban solo al ejecutar la aplicacion de verdad en un navegador.
/// </summary>
[Collection(ColeccionE2E.Nombre)]
public class CimientosE2ETests
{
    private readonly AplicacionEnPruebas _app;
    private readonly NavegadorEnPruebas _navegador;

    public CimientosE2ETests(AplicacionEnPruebas app, NavegadorEnPruebas navegador)
    {
        _app = app;
        _navegador = navegador;
    }

    private async Task<IPage> PaginaAsync()
    {
        var contexto = await _navegador.ContextoNuevoAsync(_app.Url);
        return await contexto.NewPageAsync();
    }

    [Fact]
    public async Task LosEstaticosNoRedirigenAlLogin()
    {
        // FALLO REAL: la FallbackPolicy de autorizacion se aplicaba tambien a los
        // endpoints estaticos. El CSS y el JS respondian 302 al login y el navegador
        // intentaba ejecutar el HTML del login como script.
        var pagina = await PaginaAsync();

        foreach (var recurso in new[]
                 {
                     "/lib/bootstrap/bootstrap.bundle.min.js",
                     "/_content/Havit.Blazor.Components.Web.Bootstrap/bootstrap.min.css"
                 })
        {
            var r = await pagina.APIRequest.GetAsync(recurso);

            Assert.True(r.Ok, $"{recurso} respondio {r.Status}");
            Assert.DoesNotContain("text/html", r.Headers.GetValueOrDefault("content-type", ""));
        }
    }

    [Fact]
    public async Task LaPantallaDeIngresoSeVeConEstilos()
    {
        // Si el CSS no carga, la pagina renderiza pero sin ningun estilo aplicado.
        // Se comprueba mirando un estilo que solo existe si Bootstrap llego.
        var pagina = await PaginaAsync();
        await pagina.GotoAsync("/");

        await Assertions.Expect(pagina.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesion" }))
            .ToBeVisibleAsync();

        var fondo = await pagina.Locator("button[type=submit]").EvaluateAsync<string>(
            "e => getComputedStyle(e).backgroundColor");

        Assert.NotEqual("rgba(0, 0, 0, 0)", fondo);
    }

    [Fact]
    public async Task SinSesion_LaRaizLlevaAlIngreso()
    {
        var pagina = await PaginaAsync();
        await pagina.GotoAsync("/");

        Assert.Contains("/cuenta/ingresar", pagina.Url);
    }

    [Fact]
    public async Task LasRutasDelSistemaActualPidenSesionYNoDan404()
    {
        // La ruta comodin recoge las 78 pantallas pendientes. Si una diera 404, el
        // shell se romperia al abrirla desde el menu.
        var pagina = await PaginaAsync();

        foreach (var ruta in new[] { "/initial/billing", "/buys/buy", "/parameters/users" })
        {
            await pagina.GotoAsync(ruta);
            Assert.Contains("/cuenta/ingresar", pagina.Url);
        }
    }

    [Fact]
    public async Task ElIngresoConCredencialesIncorrectas_MuestraElMensajeDelApi()
    {
        // Recorre entera la cadena: formulario, ServicioAutenticacion, proxy,
        // cliente generado, API real, y traduccion del envelope.
        var pagina = await PaginaAsync();
        await pagina.GotoAsync("/cuenta/ingresar");

        await pagina.GetByLabel("Usuario").FillAsync("usuario.que.no.existe");
        await pagina.GetByLabel("Contrasena").FillAsync("clave.incorrecta");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesion" }).ClickAsync();

        await Assertions.Expect(pagina.Locator(".alert-danger"))
            .ToBeVisibleAsync(new() { Timeout = 30_000 });
    }

    [Fact]
    public async Task NoQuedanErroresEnLaConsolaDelNavegador()
    {
        // FALLO REAL: el circuito de SignalR no arrancaba y solo se veia en la
        // consola, con un mensaje que no decia de donde venia.
        var pagina = await PaginaAsync();

        var errores = new List<string>();
        pagina.Console += (_, m) =>
        {
            if (m.Type == "error")
            {
                errores.Add(m.Text);
            }
        };

        await pagina.GotoAsync("/cuenta/ingresar");
        await pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.True(errores.Count == 0, string.Join("\n", errores));
    }
}
