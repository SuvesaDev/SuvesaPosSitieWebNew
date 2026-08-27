using SuvesaPosSitioAplicacion.ApiConexion;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El token tiene que atravesar el limite de ambitos entre el proxy y el handler
/// de HttpClient. Cuando no lo hacia, la cabecera Authorization no salia y el API
/// respondia 401 con la sesion aparentemente correcta.
/// </summary>
public class ContextoLlamadaTests
{
    [Fact]
    public async Task ElTokenSobreviveAlFlujoAsincrono()
    {
        // Es la condicion que hace falta: el handler corre dentro del await de
        // quien llama, no en el ambito de DI de la peticion.
        ContextoLlamada.Token = "token-de-prueba";

        var visto = await SimularHandler();

        Assert.Equal("token-de-prueba", visto);
    }

    [Fact]
    public async Task SinTokenPuesto_ElHandlerNoVeNada()
    {
        ContextoLlamada.Token = null;

        Assert.Null(await SimularHandler());
    }

    [Fact]
    public async Task CadaFlujoTieneElSuyo()
    {
        // Dos "peticiones" simultaneas no pueden pisarse el token.
        var a = Flujo("token-a");
        var b = Flujo("token-b");

        var resultados = await Task.WhenAll(a, b);

        Assert.Equal(new[] { "token-a", "token-b" }, resultados);
    }

    private static async Task<string?> SimularHandler()
    {
        await Task.Yield();
        return ContextoLlamada.Token;
    }

    private static async Task<string?> Flujo(string token)
    {
        await Task.Yield();
        ContextoLlamada.Token = token;
        await Task.Delay(10);
        return ContextoLlamada.Token;
    }
}
