using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Views.Reportes;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class CatalogoReportesTests
{
    [Fact]
    public void Catalogo_Tiene29ReportesUnicosEn8Dominios()
    {
        Assert.Equal(8, CatalogoReportes.Grupos.Length);
        Assert.Equal(29, CatalogoReportes.Todos.Count);
        Assert.Equal(29, CatalogoReportes.Todos.Select(x => x.Tipo).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(29, CatalogoReportes.Todos.Select(x => x.Ruta).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Catalogo_TodoReporteTienePropositoPermisoYFiltrosBasicosValidos()
    {
        foreach (var reporte in CatalogoReportes.Todos)
        {
            Assert.False(string.IsNullOrWhiteSpace(reporte.Titulo));
            Assert.False(string.IsNullOrWhiteSpace(reporte.Descripcion));
            Assert.False(string.IsNullOrWhiteSpace(reporte.Codigo));
            Assert.Equal(FiltroReporteVisible.Ninguno, reporte.FiltrosBasicos & ~reporte.Filtros);
        }
    }

    [Fact]
    public void Filtros_SonEspecificosDelDominio()
    {
        var inventario = CatalogoReportes.Buscar("inventario")!;
        Assert.True(inventario.Tiene(FiltroReporteVisible.Articulo));
        Assert.False(inventario.Tiene(FiltroReporteVisible.Cliente));
        Assert.False(inventario.Tiene(FiltroReporteVisible.Proveedor));

        var compras = CatalogoReportes.Buscar("compras")!;
        Assert.True(compras.Tiene(FiltroReporteVisible.Proveedor));
        Assert.False(compras.Tiene(FiltroReporteVisible.Cliente));
        Assert.False(compras.Tiene(FiltroReporteVisible.Bodega));
    }

    [Fact]
    public void Catalogo_CoincideConLasHojasCanonicasDelMenu()
    {
        var raiz = MenuSeePos.Items.Single(x => x.Codigo == "MODULO_REPORTES");
        var hojasMenu = raiz.Hijos.ToDictionary(x => x.Codigo!, StringComparer.OrdinalIgnoreCase);
        var hojasCatalogo = CatalogoReportes.Todos
            .Where(x => x.Codigo.StartsWith("MODULO_REPORTES.", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(hojasMenu.Count, hojasCatalogo.Length);
        foreach (var reporte in hojasCatalogo)
        {
            var hoja = Assert.Contains(reporte.Codigo, hojasMenu);
            Assert.Equal(hoja.Ruta, reporte.Ruta);
        }
    }
}
