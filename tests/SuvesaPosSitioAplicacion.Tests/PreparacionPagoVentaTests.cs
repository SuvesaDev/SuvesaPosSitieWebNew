using SuvesaPosSitioAplicacion.Services;
using static SuvesaPosSitioAplicacion.Services.PreparacionPagoVenta;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// W4 (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md §4): reparto del cobro entre formas de pago.
/// Invariante recibido − vuelto = aplicado; el vuelto sale solo de efectivo.
/// </summary>
public class PreparacionPagoVentaTests
{
    private static LineaPago Efe(decimal r) => new("EFE", r, EsEfectivo: true, RequiereReferencia: false, null);
    private static LineaPago Tarj(decimal r, string? refx = "1234") => new("TAR", r, EsEfectivo: false, RequiereReferencia: true, refx);

    [Fact]
    public void EfectivoExacto_CubreSinVuelto()
    {
        var r = Calcular(10000m, new[] { Efe(10000m) });
        Assert.True(r.Cubre100);
        Assert.Equal(0m, r.Vuelto);
        Assert.Equal(10000m, r.AplicadoTotal);
    }

    [Fact]
    public void EfectivoDeMas_DaVuelto_YSigueCubriendo()
    {
        var r = Calcular(10000m, new[] { Efe(12000m) });
        Assert.True(r.Cubre100);
        Assert.Equal(2000m, r.Vuelto);
        Assert.Equal(10000m, r.AplicadoTotal);
        Assert.Equal(2000m, r.Formas.Single().Vuelto);
    }

    [Fact]
    public void TarjetaMasEfectivo_ConVuelto_ReparteYCubre()
    {
        // Escenario E02 del plan: tarjeta 4.000 + efectivo recibido 8.000, vuelto 2.000.
        var r = Calcular(10000m, new[] { Tarj(4000m), Efe(8000m) });
        Assert.True(r.Cubre100);
        Assert.Equal(2000m, r.Vuelto);
        Assert.Equal(10000m, r.AplicadoTotal);
        Assert.Equal(4000m, r.Formas.First(f => f.Codigo == "TAR").Aplicado);
        Assert.Equal(6000m, r.Formas.First(f => f.Codigo == "EFE").Aplicado);
        Assert.Equal(2000m, r.Formas.First(f => f.Codigo == "EFE").Vuelto);
    }

    [Fact]
    public void PagoInsuficiente_NoCubre_YReportaFaltante()
    {
        var r = Calcular(10000m, new[] { Efe(6000m) });
        Assert.False(r.Cubre100);
        Assert.Equal(4000m, r.Faltante);
    }

    [Fact]
    public void SobrepagoEnTarjeta_EsError_NoVueltoFicticio()
    {
        var r = Calcular(10000m, new[] { Tarj(12000m) });
        Assert.False(r.Cubre100);
        Assert.Contains(r.Errores, e => e.Contains("no es efectivo"));
        Assert.Equal(0m, r.Vuelto);
    }

    [Fact]
    public void TarjetaSinReferencia_EsError()
    {
        var r = Calcular(10000m, new[] { Tarj(10000m, refx: null) });
        Assert.False(r.Cubre100);
        Assert.Contains(r.Errores, e => e.Contains("referencia"));
    }

    [Fact]
    public void SinFormas_EsError()
    {
        var r = Calcular(10000m, System.Array.Empty<LineaPago>());
        Assert.False(r.Cubre100);
        Assert.Contains(r.Errores, e => e.Contains("al menos una"));
    }

    [Fact]
    public void Invariante_RecibidoMenosVuelto_IgualAplicado()
    {
        var r = Calcular(10000m, new[] { Tarj(3000m), Efe(9000m) });
        Assert.Equal(r.AplicadoTotal, r.RecibidoTotal - r.Vuelto);
    }

    private static LineaPago Dep(decimal r, string? refx, int? banco = 1) => new("DEP", r, EsEfectivo: false, RequiereReferencia: true, refx, RequiereBanco: true, IdBanco: banco);

    [Fact]
    public void VariosDepositos_SumanYCubren()
    {
        var r = Calcular(10000m, new[] { Dep(6000m, "A-1"), Dep(4000m, "A-2") });
        Assert.True(r.Cubre100);
        Assert.Equal(10000m, r.RecibidoTotal);
        Assert.Equal(2, r.Formas.Count(f => f.Codigo == "DEP"));
    }

    [Fact]
    public void DosDepositosConMismoBancoYReferencia_EsError()
    {
        var r = Calcular(10000m, new[] { Dep(5000m, "A-1"), Dep(5000m, " a-1 ") });
        Assert.False(r.Cubre100);
        Assert.Contains(r.Errores, e => e.Contains("mismo banco y la misma referencia"));
    }

    [Fact]
    public void MismaReferenciaEnBancosDistintos_EsValida()
    {
        var r = Calcular(10000m, new[] { Dep(5000m, "A-1", banco: 1), Dep(5000m, "A-1", banco: 2) });
        Assert.True(r.Cubre100);
    }

    [Fact]
    public void ElAvisoDiceQueLineaFalla()
    {
        var r = Calcular(10000m, new[] { Dep(5000m, "A-1"), Dep(5000m, null) });
        Assert.Contains(r.Errores, e => e.Contains("DEP (línea 2)") && e.Contains("referencia"));
    }

    [Fact]
    public void MasDeCincoLineasDeLaMismaForma_EsError()
    {
        var lineas = Enumerable.Range(1, 6).Select(i => Dep(1000m, "R" + i)).ToArray();
        var r = Calcular(6000m, lineas);
        Assert.Contains(r.Errores, e => e.Contains("máximo 5"));
    }
}
