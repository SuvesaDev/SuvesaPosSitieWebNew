using Microsoft.Playwright;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Navegador compartido por las pruebas.
///
/// Usa el Chrome instalado en el equipo (<c>Channel = "chrome"</c>) en lugar de
/// descargar los navegadores propios de Playwright: son cientos de megas y aqui no
/// hacen falta, porque lo que se prueba es la aplicacion y no la compatibilidad
/// entre navegadores.
/// </summary>
public sealed class NavegadorEnPruebas : IAsyncLifetime
{
    private IPlaywright? _playwright;

    public IBrowser Navegador { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        Navegador = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "chrome",
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (Navegador is not null)
        {
            await Navegador.CloseAsync();
        }

        _playwright?.Dispose();
    }

    /// <summary>Contexto limpio: cada prueba empieza sin cookies ni almacenamiento.</summary>
    public Task<IBrowserContext> ContextoNuevoAsync(string urlBase)
        => Navegador.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = urlBase,
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
            IgnoreHTTPSErrors = true
        });
}
