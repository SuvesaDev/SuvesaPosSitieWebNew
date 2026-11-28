using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class RepartoLotesDevolucionTests
{
    private static CalculoDocumento.LineaCalculada Calc(decimal sub, decimal imp) => new(sub, 0m, imp, sub, sub + imp);

    [Fact]
    public void SinLotes_UnaLineaSinLote()
    {
        var p = RepartoLotesDevolucion.Partir(2m, Calc(100m, 13m), Array.Empty<(long, decimal)>());
        var u = Assert.Single(p);
        Assert.Null(u.IdStockLote);
        Assert.Equal(2m, u.Cantidad);
    }

    [Fact]
    public void UnLote_UnaLineaConEseLote_YLosImportesIntactos()
    {
        var p = RepartoLotesDevolucion.Partir(2m, Calc(100m, 13m), new[] { (308L, 2m) });
        var u = Assert.Single(p);
        Assert.Equal(308, u.IdStockLote);
        Assert.Equal(113m, u.Calculo.Total);
    }

    [Fact]
    public void VariosLotes_UnaLineaPorLote_YLaSumaNoPierdeCentavos()
    {
        var calc = Calc(100m, 13m);
        var p = RepartoLotesDevolucion.Partir(3m, calc, new[] { (1L, 1m), (2L, 1m), (3L, 1m) });

        Assert.Equal(3, p.Count);
        Assert.Equal(new long?[] { 1, 2, 3 }, p.Select(x => x.IdStockLote).ToArray());
        Assert.Equal(3m, p.Sum(x => x.Cantidad));
        Assert.Equal(calc.SubTotal, p.Sum(x => x.Calculo.SubTotal));
        Assert.Equal(calc.MontoImpuesto, p.Sum(x => x.Calculo.MontoImpuesto));
        Assert.Equal(calc.Total, p.Sum(x => x.Calculo.Total));
    }

    [Fact]
    public void LineaDeRegalo_ConTotalCero_ConservaTotalCeroEnCadaParte()
    {
        var regalo = new CalculoDocumento.LineaCalculada(50m, 0m, 6.5m, 50m, 0m);
        var p = RepartoLotesDevolucion.Partir(2m, regalo, new[] { (1L, 1m), (2L, 1m) });
        Assert.All(p, x => Assert.Equal(0m, x.Calculo.Total));
        Assert.Equal(6.5m, p.Sum(x => x.Calculo.MontoImpuesto));
    }
}
