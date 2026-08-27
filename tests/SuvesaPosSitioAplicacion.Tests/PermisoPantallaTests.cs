using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El permiso viaja como texto dentro de un claim. Si el formato se rompe, un usuario
/// podria acabar viendo pantallas que no le tocan, o al reves. Por eso se prueba.
/// </summary>
public class PermisoPantallaTests
{
    [Fact]
    public void ComponerYDescomponer_ConservaElPermiso()
    {
        var original = new PermisoPantalla(
            Menu: "Ventas",
            Pantalla: "Facturacion",
            Ver: true,
            Crear: true,
            Modificar: false,
            Borrar: false);

        var texto = original.AClaim();
        var vuelta = PermisoPantalla.DesdeClaim(texto);

        Assert.Equal(original, vuelta);
    }

    [Fact]
    public void Descomponer_ConFormatoInvalido_DevuelveNulo()
    {
        Assert.Null(PermisoPantalla.DesdeClaim("Ventas|Facturacion|1"));
        Assert.Null(PermisoPantalla.DesdeClaim(""));
        Assert.Null(PermisoPantalla.DesdeClaim("basura"));
    }

    [Theory]
    [InlineData(AccionPantalla.Ver, true)]
    [InlineData(AccionPantalla.Crear, true)]
    [InlineData(AccionPantalla.Modificar, false)]
    [InlineData(AccionPantalla.Borrar, false)]
    public void Permite_RespondeSegunLaAccion(AccionPantalla accion, bool esperado)
    {
        var permiso = new PermisoPantalla("Ventas", "Facturacion",
            Ver: true, Crear: true, Modificar: false, Borrar: false);

        Assert.Equal(esperado, permiso.Permite(accion));
    }

    [Fact]
    public void Componer_NoPierdeElMenuVacio()
    {
        // El API devuelve menu nulo en algunas pantallas; no debe romper el formato.
        var permiso = new PermisoPantalla("", "Inicio", true, false, false, false);

        var vuelta = PermisoPantalla.DesdeClaim(permiso.AClaim());

        Assert.NotNull(vuelta);
        Assert.Equal("Inicio", vuelta!.Pantalla);
        Assert.True(vuelta.Ver);
    }
}
