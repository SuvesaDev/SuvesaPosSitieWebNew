using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Aritmetica de documento. Es lo que acaba en una factura electronica, asi que un
/// centimo de diferencia no es un defecto cosmetico.
/// </summary>
public class CalculoDocumentoTests
{
    [Fact]
    public void LineaSimple_SinDescuentoNiImpuesto()
    {
        var l = CalculoDocumento.Linea(cantidad: 3, precioUnitario: 1000m,
            porcentajeDescuento: 0, porcentajeImpuesto: 0);

        Assert.Equal(3000m, l.SubTotal);
        Assert.Equal(0m, l.MontoDescuento);
        Assert.Equal(0m, l.MontoImpuesto);
        Assert.Equal(3000m, l.Total);
    }

    [Fact]
    public void ElImpuestoSeAplicaDESPUESDelDescuento()
    {
        // Es el orden del sistema actual:
        //   Monto_Impuesto = (SubTotal - Monto_Descuento) * (Impuesto / 100)
        // Invertirlo cambia el total y seria una diferencia fiscal.
        var l = CalculoDocumento.Linea(1, 1000m, porcentajeDescuento: 10, porcentajeImpuesto: 13);

        Assert.Equal(1000m, l.SubTotal);
        Assert.Equal(100m, l.MontoDescuento);
        Assert.Equal(900m, l.SubtotalGravado);
        Assert.Equal(117m, l.MontoImpuesto);   // 13% de 900, no de 1000
        Assert.Equal(1017m, l.Total);
    }

    [Fact]
    public void ImpuestoDelTreceSobreUnPrecioConDecimales()
    {
        var l = CalculoDocumento.Linea(1, 2090.91m, 0, 13);

        Assert.Equal(2090.91m, l.SubTotal);
        Assert.Equal(271.82m, l.MontoImpuesto);   // 271.8183 -> 271.82
        Assert.Equal(2362.73m, l.Total);
    }

    [Fact]
    public void NoSeRedondeaAMitadDeCamino()
    {
        // Si se redondeara el descuento antes de calcular el impuesto, el resultado
        // cambiaria. Se comprueba con un caso donde ese redondeo intermedio se nota.
        var l = CalculoDocumento.Linea(1, 333.33m, porcentajeDescuento: 7, porcentajeImpuesto: 13);

        // descuento = 23.3331, gravado = 309.9969, impuesto = 40.299597
        Assert.Equal(23.33m, l.MontoDescuento);
        Assert.Equal(310.00m, l.SubtotalGravado);
        Assert.Equal(40.30m, l.MontoImpuesto);
        Assert.Equal(350.30m, l.Total);
    }

    [Fact]
    public void CantidadFraccionada()
    {
        // Se venden kilos y fracciones, no solo unidades.
        var l = CalculoDocumento.Linea(2.5m, 1200m, 0, 13);

        Assert.Equal(3000m, l.SubTotal);
        Assert.Equal(390m, l.MontoImpuesto);
        Assert.Equal(3390m, l.Total);
    }

    [Fact]
    public void TotalesSumanLasLineas()
    {
        var lineas = new[]
        {
            CalculoDocumento.Linea(2, 1000m, 0, 13),
            CalculoDocumento.Linea(1, 500m, 10, 13),
            CalculoDocumento.Linea(3, 250m, 0, 0)
        };

        var t = CalculoDocumento.Totales(lineas);

        Assert.Equal(3250m, t.SubTotal);
        Assert.Equal(50m, t.Descuento);
        Assert.Equal(318.50m, t.Impuesto);   // 260 + 58.50 + 0
        Assert.Equal(3518.50m, t.Total);
    }

    [Fact]
    public void LosImportesQueLleganEnDoubleSeConviertenEnElBorde()
    {
        // El API entrega los precios en double. La regla es convertir con
        // Formato.AImporte y operar en decimal, nunca sumar doubles.
        double precioDelApi = 2090.91;
        double cantidadDelApi = 3;

        var l = CalculoDocumento.Linea(
            Formato.AImporte(cantidadDelApi),
            Formato.AImporte(precioDelApi),
            0, 13);

        Assert.Equal(6272.73m, l.SubTotal);
        Assert.Equal(815.45m, l.MontoImpuesto);
        Assert.Equal(7088.18m, l.Total);
    }

    [Fact]
    public void SumarEnDoubleDaOtroResultadoQueSumarEnDecimal()
    {
        // Documenta por que existe la regla. 0.1 + 0.2 en double no da 0.3.
        double enDouble = 0;
        for (var i = 0; i < 10; i++) enDouble += 0.1;

        decimal enDecimal = 0;
        for (var i = 0; i < 10; i++) enDecimal += 0.1m;

        Assert.NotEqual(1.0d, enDouble);
        Assert.Equal(1.0m, enDecimal);
    }

    [Theory]
    [InlineData(0.005, 0.01)]
    [InlineData(0.015, 0.02)]
    [InlineData(0.025, 0.03)]
    public void ElEmpateRedondeaAlAlza(double valor, double esperado)
    {
        // MidpointRounding.AwayFromZero, no el "al par" que trae .NET por defecto.
        // Con el de por defecto, 0.025 daria 0.02.
        Assert.Equal((decimal)esperado, CalculoDocumento.Redondear((decimal)valor));
    }

    [Fact]
    public void LineaBonificada_PrecioCeroYElEmisorAsumeElImpuesto()
    {
        // §4.4: el articulo de regalo sale gratis y el 13% se calcula sobre el
        // precio de lista (1000) para reportarlo a Hacienda, pero Hacienda exige
        // que ese impuesto lo asuma el emisor (rechazo -476), no el cliente.
        var l = CalculoDocumento.LineaBonificada(cantidad: 2, precioReferencia: 1000m, porcentajeImpuesto: 13);

        Assert.Equal(0m, l.SubTotal);
        Assert.Equal(0m, l.SubtotalGravado);
        Assert.Equal(0m, l.MontoDescuento);
        Assert.Equal(260m, l.MontoImpuesto);   // 13% de 2000, se reporta a Hacienda igual
        Assert.Equal(0m, l.Total);             // el cliente no paga nada por el regalo
    }
}
