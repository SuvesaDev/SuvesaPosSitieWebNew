using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El borde entre la coma flotante que entrega el API y el decimal con el que se
/// debe operar. Si esto se rompe, se rompe en importes.
/// </summary>
public class FormatoTests
{
    [Fact]
    public void Importe_UsaDosDecimalesYSeparadorDeMiles()
    {
        Assert.Equal("28.500,00", Formato.Importe(28500m));
        Assert.Equal("2.090,91", Formato.Importe(2090.91m));
        Assert.Equal("0,00", Formato.Importe(0m));
    }

    [Fact]
    public void Cantidad_OmiteDecimalesCuandoEsEntera()
    {
        Assert.Equal("42", Formato.Cantidad(42m));
        Assert.Equal("1.480", Formato.Cantidad(1480m));
        Assert.Equal("2,50", Formato.Cantidad(2.5m));
    }

    [Fact]
    public void AImporte_ConvierteDirectoYNoDestapaElRuidoDelFloat()
    {
        // Pasar el float por double destapa su ruido binario: 2090.91f vista como
        // double es 2090.909912109375. El cast directo a decimal respeta los ~7
        // digitos significativos que el float guarda de verdad.
        Assert.NotEqual(2090.91d, (double)2090.91f);

        Assert.Equal(2090.91m, Formato.AImporte(2090.91f));
    }

    [Fact]
    public void AImporte_DesdeDouble_ConservaElValor()
    {
        Assert.Equal(2090.91m, Formato.AImporte(2090.91d));
        Assert.Equal(28500m, Formato.AImporte(28500d));
    }

    [Fact]
    public void AImporte_NoInventaPrecisionQueElApiYaPerdio()
    {
        // Un float de precision simple guarda ~7 digitos significativos. Con un
        // importe de ocho digitos el valor YA llego degradado: la conversion
        // conserva lo que hay, no lo arregla.
        var degradado = Formato.AImporte(12345678.91f);

        Assert.NotEqual(12345678.91m, degradado);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void AImporte_ConValoresNoFinitos_DevuelveCeroYNoRevienta(float valor)
    {
        Assert.Equal(0m, Formato.AImporte(valor));
    }

    [Fact]
    public void AImporte_FueraDelRangoDeDecimal_DevuelveCero()
    {
        Assert.Equal(0m, Formato.AImporte(1e30));
        Assert.Equal(0m, Formato.AImporte(-1e30));
    }

    [Fact]
    public void Importe_NuloSeMuestraComoRaya()
    {
        Assert.Equal("—", Formato.Importe((decimal?)null));
    }
}
