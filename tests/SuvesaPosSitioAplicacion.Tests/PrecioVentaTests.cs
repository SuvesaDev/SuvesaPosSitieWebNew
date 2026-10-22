using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public class PrecioVentaTests
{
    [Fact]
    public void Colones_NoConvierte()
    {
        var a = new InventarioDTO { MonedaCosto = 1, ConviertePrecioColones = true, TipoCambioVenta = 500 };
        Assert.Equal(22.77m, PrecioVenta.EnColones(a, 22.77m));
    }

    [Fact]
    public void Dolares_SinCasilla_NoConvierte()
    {
        var a = new InventarioDTO { MonedaCosto = 2, ConviertePrecioColones = false, TipoCambioVenta = 500 };
        Assert.Equal(22.77m, PrecioVenta.EnColones(a, 22.77m));
    }

    [Fact]
    public void Dolares_UsaValorVentaCongelado()
    {
        var a = new InventarioDTO { MonedaCosto = 2, ConviertePrecioColones = true, TipoCambioVenta = 500 };
        Assert.Equal(11385m, PrecioVenta.EnColones(a, 22.77m));
    }

    [Fact]
    public void TarifasBcdYPromo_UsanElMismoFactorQueA()
    {
        var a = new InventarioDTO
        {
            MonedaCosto = 2,
            ConviertePrecioColones = true,
            TipoCambioVenta = 500,
            Precio_A = 10,
            Precio_B = 9,
            Precio_C = 8,
            Precio_D = 7,
            Precio_Promo = 6
        };
        Assert.Equal(5000m, PrecioVenta.EnColones(a, a.Precio_A));
        Assert.Equal(4500m, PrecioVenta.EnColones(a, a.Precio_B));
        Assert.Equal(4000m, PrecioVenta.EnColones(a, a.Precio_C));
        Assert.Equal(3500m, PrecioVenta.EnColones(a, a.Precio_D));
        Assert.Equal(3000m, PrecioVenta.EnColones(a, a.Precio_Promo));
    }
}
