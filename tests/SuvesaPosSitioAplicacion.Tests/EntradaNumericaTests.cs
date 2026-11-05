using System.Globalization;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Un campo numerico del navegador entrega "0.5" con punto aunque la pantalla corra en
/// es-CR (coma decimal). Antes se leia con la cultura del servidor y el decimal se perdia.
/// </summary>
public class EntradaNumericaTests
{
    [Theory]
    [InlineData("0.5", 0.5)]
    [InlineData("1.25", 1.25)]
    [InlineData("3", 3)]
    [InlineData("0", 0)]
    public void TryLeer_AceptaElPuntoDecimalDelNavegador_AunConCulturaCostaRica(string texto, double esperado)
    {
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-CR");
            Assert.True(EntradaNumerica.TryLeer(texto, out var valor));
            Assert.Equal(esperado, valor);
        }
        finally { CultureInfo.CurrentCulture = anterior; }
    }

    [Fact]
    public void TryLeer_ConservaElSignoParaQueLaPantallaRechaceSoloLosNegativos()
    {
        Assert.True(EntradaNumerica.TryLeer("-2", out var valor));
        Assert.Equal(-2, valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void TryLeer_RechazaLoQueNoEsUnNumero(string? texto) =>
        Assert.False(EntradaNumerica.TryLeer(texto, out _));

    [Fact]
    public void Texto_EscribeConPuntoParaElCampoNumerico_AunConCulturaCostaRica()
    {
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-CR");
            Assert.Equal("0.5", EntradaNumerica.Texto(0.5));
            Assert.Equal(string.Empty, EntradaNumerica.Texto(null));
        }
        finally { CultureInfo.CurrentCulture = anterior; }
    }
}
