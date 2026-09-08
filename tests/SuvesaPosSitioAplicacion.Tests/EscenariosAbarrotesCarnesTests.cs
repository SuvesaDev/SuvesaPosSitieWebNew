using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Casos diarios que se repiten en la venta y compra de un almacén: unidades de
/// abarrotes, kilos de carne, descuentos de proveedor y arqueo multimoneda.
/// </summary>
public class EscenariosAbarrotesCarnesTests
{
    [Fact]
    public void FacturaElectronica_MezclaAbarrotesYCarnePorKilos_SumaIgualQueElComprobante()
    {
        var arroz = CalculoDocumento.Linea(cantidad: 6m, precioUnitario: 1_250m, porcentajeDescuento: 0m, porcentajeImpuesto: 13m);
        var frijoles = CalculoDocumento.Linea(cantidad: 4m, precioUnitario: 1_800m, porcentajeDescuento: 5m, porcentajeImpuesto: 13m);
        var carneMolida = CalculoDocumento.Linea(cantidad: 2.35m, precioUnitario: 5_300m, porcentajeDescuento: 0m, porcentajeImpuesto: 13m);

        var total = CalculoDocumento.Totales([arroz, frijoles, carneMolida]);

        Assert.Equal(7_500m, arroz.SubTotal);
        Assert.Equal(975m, arroz.MontoImpuesto);
        Assert.Equal(360m, frijoles.MontoDescuento);
        Assert.Equal(889.20m, frijoles.MontoImpuesto);
        Assert.Equal(12_455m, carneMolida.SubTotal);
        Assert.Equal(1_619.15m, carneMolida.MontoImpuesto);
        Assert.Equal(27_155m, total.SubTotal);
        Assert.Equal(360m, total.Descuento);
        Assert.Equal(3_483.35m, total.Impuesto);
        Assert.Equal(30_278.35m, total.Total);
    }

    [Fact]
    public void PreventaDeCarnes_ConCantidadFraccionada_ConservaElMismoTotalAlPasarACobro()
    {
        // Una preventa no puede variar entre la báscula y Cobrar: 1.875 kg de
        // posta, con descuento de mostrador y el IVA calculado después de él.
        var posta = CalculoDocumento.Linea(1.875m, 6_450m, porcentajeDescuento: 3m, porcentajeImpuesto: 13m);

        Assert.Equal(12_093.75m, posta.SubTotal);
        Assert.Equal(362.81m, posta.MontoDescuento);
        Assert.Equal(1_525.02m, posta.MontoImpuesto);
        Assert.Equal(13_255.96m, posta.Total);
    }

    [Fact]
    public void CompraDelProveedor_DeCarneMolida_RespetaDescuentoAntesDelImpuesto()
    {
        // La misma aritmética se usa para recibir una factura de compra:
        // 40 kg a ₡3.800 con 2% de descuento comercial y 13% de IVA.
        var compra = CalculoDocumento.Linea(40m, 3_800m, porcentajeDescuento: 2m, porcentajeImpuesto: 13m);

        Assert.Equal(152_000m, compra.SubTotal);
        Assert.Equal(3_040m, compra.MontoDescuento);
        Assert.Equal(148_960m, compra.SubtotalGravado);
        Assert.Equal(19_364.80m, compra.MontoImpuesto);
        Assert.Equal(168_324.80m, compra.Total);
    }

    [Fact]
    public void ArqueoDeCaja_ConvierteDolaresAntesDeCompararConElCierre()
    {
        // Jornada: ₡120.000 declarados en efectivo y US$340 en caja, con tipo
        // de cambio de venta ₡522,50. No se pueden sumar monedas directamente.
        var total = CalculoArqueo.Total(colones: 120_000m, dolares: 340m, tipoCambio: 522.50m);

        Assert.Equal(297_650m, total);
    }
}
