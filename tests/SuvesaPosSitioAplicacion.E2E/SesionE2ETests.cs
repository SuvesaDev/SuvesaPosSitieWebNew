using Microsoft.Playwright;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// El camino que hay detras del login. Necesita un usuario de pruebas en el entorno;
/// sin el, se omiten.
///
/// Esta es la clase que habria atrapado el fallo mas caro de la Ola 0: el token que
/// no llegaba al handler de HttpClient. Las pruebas unitarias pasaban igual, porque
/// el fallo estaba en el cableado y no en la logica.
/// </summary>
[Collection(ColeccionE2E.Nombre)]
[Trait("Categoria", "RequiereCredenciales")]
public class SesionE2ETests
{
    private readonly AplicacionEnPruebas _app;
    private readonly NavegadorEnPruebas _navegador;

    public SesionE2ETests(AplicacionEnPruebas app, NavegadorEnPruebas navegador)
    {
        _app = app;
        _navegador = navegador;
    }

    /// <summary>Entra y deja la pagina en el modal de seleccion de centro.</summary>
    private async Task<IPage> IngresarAsync()
    {
        var contexto = await _navegador.ContextoNuevoAsync(_app.Url);
        var pagina = await contexto.NewPageAsync();

        await pagina.GotoAsync("/cuenta/ingresar");
        await pagina.GetByLabel("Usuario").FillAsync(CredencialesPrueba.Usuario!);
        await pagina.GetByLabel("Contraseña").FillAsync(CredencialesPrueba.Password!);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        await pagina.WaitForURLAsync("**/cuenta/sucursal", new() { Timeout = 60_000 });
        return pagina;
    }

    /// <summary>Entra y elige el primer centro, dejando la sesion lista para trabajar.</summary>
    private async Task<IPage> EntrarAlShellAsync()
    {
        var pagina = await IngresarAsync();

        var centros = pagina.Locator("select option");
        await Assertions.Expect(centros.Nth(1)).ToBeAttachedAsync(new() { Timeout = 60_000 });

        var valor = await centros.Nth(1).GetAttributeAsync("value");
        await pagina.Locator("select").SelectOptionAsync(valor!);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Continuar" }).ClickAsync();

        await pagina.WaitForURLAsync(u => !u.Contains("/cuenta/"), new() { Timeout = 60_000 });

        // Esperar a que el circuito haya pintado. Tras la navegacion la pagina llega
        // renderizada en estatico y todavia sin los datos de sesion; afirmar aqui sin
        // esperar hacia que las pruebas fallaran con la aplicacion correcta.
        await Assertions.Expect(pagina.GetByRole(AriaRole.Button, new() { Name = "Salir" }))
            .ToBeVisibleAsync(new() { Timeout = 60_000 });

        return pagina;
    }

    [HechoConCredenciales]
    public async Task TrasIngresar_ElModalDeCentroTraeLaLista()
    {
        // FALLO REAL: el token no llegaba al handler de HttpClient porque
        // IHttpClientFactory no resuelve sus handlers en el ambito de la peticion.
        // La consulta de centros respondia 401 y el select nunca aparecia.
        var pagina = await IngresarAsync();

        await Assertions.Expect(pagina.GetByText("Seleccione el centro")).ToBeVisibleAsync();

        // Al menos un centro real, ademas del "Seleccione..."
        await Assertions.Expect(pagina.Locator("select option").Nth(1))
            .ToBeAttachedAsync(new() { Timeout = 60_000 });

        await Assertions.Expect(pagina.Locator(".alert-danger")).Not.ToBeVisibleAsync();
    }

    [HechoConCredenciales]
    public async Task TrasElegirCentro_SeEntraAlEspacioDeTrabajo()
    {
        var pagina = await EntrarAlShellAsync();

        // Acotado a la barra superior: "admin" tambien aparece en el panel de sesion
        // de la portada y dentro de la palabra "Administrador".
        await Assertions.Expect(
            pagina.Locator("nav.navbar span.text-white").Filter(new() { HasTextString = CredencialesPrueba.Usuario! }))
            .ToBeVisibleAsync();
        await Assertions.Expect(pagina.GetByRole(AriaRole.Button, new() { Name = "Salir" }))
            .ToBeVisibleAsync();
    }

    [HechoConCredenciales]
    public async Task ElMenuLateralMuestraEntradas()
    {
        var pagina = await EntrarAlShellAsync();

        await Assertions.Expect(pagina.GetByRole(AriaRole.Navigation, new() { Name = "Menu principal" }))
            .ToBeVisibleAsync();

        var raices = pagina.Locator("nav[aria-label='Menu principal'] > ul > li");
        Assert.True(await raices.CountAsync() > 0, "El menu lateral salio vacio.");
    }

    [HechoConCredenciales]
    public async Task AbrirVentaDosVeces_NumeraLasPestanas()
    {
        var pagina = await EntrarAlShellAsync();

        // Por aria-label y no por rol+nombre: al abrir la primera pestana aparece
        // otro boton llamado "Venta # 1" y el localizador dejaria de ser univoco.
        var atajoVenta = pagina.Locator("button[aria-label='Venta']");
        await atajoVenta.ClickAsync();
        await atajoVenta.ClickAsync();

        // Exact evita casar con el boton "Cerrar Venta # 1" de la propia pestana.
        await Assertions.Expect(
            pagina.GetByRole(AriaRole.Button, new() { Name = "Venta # 1", Exact = true }))
            .ToBeVisibleAsync(new() { Timeout = 30_000 });
        await Assertions.Expect(
            pagina.GetByRole(AriaRole.Button, new() { Name = "Venta # 2", Exact = true }))
            .ToBeVisibleAsync();
    }

    [HechoConCredenciales]
    public async Task LasPestanasSobrevivenARecargar()
    {
        var pagina = await EntrarAlShellAsync();

        await pagina.Locator("button[aria-label='Clientes']").ClickAsync();
        var pestana = pagina.GetByRole(AriaRole.Button, new() { Name = "Clientes", Exact = true });
        await Assertions.Expect(pestana).ToBeVisibleAsync(new() { Timeout = 30_000 });

        await pagina.ReloadAsync();

        await Assertions.Expect(pestana).ToBeVisibleAsync(new() { Timeout = 30_000 });
    }
}
