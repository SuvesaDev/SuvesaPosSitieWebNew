using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Levanta la aplicacion en un puerto libre para las pruebas de extremo a extremo,
/// y la apaga al terminar. Se comparte entre todas las pruebas de la coleccion:
/// arrancar Kestrel por cada prueba costaria mas que la prueba misma.
///
/// Corre en Development a proposito: es la configuracion que usan de verdad, y es
/// donde viven los fallos que estas pruebas buscan.
/// </summary>
public sealed class AplicacionEnPruebas : IAsyncLifetime
{
    private Process? _proceso;
    private readonly ConcurrentQueue<string> _salida = new();

    public string Url { get; private set; } = "";

    /// <summary>Ultimas lineas del log de la aplicacion, para diagnosticar un fallo.</summary>
    public string UltimasLineas(int cuantas = 40)
        => string.Join('\n', _salida.TakeLast(cuantas));

    public async Task InitializeAsync()
    {
        var puerto = PuertoLibre();
        Url = $"http://localhost:{puerto}";

        var raiz = RaizDelRepositorio();

        _proceso = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = raiz,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ArgumentList =
            {
                "run",
                // Sin recompilar: si el SDK decide reconstruir a mitad de la suite,
                // la aplicacion tarda en responder y las pruebas caducan navegando.
                "--no-build",
                "--project", Path.Combine(raiz, "src", "SuvesaPosSitioAplicacion"),
                "--urls", Url
            },
            Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Development" }
        });

        // HAY QUE DRENAR LAS TUBERIAS. Con RedirectStandardOutput activo y nadie
        // leyendo, el buffer del sistema (~64 KB) se llena y el proceso hijo se
        // BLOQUEA. En Development la aplicacion registra a nivel Debug, asi que eso
        // ocurre a mitad de la suite: las primeras pruebas pasan y a partir de ahi
        // todo caduca navegando, como si el servidor hubiera muerto.
        _proceso!.OutputDataReceived += (_, e) => Guardar(e.Data);
        _proceso.ErrorDataReceived += (_, e) => Guardar(e.Data);
        _proceso.BeginOutputReadLine();
        _proceso.BeginErrorReadLine();

        await EsperarAQueResponda();
    }

    public Task DisposeAsync()
    {
        if (_proceso is { HasExited: false })
        {
            _proceso.Kill(entireProcessTree: true);
            _proceso.WaitForExit(10_000);
        }

        _proceso?.Dispose();
        return Task.CompletedTask;
    }

    private void Guardar(string? linea)
    {
        if (linea is null)
        {
            return;
        }

        _salida.Enqueue(linea);

        // No crece sin limite: solo interesa lo reciente.
        while (_salida.Count > 500 && _salida.TryDequeue(out _))
        {
        }
    }

    private async Task EsperarAQueResponda()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var limite = DateTime.UtcNow.AddSeconds(90);

        while (DateTime.UtcNow < limite)
        {
            try
            {
                var r = await http.GetAsync($"{Url}/healthz");
                if (r.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Todavia arrancando.
            }

            await Task.Delay(500);
        }

        throw new InvalidOperationException(
            $"La aplicacion no respondio en {Url}/healthz dentro del plazo.\n\n{UltimasLineas()}");
    }

    private static int PuertoLibre()
    {
        using var escucha = new TcpListener(System.Net.IPAddress.Loopback, 0);
        escucha.Start();
        var puerto = ((System.Net.IPEndPoint)escucha.LocalEndpoint).Port;
        escucha.Stop();
        return puerto;
    }

    private static string RaizDelRepositorio()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SuvesaPosSitioWeb.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
               ?? throw new InvalidOperationException("No se encontro la raiz del repositorio.");
    }
}

[CollectionDefinition(Nombre)]
public sealed class ColeccionE2E
    : ICollectionFixture<AplicacionEnPruebas>, ICollectionFixture<NavegadorEnPruebas>
{
    public const string Nombre = "e2e";
}
