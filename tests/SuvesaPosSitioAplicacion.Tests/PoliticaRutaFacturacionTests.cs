using SuvesaPosSitioAplicacion.Services;
using static SuvesaPosSitioAplicacion.Services.PoliticaRutaFacturacion;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// W3 (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md §3): la matriz de rutas de la pantalla de
/// Facturación se decide desde la serie, no desde un bool "es crédito". Toda combinación
/// inválida cae en <see cref="PoliticaRutaFacturacion.Ruta.ConfiguracionInvalida"/>.
/// </summary>
public class PoliticaRutaFacturacionTests
{
    private static EntradaSerie Serie(bool tiquete, bool credito, bool electronico, string? fe, bool v44 = true)
        => new(tiquete, credito, electronico, v44, fe);

    [Fact]
    public void NoTiquete_Contado_Electronico_GuardaPreventa()
        => Assert.Equal(Ruta.GuardarPreventaContado, Resolver(Serie(false, false, true, "01")).Ruta);

    [Fact]
    public void NoTiquete_Contado_Interno_GuardaPreventa()
        => Assert.Equal(Ruta.GuardarPreventaContado, Resolver(Serie(false, false, false, null)).Ruta);

    [Fact]
    public void NoTiquete_Credito_Electronico_ConfirmaCredito()
        => Assert.Equal(Ruta.ConfirmarCredito, Resolver(Serie(false, true, true, "01")).Ruta);

    [Fact]
    public void NoTiquete_Credito_Interno_ConfirmaCredito()
        => Assert.Equal(Ruta.ConfirmarCredito, Resolver(Serie(false, true, false, null)).Ruta);

    [Fact]
    public void Tiquete_Contado_Electronico_04_CobraElectronico()
        => Assert.Equal(Ruta.CobrarTiqueteElectronico, Resolver(Serie(true, false, true, "04")).Ruta);

    [Fact]
    public void Tiquete_Contado_Interno_CobraInterno()
        => Assert.Equal(Ruta.CobrarTiqueteInterno, Resolver(Serie(true, false, false, null)).Ruta);

    [Fact]
    public void Tiquete_Credito_EsInvalido()
    {
        var r = Resolver(Serie(true, true, true, "04"));
        Assert.False(r.EsValida);
        Assert.Contains("crédito", r.Motivo, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tiquete_Electronico_ConCodigo01_EsInvalido()
        => Assert.False(Resolver(Serie(true, false, true, "01")).EsValida);

    [Fact]
    public void FacturaElectronica_ConCodigo04_EsInvalido()
        => Assert.False(Resolver(Serie(false, false, true, "04")).EsValida);

    [Fact]
    public void RequiereElectronico_SinV44_EsInvalido()
    {
        var r = Resolver(Serie(false, false, electronico: true, fe: "01", v44: false));
        Assert.False(r.EsValida);
        Assert.Contains("V4.4", r.Motivo!);
    }

    [Theory]
    [InlineData(Ruta.GuardarPreventaContado, "Guardar preventa")]
    [InlineData(Ruta.ConfirmarCredito, "Facturar a crédito")]
    [InlineData(Ruta.CobrarTiqueteElectronico, "Cobrar")]
    [InlineData(Ruta.CobrarTiqueteInterno, "Cobrar")]
    public void TextoAccion_PorRuta(Ruta ruta, string esperado)
        => Assert.Equal(esperado, TextoAccion(ruta));
}
