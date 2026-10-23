using SuvesaPosSitioAplicacion.DTOs.Bodega;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>La zona del cliente solo ayuda a armar el manifiesto: decide si un bulto cae dentro de la ruta.</summary>
public sealed class RutaDespachoTests
{
    private static BultoDespacho Bulto(int? provincia, int? canton = null, int? distrito = null)
        => new() { IdProvincia = provincia, IdCanton = canton, IdDistrito = distrito };

    [Fact]
    public void UnaProvinciaCompletaCubreCualquierCantonDeEsaProvincia()
    {
        var ruta = new RutaDespacho { Zonas = { new RutaZona { IdProvincia = 4 } } };
        Assert.True(ruta.CubreA(Bulto(4, 401, 40101)));
        Assert.True(ruta.CubreA(Bulto(4)));
        Assert.False(ruta.CubreA(Bulto(3, 301)));
    }

    [Fact]
    public void UnCantonConcretoNoCubreOtrosCantonesDeLaProvincia()
    {
        var ruta = new RutaDespacho { Zonas = { new RutaZona { IdProvincia = 3, IdCanton = 301 } } };
        Assert.True(ruta.CubreA(Bulto(3, 301, 30101)));
        Assert.False(ruta.CubreA(Bulto(3, 302)));
    }

    [Fact]
    public void UnDistritoConcretoSoloCubreEseDistrito()
    {
        var ruta = new RutaDespacho { Zonas = { new RutaZona { IdProvincia = 3, IdCanton = 301, IdDistrito = 30101 } } };
        Assert.True(ruta.CubreA(Bulto(3, 301, 30101)));
        Assert.False(ruta.CubreA(Bulto(3, 301, 30102)));
    }

    [Fact]
    public void VariasZonasYUnClienteSinZonaEnLaFicha()
    {
        var ruta = new RutaDespacho { Zonas = { new RutaZona { IdProvincia = 4 }, new RutaZona { IdProvincia = 3, IdCanton = 301 } } };
        Assert.True(ruta.CubreA(Bulto(4, 402)));
        Assert.True(ruta.CubreA(Bulto(3, 301)));
        Assert.False(ruta.CubreA(Bulto(null)));          // sin zona: no se puede afirmar que cae en la ruta
        Assert.False(new RutaDespacho().CubreA(Bulto(4)));  // ruta sin zonas
    }
}
