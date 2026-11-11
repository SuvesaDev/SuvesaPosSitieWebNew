using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>La marca «esDeducible» de la cuenta viaja en los dos sentidos aunque los contratos generados todavía no la traigan.</summary>
public class CuentaDeducibleContratoTests
{
    [Fact]
    public void LaCuentaQueLlegaDelApiTraeSuMarcaDeDeducible()
    {
        var cuenta = JsonSerializer.Deserialize<CuentaContableDTO>("""{"id":1,"codigo":"6.1.6.9","descripcion":"Faltante de caja","esDeducible":false}""")!;

        Assert.False(cuenta.EsDeducible);
        Assert.True(JsonSerializer.Deserialize<CuentaContableDTO>("""{"id":2,"codigo":"6.1.1.1"}""")!.EsDeducible);   // sin dato: deducible
    }

    [Fact]
    public void LaEdicionEnviaLaMarcaSoloSiSeDefinio()
    {
        var conMarca = JsonSerializer.Serialize(new EditarCuentaContableDTO { Descripcion = "x", EsDeducible = false });
        var sinMarca = JsonSerializer.Serialize(new EditarCuentaContableDTO { Descripcion = "x" });

        Assert.Contains("\"esDeducible\":false", conMarca);
        Assert.Contains("\"esDeducible\":null", sinMarca);     // el API lee null como «dejar como está»
    }
}
