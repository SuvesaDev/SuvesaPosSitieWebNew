using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Antes del ajuste de columnas por tipo de reporte, varios tipos caían al agrupamiento
/// genérico por defecto (por día, o por Estado) aunque sus filas no tuvieran Fecha real,
/// Monto/Cantidad poblados, o un Estado que no fuera casi único por fila — el resultado
/// era una gráfica vacía o de un solo balde sin ningún sentido de dominio (plan de
/// reportes web, bug #7). Estos casos fijan que cada uno ahora produce una serie real.
/// </summary>
public sealed class ResumenGraficasReporteOperacionTests
{
    [Fact]
    public void VentasHoras_Evolucion_UsaFranjaHorariaNoFecha()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Entidad = "09:00 – 09:59", Monto = 5000m },
            new FilaReporteOperacionWebDTO { Entidad = "08:00 – 08:59", Monto = 3000m },
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("ventas-horas", filas);

        Assert.Equal(2, resumen.Evolucion.Count);
        Assert.Equal("08:00 – 08:59", resumen.Evolucion[0].Etiqueta);
        Assert.Equal("09:00 – 09:59", resumen.Evolucion[1].Etiqueta);
    }

    [Fact]
    public void KpiRutas_Evolucion_NoQuedaVaciaSinFecha()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Entidad = "Ruta 1", Monto = 100000m, Saldo = 5000m },
            new FilaReporteOperacionWebDTO { Entidad = "Ruta 2", Monto = 50000m, Saldo = 1000m },
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("kpi-rutas", filas);

        Assert.NotEmpty(resumen.Evolucion);
        Assert.Contains(resumen.Evolucion, p => p.Etiqueta == "Ruta 1" && p.Valor == 5000m);
    }

    [Fact]
    public void InventarioAbc_Evolucion_RankeaPorVentaEnVezDeQuedarVacia()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Entidad = "Artículo A", Monto = 90000m, Estado = "Clase A" },
            new FilaReporteOperacionWebDTO { Entidad = "Artículo B", Monto = 10000m, Estado = "Clase C" },
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("inventario-abc", filas);

        Assert.Equal("Artículo A", resumen.Evolucion[0].Etiqueta);
    }

    [Fact]
    public void Cabys_UsaConteoEnVezDeMontoSiempreCero()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 1), Entidad = "Artículo A", Estado = "Válido" },
            new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 1), Entidad = "Artículo B", Estado = "Sin CABYS" },
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("cabys", filas);

        Assert.True(resumen.EsCantidad);
        Assert.Equal(2m, resumen.Evolucion.Sum(p => p.Valor));
        Assert.Contains(resumen.Estados, p => p.Etiqueta == "Sin CABYS" && p.Valor == 1m);
    }

    [Fact]
    public void Rentabilidad_Comparativo_AgrupaPorClienteNoPorEstadoCasiUnico()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Entidad = "Cliente A", Monto = 20000m, Estado = "Comisión: 1500.00" },
            new FilaReporteOperacionWebDTO { Entidad = "Cliente A", Monto = 5000m, Estado = "Comisión: 200.00" },
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("rentabilidad", filas);

        Assert.Single(resumen.Estados);
        Assert.Equal("Cliente A", resumen.Estados[0].Etiqueta);
        Assert.Equal(25000m, resumen.Estados[0].Valor);
    }

    [Fact]
    public void VentasCompras_Comparativo_AgrupaPorMesNoPorEstadoConstante()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 1), Saldo = 1000m, Estado = "Ventas menos compras" },
            new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 15), Saldo = 500m, Estado = "Ventas menos compras" },
            new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 8, 20), Saldo = -200m, Estado = "Ventas menos compras" },
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("ventas-compras", filas);

        Assert.Equal(2, resumen.Estados.Count);
    }
}
