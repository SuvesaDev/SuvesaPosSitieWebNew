using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El API y el menu no escriben igual los mismos titulos. Comparando en crudo,
/// esas pantallas desaparecen del menu para cualquier rol no administrador.
/// </summary>
public class NombrePantallaTests
{
    [Theory]
    [InlineData("Facturacion", "Facturación")]
    [InlineData("Consignacion", "Consignación")]
    [InlineData("PROFORMAS O COTIZACION", "Proformas o Cotización")]
    [InlineData("  Bancos  ", "Bancos")]
    public void CasosRealesDelApi_SeReconocenComoElMismoNombre(string delApi, string delMenu)
    {
        Assert.True(NombrePantalla.Comparador.Equals(delApi, delMenu));
    }

    [Fact]
    public void NombresDistintos_SiguenSiendoDistintos()
    {
        // No se trata de que todo case: eso abriria pantallas que no toca.
        Assert.False(NombrePantalla.Comparador.Equals("Facturación", "Devoluciones"));
        Assert.False(NombrePantalla.Comparador.Equals("Compra", "Compras"));
    }

    [Fact]
    public void SirveComoLlaveDeDiccionario()
    {
        var d = new Dictionary<string, int>(NombrePantalla.Comparador)
        {
            ["Facturación"] = 1
        };

        Assert.True(d.ContainsKey("Facturacion"));
        Assert.Equal(1, d["FACTURACION"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValoresVacios_NoRevientan(string? nombre)
    {
        Assert.Equal(string.Empty, NombrePantalla.Normalizar(nombre));
    }
}
