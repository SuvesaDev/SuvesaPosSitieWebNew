using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// GetFamiliasAsync devuelve la respuesta como `object` (el swagger no la tipo),
/// asi que Familias.Obtener la reinterpreta a mano con System.Text.Json. Sin esta
/// prueba, un cambio en esa deserializacion fallaria en silencio: compilaria, y
/// solo se veria al abrir la pantalla con datos reales.
/// </summary>
[Trait("Categoria", "RequiereCredenciales")]
public class FamiliasDeserializacionTests
{
    [HechoConCredenciales]
    public async Task ElObjectSeDeserializaAListaDeFamiliaDTO()
    {
        var url = new Uri(CredencialesPrueba.Api);

        HttpClient Cliente() => new(
            new SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler(
                NullLogger<SuvesaPosSitioAplicacion.Helpers.ApiAuthHeaderHandler>.Instance)
            { InnerHandler = new HttpClientHandler() })
        { BaseAddress = url };

        var seguridad = new Seguridad(
            new UsuarioApiCliente(Cliente()),
            new CentrosApiCliente(Cliente()),
            new SesionFija(),
            NullLogger<Seguridad>.Instance);

        var login = await seguridad.Login(CredencialesPrueba.Usuario!, CredencialesPrueba.Password!);
        Assert.True(login.EsCorrecta, login.Excepcion);

        var familias = new Familias(
            new FamiliasApiCliente(Cliente()),
            new SesionFija(login.Responses!.Token),
            NullLogger<Familias>.Instance);

        var r = await familias.Obtener();

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.NotNull(r.Responses);
        Assert.NotEmpty(r.Responses!);

        var primera = r.Responses!.First();
        Assert.True(primera.Codigo > 0);
        Assert.False(string.IsNullOrWhiteSpace(primera.Descripcion));
    }
}
