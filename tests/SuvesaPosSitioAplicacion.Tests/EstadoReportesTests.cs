using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Services;
using SuvesaPosSitioAplicacion.Views.Reportes;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class EstadoReportesTests
{
    private sealed class AlmacenFalso : IAlmacenEspacioTrabajo
    {
        public Task<EstadoEspacioGuardado?> LeerAsync() => Task.FromResult<EstadoEspacioGuardado?>(null);
        public Task GuardarAsync(EstadoEspacioGuardado estado) => Task.CompletedTask;
    }

    private static (EstadoEspacioTrabajo espacio, EstadoReportes reportes) Crear()
    {
        var espacio = new EstadoEspacioTrabajo(new AlmacenFalso(), NullLogger<EstadoEspacioTrabajo>.Instance);
        return (espacio, new EstadoReportes(espacio));
    }

    [Fact]
    public void CadaReporteConservaUnContextoIndependiente()
    {
        var (_, reportes) = Crear();
        var ventas = reportes.Obtener("ventas");
        var compras = reportes.Obtener("compras");
        ventas.Texto = "Cliente A";
        compras.Texto = "Proveedor B";

        Assert.Equal("Cliente A", reportes.Obtener("ventas").Texto);
        Assert.Equal("Proveedor B", reportes.Obtener("compras").Texto);
        Assert.NotSame(ventas, compras);
    }

    [Fact]
    public void CerrarPestanaDescartaSoloSuReporte()
    {
        var (espacio, reportes) = Crear();
        var ventas = reportes.Obtener("ventas");
        var compras = reportes.Obtener("compras");
        ventas.Texto = "A";
        compras.Texto = "B";
        var pestanaVentas = espacio.Abrir("Ventas", "/moduloReportes/ventas");
        espacio.Abrir("Compras", "/moduloReportes/compras");

        espacio.Cerrar(pestanaVentas.Id);

        Assert.NotSame(ventas, reportes.Obtener("ventas"));
        Assert.Same(compras, reportes.Obtener("compras"));
    }

    [Fact]
    public void CerrarTodasDescartaTodosLosContextos()
    {
        var (espacio, reportes) = Crear();
        var ventas = reportes.Obtener("ventas");
        espacio.Abrir("Ventas", "/moduloReportes/ventas");

        espacio.CerrarTodas();

        Assert.NotSame(ventas, reportes.Obtener("ventas"));
    }

    [Fact]
    public void DesgloseHeredaElFiltroAplicadoYNoCambiosSinConsultar()
    {
        var (_, reportes) = Crear();
        var origen = reportes.Obtener("panel-ejecutivo");
        origen.Desde = new DateTime(2026, 9, 1);
        origen.Hasta = new DateTime(2026, 9, 30);
        origen.IdSucursal = 9;
        origen.FiltroAplicado = new FiltroReporteOperacionWebDTO
        {
            Desde = new DateTime(2026, 8, 1),
            Hasta = new DateTime(2026, 8, 31),
            IdSucursal = 4
        };

        var destino = reportes.Obtener("ventas");
        destino.CopiarFiltrosDesde(origen, CatalogoReportes.Buscar("ventas")!);

        Assert.Equal(new DateTime(2026, 8, 1), destino.Desde);
        Assert.Equal(new DateTime(2026, 8, 31), destino.Hasta);
        Assert.Equal(4, destino.IdSucursal);
    }
}
