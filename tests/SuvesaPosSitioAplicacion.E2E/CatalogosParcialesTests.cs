using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Categorias y Presentaciones tienen un ciclo de escritura incompleto en el API
/// (sin edicion en un caso, escritura unificada en el otro). Se verifica que la
/// lectura de ambos trae datos reales con la forma esperada.
/// </summary>
public class CatalogosParcialesTests
{
    private static HttpClient Cliente(Uri url) => new(
        new SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler(
            NullLogger<SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler>.Instance)
        { InnerHandler = new HttpClientHandler() })
    { BaseAddress = url };

    private static async Task<string> TokenAsync()
    {
        var url = new Uri(CredencialesPrueba.Api);
        var seguridad = new Seguridad(
            new UsuarioApiCliente(Cliente(url)),
            new CentrosApiCliente(Cliente(url)),
            new SesionFija(),
            NullLogger<Seguridad>.Instance);

        var login = await seguridad.Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);
        Assert.True(login.EsCorrecta, login.Excepcion);
        return login.Responses!.Token!;
    }

    [HechoConCredenciales]
    public async Task Categorias_TraeDatosReales()
    {
        var url = new Uri(CredencialesPrueba.Api);
        var token = await TokenAsync();

        var categorias = new Categorias(
            new CategoriasApiCliente(Cliente(url)),
            new SesionFija(token),
            NullLogger<Categorias>.Instance);

        var r = await categorias.Obtener();

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.NotNull(r.Responses);
        Assert.NotEmpty(r.Responses!);
        Assert.False(string.IsNullOrWhiteSpace(r.Responses!.First().Descripcion));
    }

    [HechoConCredenciales]
    public async Task Presentaciones_TraeDatosRealesConCodigo()
    {
        var url = new Uri(CredencialesPrueba.Api);
        var token = await TokenAsync();

        var presentaciones = new Presentaciones(
            new PresentacionApiCliente(Cliente(url)),
            new SesionFija(token),
            NullLogger<Presentaciones>.Instance);

        var r = await presentaciones.Obtener();

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.NotNull(r.Responses);
        Assert.NotEmpty(r.Responses!);

        var primera = r.Responses!.First();
        // El DTO de lectura (Presentacione) trae CodPres; el de escritura
        // (PresentacionDTO) no lo tiene, por eso son tipos distintos.
        Assert.True(primera.CodPres > 0);
        Assert.False(string.IsNullOrWhiteSpace(primera.Presentaciones));
    }
}
