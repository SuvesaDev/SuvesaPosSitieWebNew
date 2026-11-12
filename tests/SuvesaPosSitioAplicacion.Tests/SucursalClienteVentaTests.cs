using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class SucursalClienteVentaTests
{
    private static List<ClienteDatosSucursalDTO> Lista(params long[] ids)
        => ids.Select(i => new ClienteDatosSucursalDTO { Id = i, Sucursal = "S" + i, NombreComercial = "N" + i }).ToList();

    [Fact]
    public void VariasSucursales_SonObligatorias_YSeMuestraElSelector()
    {
        var l = Lista(1, 2);
        Assert.True(SucursalClienteVenta.Obligatoria(l));
        Assert.True(SucursalClienteVenta.MostrarSelector(l));
        Assert.NotNull(SucursalClienteVenta.Validar(l, null));
        Assert.NotNull(SucursalClienteVenta.Validar(l, 0));
        Assert.Null(SucursalClienteVenta.Validar(l, 2));
    }

    [Fact]
    public void UnaSolaSucursal_SePropone_YNoSeExige()
    {
        var l = Lista(7);
        Assert.False(SucursalClienteVenta.Obligatoria(l));
        Assert.False(SucursalClienteVenta.MostrarSelector(l));
        Assert.Equal(7, SucursalClienteVenta.Predeterminada(l, null));
        Assert.Null(SucursalClienteVenta.Validar(l, null));
    }

    [Fact]
    public void SinSucursales_NoHayNadaQueElegir()
    {
        var l = Lista();
        Assert.Null(SucursalClienteVenta.Predeterminada(l, null));
        Assert.Null(SucursalClienteVenta.Validar(l, null));
    }

    [Fact]
    public void Predeterminada_ConservaLaActualSoloSiEsDelCliente()
    {
        var l = Lista(1, 2);
        Assert.Equal(2, SucursalClienteVenta.Predeterminada(l, 2));
        Assert.Null(SucursalClienteVenta.Predeterminada(l, 99));
        Assert.Contains("no pertenece", SucursalClienteVenta.Validar(l, 99));
    }

    [Fact]
    public void Etiqueta_NoRepiteNombresIguales()
    {
        Assert.Equal("Centro - Super Pets", SucursalClienteVenta.Etiqueta(new ClienteDatosSucursalDTO { Id = 1, Sucursal = "Centro", NombreComercial = "Super Pets" }));
        Assert.Equal("Centro", SucursalClienteVenta.Etiqueta(new ClienteDatosSucursalDTO { Id = 1, Sucursal = "Centro", NombreComercial = "centro" }));
    }
}
