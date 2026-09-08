using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Regla del grupo de bonificación (docs/BONIFICACION_DISENO_WEB.md §4.3.d).</summary>
public class BonificacionCalculoTests
{
    private static BonificacionCalculo.ArticuloUsado A(long cod, decimal precio, int cant, decimal imp = 13m)
        => new(cod, $"Art {cod}", precio, imp, cant);

    [Fact]
    public void CompraDiezLlevaUno_ConUnSoloArticulo_RegalaLaUnidadMasBarata()
    {
        // Config 10 lleva 1 factura 11 unidades: 10 pagadas + 1 gratis.
        var r = BonificacionCalculo.ResolverGrupo(10, 1, new[] { A(1, 1000m, 11) });

        Assert.True(r.Ok);
        Assert.Equal(2, r.Lineas.Count);
        var pagada = r.Lineas.Single(l => !l.EsBonificacion);
        var regalo = r.Lineas.Single(l => l.EsBonificacion);
        Assert.Equal(10, pagada.Cantidad);
        Assert.Equal(10000m, pagada.Calculo.SubTotal);
        Assert.Equal(1, regalo.Cantidad);
        Assert.Equal(0m, regalo.Calculo.SubTotal);
        Assert.Equal(130m, regalo.Calculo.MontoImpuesto);   // 13% de 1000, aunque la linea es 0
        Assert.Equal(1000m, regalo.PrecioUnit);             // conserva el precio de lista: base del impuesto en Facturacion
    }

    [Fact]
    public void ConMezcla_ElRegaloEsElArticuloMasBarato()
    {
        // Fanta 700 (x9) + Coca 900 (x2) = 11. Config 10 lleva 1 -> regala 1 Fanta.
        var r = BonificacionCalculo.ResolverGrupo(10, 1, new[] { A(10, 700m, 9), A(20, 900m, 2) });

        Assert.True(r.Ok);
        var regalo = r.Lineas.Single(l => l.EsBonificacion);
        Assert.Equal(10, regalo.Codigo);          // la Fanta, mas barata
        Assert.Equal(1, regalo.Cantidad);
        Assert.Equal(8, r.Lineas.Single(l => l.Codigo == 10 && !l.EsBonificacion).Cantidad);
        Assert.Equal(2, r.Lineas.Single(l => l.Codigo == 20).Cantidad);
    }

    [Fact]
    public void ExcederLaCantidadDeLaConfiguracion_Falla()
    {
        var r = BonificacionCalculo.ResolverGrupo(10, 1, new[] { A(1, 100m, 8), A(2, 100m, 5) }); // 13 > 11
        Assert.False(r.Ok);
        Assert.Contains("excede", r.Error);
    }

    [Fact]
    public void RegalaVariasUnidades_CuandoCantidadBonificableEsMayorAUno()
    {
        // Config 12 lleva 2 factura 14 unidades: 12 pagadas + 2 gratis, todo el mismo articulo.
        var r = BonificacionCalculo.ResolverGrupo(12, 2, new[] { A(1, 500m, 14) });
        Assert.True(r.Ok);
        Assert.Equal(12, r.Lineas.Single(l => !l.EsBonificacion).Cantidad);
        Assert.Equal(2, r.Lineas.Single(l => l.EsBonificacion).Cantidad);
    }
}
