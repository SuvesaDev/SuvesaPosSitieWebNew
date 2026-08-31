using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Security;
using Xunit.Abstractions;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Ejercita contra el API real todo lo que quedo sin verificar en las semanas 2 y 3.
/// No usa navegador: llama a la misma capa ApiConexion que usa el sitio.
/// </summary>
[Collection(ColeccionE2E.Nombre)]
[Trait("Categoria", "RequiereCredenciales")]
public class SesionRealTests
{
    private readonly ITestOutputHelper _salida;

    public SesionRealTests(ITestOutputHelper salida) => _salida = salida;

    /// <summary>
    /// Construye el proxy con el handler de autenticacion en la tuberia, igual que
    /// en la aplicacion: asi la prueba ejercita tambien el paso del token.
    /// </summary>
    private static Seguridad CrearProxy(string? token = null)
    {
        var url = new Uri(CredencialesPrueba.Api);

        HttpClient Cliente() => new(
            new SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler(
                NullLogger<SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler>.Instance)
            {
                InnerHandler = new HttpClientHandler()
            })
        { BaseAddress = url };

        return new Seguridad(
            new UsuarioApiCliente(Cliente()),
            new CentrosApiCliente(Cliente()),
            new SesionFija(token),
            NullLogger<Seguridad>.Instance);
    }

    [HechoConCredenciales]
    public async Task Login_DevuelveTokenYPermisos()
    {
        var r = await CrearProxy().Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.NotNull(r.Responses);
        Assert.False(string.IsNullOrWhiteSpace(r.Responses!.Token));

        var permisos = r.Responses.Permisos ?? new();

        _salida.WriteLine($"usuario        : {r.Responses.Usuario}");
        _salida.WriteLine($"perfil         : {r.Responses.Perfil?.Codigo} (super={r.Responses.Perfil?.EsSuperAdministracion})");
        _salida.WriteLine($"permisos       : {permisos.Count}");
        _salida.WriteLine($"expiracion     : {r.Responses.Expiracion:O}");
        _salida.WriteLine($"tamano token   : {r.Responses.Token!.Length} caracteres");
    }

    [HechoConCredenciales]
    public async Task ObtenerSucursales_DevuelveCentros()
    {
        var login = await CrearProxy().Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);
        Assert.True(login.EsCorrecta, login.Excepcion);

        // Segundo proxy, ya con el token de la sesion: recorre el mismo camino que
        // la aplicacion, incluido ApiAuthHeaderHandler.
        var r = await CrearProxy(login.Responses!.Token).ObtenerSucursales();

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.NotNull(r.Responses);

        foreach (var s in r.Responses!)
        {
            _salida.WriteLine($"  [{s.Id}] {s.Alias ?? s.NombreComercial}");
        }
    }

    /// <summary>
    /// Rediseno V2: los permisos casan por CODIGO de funcion. Todo codigo que el API
    /// devuelve en <c>permisos[]</c> tiene que existir como codigo de nodo en el menu
    /// del sitio (si no, esa concesion no abre nada). ASERCION: ya no solo informa.
    /// </summary>
    [HechoConCredenciales]
    public async Task LosCodigosDeFuncionDelApiExistenEnElMenu()
    {
        var r = await CrearProxy().Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);
        Assert.True(r.EsCorrecta, r.Excepcion);

        var delApi = (r.Responses!.Permisos ?? new())
            .Select(p => p.FuncionCodigo)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (delApi.Count == 0)
        {
            _salida.WriteLine("El usuario (¿SUPER_ADMIN?) no trae permisos por funcion; nada que comparar.");
            return;
        }

        var delMenu = CodigosDelMenu().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var huerfanos = delApi.Where(c => !delMenu.Contains(c)).OrderBy(c => c).ToList();

        _salida.WriteLine($"funciones del API : {delApi.Count}");
        _salida.WriteLine($"codigos del menu  : {delMenu.Count}");
        foreach (var c in huerfanos) _salida.WriteLine($"   huerfano: {c}");

        Assert.True(huerfanos.Count == 0,
            "El API concede permiso sobre funciones que el menu no tiene: " + string.Join(", ", huerfanos) +
            ". Regenera la semilla del API o corre tools/anotar_codigos_menu.py.");
    }

    private static IEnumerable<string> CodigosDelMenu()
    {
        static IEnumerable<string> Recorrer(IEnumerable<Models.ItemMenu> ns)
        {
            foreach (var n in ns)
            {
                if (!string.IsNullOrWhiteSpace(n.Codigo)) yield return n.Codigo!;
                foreach (var h in Recorrer(n.Hijos)) yield return h;
            }
        }

        return Recorrer(MenuSeePos.Items);
    }
}
