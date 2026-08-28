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

        var permisos = r.Responses.Rol?.Permisos ?? (ICollection<PermisosDTO>)Array.Empty<PermisosDTO>();

        _salida.WriteLine($"usuario        : {r.Responses.Usuario}");
        _salida.WriteLine($"administrador  : {r.Responses.Administrador}");
        _salida.WriteLine($"rol            : {r.Responses.Rol?.NombreRol}");
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
    /// El riesgo que mas me preocupa del menu: los permisos casan por TITULO, no por
    /// ruta. Si un NombrePantalla del API no coincide exactamente con el titulo del
    /// menu, esa pantalla desaparece para todo el que no sea administrador.
    /// </summary>
    [HechoConCredenciales]
    public async Task LosNombresDePantallaDelApiCoincidenConElMenu()
    {
        var r = await CrearProxy().Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);
        Assert.True(r.EsCorrecta, r.Excepcion);

        var delApi = (r.Responses!.Rol?.Permisos ?? (ICollection<PermisosDTO>)Array.Empty<PermisosDTO>())
            .Select(p => p.NombrePantalla)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (delApi.Count == 0)
        {
            _salida.WriteLine("El usuario no trae permisos por pantalla; nada que comparar.");
            return;
        }

        var delMenu = TitulosDelMenu().ToHashSet(StringComparer.OrdinalIgnoreCase);

        var huerfanos = delApi.Where(n => !delMenu.Contains(n)).OrderBy(n => n).ToList();
        var sinPermiso = delMenu.Where(t => !delApi.Contains(t, StringComparer.OrdinalIgnoreCase))
                                .OrderBy(t => t).ToList();

        _salida.WriteLine($"pantallas del API  : {delApi.Count}");
        _salida.WriteLine($"titulos del menu   : {delMenu.Count}");
        _salida.WriteLine("");
        _salida.WriteLine($"-- del API que NO estan en el menu ({huerfanos.Count}):");
        foreach (var n in huerfanos) _salida.WriteLine($"     {n}");
        _salida.WriteLine("");
        _salida.WriteLine($"-- del menu que el API no menciona ({sinPermiso.Count}):");
        foreach (var t in sinPermiso) _salida.WriteLine($"     {t}");

        // No se afirma nada todavia: primero hay que ver el desfase real.
        // Cuando se conozca, esto pasa a ser una asercion.
    }

    private static IEnumerable<string> TitulosDelMenu()
    {
        static IEnumerable<string> Recorrer(IEnumerable<Models.ItemMenu> ns)
        {
            foreach (var n in ns)
            {
                yield return n.Titulo;
                foreach (var h in Recorrer(n.Hijos))
                {
                    yield return h;
                }
            }
        }

        return Recorrer(MenuSeePos.Items);
    }
}
