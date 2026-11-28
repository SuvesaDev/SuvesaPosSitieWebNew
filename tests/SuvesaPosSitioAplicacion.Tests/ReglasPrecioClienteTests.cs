using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Reglas de precio/descuento por cliente compartidas por Facturación y Proforma.</summary>
public class ReglasPrecioClienteTests
{
    private static readonly DateTime Hoy = new(2026, 11, 6);

    private static InventarioDTO Art(float a = 1000, float b = 900, float c = 800, float d = 700) => new()
    {
        Precio_A = a, Precio_B = b, Precio_C = c, Precio_D = d, MonedaCosto = 1,
    };

    [Theory]
    [InlineData(1, 1000)]
    [InlineData(2, 900)]
    [InlineData(3, 800)]
    [InlineData(4, 700)]
    public void CadaTarifaUsaSuPrecio(int tarifa, int esperado)
        => Assert.Equal(esperado, ReglasPrecioCliente.PrecioLista(Art(), tarifa, null, Hoy));

    [Fact]
    public void SinPrecioEnLaTarifa_CaeAlA()
        => Assert.Equal(1000m, ReglasPrecioCliente.PrecioLista(Art(b: 0), 2, null, Hoy));

    [Fact]
    public void TarifaNueva_UsaSuPrecioPorArticulo()
        => Assert.Equal(650m, ReglasPrecioCliente.PrecioLista(Art(), 5, 650m, Hoy));

    [Fact]
    public void PromocionVigente_ReemplazaAlA_PeroNoALasOtrasTarifas()
    {
        var a = Art();
        a.Promo_Activa = true; a.Promo_Inicio = Hoy.AddDays(-1); a.Promo_Finaliza = Hoy.AddDays(1); a.Precio_Promo = 800;
        Assert.Equal(800m, ReglasPrecioCliente.PrecioLista(a, 1, null, Hoy));
        Assert.Equal(900m, ReglasPrecioCliente.PrecioLista(a, 2, null, Hoy));
        a.Promo_Finaliza = Hoy.AddDays(-1);   // vencida
        Assert.Equal(1000m, ReglasPrecioCliente.PrecioLista(a, 1, null, Hoy));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(3, 3)]
    public void TarifaDelCliente(int tipo, int esperado) => Assert.Equal(esperado, ReglasPrecioCliente.TarifaDelCliente(tipo));

    [Theory]
    [InlineData(10.0, 5f, 5)]     // el descuento del cliente no pasa del máximo del artículo
    [InlineData(3.0, 5f, 3)]
    [InlineData(null, 5f, 0)]
    public void DescuentoInicial_RespetaElMaximoDelArticulo(double? cliente, float max, int esperado)
        => Assert.Equal(esperado, ReglasPrecioCliente.DescuentoInicial(cliente, max));
}
