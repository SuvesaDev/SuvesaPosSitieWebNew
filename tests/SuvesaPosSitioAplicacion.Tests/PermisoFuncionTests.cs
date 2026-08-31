using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El permiso viaja como texto dentro de un claim
/// (<c>moduloCodigo|funcionCodigo|VER,CREAR,...</c>). Si el formato se rompe, un
/// usuario podria acabar viendo funciones que no le tocan, o al reves.
/// </summary>
public class PermisoFuncionTests
{
    private static PermisoFuncion Permiso(params string[] acciones)
        => new("INICIO", "INICIO.FACTURACION", acciones.ToHashSet());

    [Fact]
    public void ComponerYDescomponer_ConservaElPermiso()
    {
        var original = Permiso("VER", "CREAR", "IMPRIMIR");

        var vuelta = PermisoFuncion.DesdeClaim(original.AClaim());

        Assert.NotNull(vuelta);
        Assert.Equal("INICIO", vuelta!.ModuloCodigo);
        Assert.Equal("INICIO.FACTURACION", vuelta.FuncionCodigo);
        Assert.Equal(new[] { "CREAR", "IMPRIMIR", "VER" }, vuelta.Acciones.OrderBy(a => a));
    }

    [Fact]
    public void Descomponer_ConFormatoInvalido_DevuelveNulo()
    {
        Assert.Null(PermisoFuncion.DesdeClaim(""));
        Assert.Null(PermisoFuncion.DesdeClaim("basura"));
        Assert.Null(PermisoFuncion.DesdeClaim("INICIO|"));       // funcion vacia
        Assert.Null(PermisoFuncion.DesdeClaim("solo|dos"));      // faltan las acciones
    }

    [Theory]
    [InlineData(AccionPantalla.Ver, true)]
    [InlineData(AccionPantalla.Crear, true)]
    [InlineData(AccionPantalla.Editar, false)]
    [InlineData(AccionPantalla.Imprimir, true)]
    [InlineData(AccionPantalla.Exportar, false)]
    public void Permite_RespondeSegunLaAccion(AccionPantalla accion, bool esperado)
    {
        var permiso = Permiso("VER", "CREAR", "IMPRIMIR");
        Assert.Equal(esperado, permiso.Permite(accion));
    }

    [Fact]
    public void Modificar_EsAliasDeEditar()
    {
        var permiso = Permiso("EDITAR");
        Assert.True(permiso.Permite(AccionPantalla.Editar));
        Assert.True(permiso.Permite(AccionPantalla.Modificar));
    }

    [Fact]
    public void SinAcciones_NoConcedeNada()
    {
        var permiso = PermisoFuncion.DesdeClaim("INICIO|INICIO.FACTURACION|");
        Assert.NotNull(permiso);
        Assert.False(permiso!.Permite(AccionPantalla.Ver));
    }
}
