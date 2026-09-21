using ClosedXML.Excel;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Verifica los dos formatos exportables y que ambos incluyan las gráficas.</summary>
public sealed class GeneradorReporteOperacionTests
{
    static GeneradorReporteOperacionTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void ExcelYPdf_ConservanDetalleEIncluyenGraficas()
    {
        var reporte = new ReporteOperacionWebDTO
        {
            Titulo = "Reporte de ventas",
            Fuente = "Ventas registradas",
            Desde = new DateTime(2026, 9, 1),
            Hasta = new DateTime(2026, 9, 3),
            Indicadores =
            [
                new IndicadorReporteOperacionWebDTO { Etiqueta = "Total", Valor = 45000m },
                new IndicadorReporteOperacionWebDTO { Etiqueta = "Documentos", Valor = 3m, Formato = "cantidad" }
            ],
            Filas =
            [
                new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 1, 9, 10, 0), Referencia = "1", Entidad = "Cliente A", Lote = "LT-001", Descripcion = "Factura", Cantidad = 1m, Monto = 12000m, Saldo = 0m, Estado = "Aceptado" },
                new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 2, 10, 30, 0), Referencia = "2", Entidad = "Cliente B", Descripcion = "Factura", Cantidad = 1m, Monto = 18000m, Saldo = 0m, Estado = "Aceptado" },
                new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 3, 11, 45, 0), Referencia = "3", Entidad = "Cliente C", Descripcion = "Factura", Cantidad = 1m, Monto = 15000m, Saldo = 0m, Estado = "Pendiente" }
            ]
        };
        var generador = new GeneradorReporteOperacion();
        var fechaEquipo = new DateTime(2026, 9, 10, 8, 30, 0);

        var pdf = generador.Pdf("ventas", reporte, fechaEquipo);
        var excel = generador.Excel("ventas", reporte, fechaEquipo);

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
        using var libro = new XLWorkbook(new MemoryStream(excel));
        Assert.Equal("Reporte de ventas", libro.Worksheet("Reporte").Cell(1, 1).GetString());
        Assert.Equal("Ventas por día", libro.Worksheet("Gráficas").Cell(4, 1).GetString());
        Assert.Equal("Clientes con mayor venta", libro.Worksheet("Gráficas").Cell(16, 1).GetString());
        Assert.Equal(3, libro.Worksheet("Reporte").Table("DetalleReporteOperacion").DataRange.RowCount());
        Assert.Equal("Lote", libro.Worksheet("Reporte").Cell(9, 4).GetString());
        Assert.Equal("LT-001", libro.Worksheet("Reporte").Cell(10, 4).GetString());

        // Regresión: el formato de fecha de la columna "Fecha" del detalle se aplicaba
        // a la columna 1 ENTERA, pisando el formato numérico del primer indicador (que
        // también cae en la columna 1) — Excel mostraba el monto como una fecha
        // absurda (p. ej. "16/05/2130"). El indicador debe seguir siendo un número.
        var celdaIndicador = libro.Worksheet("Reporte").Cell(6, 1);
        Assert.Equal(45000m, celdaIndicador.GetValue<decimal>());
        Assert.DoesNotContain("yyyy", celdaIndicador.Style.NumberFormat.Format, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GraficasCxP_UsanSaldosYBandasDeAntiguedad()
    {
        var filas = new[]
        {
            new FilaReporteOperacionWebDTO { Entidad = "Proveedor A", Saldo = 100m, Estado = "31–60 días" },
            new FilaReporteOperacionWebDTO { Entidad = "Proveedor B", Saldo = 250m, Estado = "0–30 días" },
            new FilaReporteOperacionWebDTO { Entidad = "Proveedor A", Saldo = 50m, Estado = "0–30 días" }
        };

        var resumen = ResumenGraficasReporteOperacion.Crear("cuentas-por-pagar", filas);

        Assert.Equal("Antigüedad de cuentas por pagar", resumen.TituloEvolucion);
        Assert.Equal(["0–30 días", "31–60 días"], resumen.Evolucion.Select(x => x.Etiqueta));
        Assert.Equal(300m, resumen.Evolucion[0].Valor);
        Assert.Equal("Proveedor B", resumen.Estados[0].Etiqueta);
        Assert.Equal(250m, resumen.Estados[0].Valor);
    }

    [Fact]
    public void ExportacionesClientes_IncluyenDatosDeLaFicha()
    {
        var reporte = new ReporteOperacionWebDTO
        {
            Titulo = "Clientes y comportamiento",
            Desde = new DateTime(2026, 9, 1),
            Hasta = new DateTime(2026, 9, 30),
            Indicadores = [new IndicadorReporteOperacionWebDTO { Etiqueta = "Clientes con venta", Valor = 1, Formato = "cantidad" }],
            Filas =
            [
                new FilaReporteOperacionWebDTO
                {
                    Fecha = new DateTime(2026, 9, 10, 7, 46, 0),
                    Referencia = "67",
                    Entidad = "Rafael Alberto Martínez Quesada",
                    NombreFantasia = "Veterinaria Rafael",
                    Identificacion = "1-2345-6789",
                    Telefono = "8888-9999",
                    Sucursal = "Sucursal Escazú",
                    Cantidad = 35,
                    Monto = 390_379.81m,
                    Saldo = 11_153.71m,
                    Descripcion = "Crecimiento vs. período anterior: 0,00%",
                    Estado = "Con ventas"
                }
            ]
        };
        var generador = new GeneradorReporteOperacion();

        var pdf = generador.Pdf("clientes", reporte, new DateTime(2026, 9, 16, 9, 0, 0));
        var excel = generador.Excel("clientes", reporte, new DateTime(2026, 9, 16, 9, 0, 0));

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
        using var libro = new XLWorkbook(new MemoryStream(excel));
        var hoja = libro.Worksheet("Reporte");
        var tabla = hoja.Table("DetalleReporteOperacion");
        var filaCabecera = tabla.RangeAddress.FirstAddress.RowNumber;
        Assert.Equal("Nombre fantasía", hoja.Cell(filaCabecera, 3).GetString());
        Assert.Equal("Identificación", hoja.Cell(filaCabecera, 4).GetString());
        Assert.Equal("Teléfono", hoja.Cell(filaCabecera, 5).GetString());
        Assert.Equal("Sucursal", hoja.Cell(filaCabecera, 6).GetString());
        Assert.Equal("Veterinaria Rafael", tabla.DataRange.FirstRow().Cell(3).GetString());
        Assert.Equal("1-2345-6789", tabla.DataRange.FirstRow().Cell(4).GetString());
        Assert.Equal("8888-9999", tabla.DataRange.FirstRow().Cell(5).GetString());
        Assert.Equal("Sucursal Escazú", tabla.DataRange.FirstRow().Cell(6).GetString());
    }

    [Fact]
    public void ExportacionesCuentasPorCobrar_IncluyenTelefonoYCorreo()
    {
        var reporte = new ReporteOperacionWebDTO
        {
            Titulo = "Cuentas por cobrar",
            Desde = new DateTime(2026, 8, 18),
            Hasta = new DateTime(2026, 9, 16),
            Indicadores = [new IndicadorReporteOperacionWebDTO { Etiqueta = "Saldo pendiente", Valor = 24_253.19m }],
            Filas =
            [
                new FilaReporteOperacionWebDTO
                {
                    Entidad = "Clínica Veterinaria La Sabana",
                    Telefono = "8777-6655",
                    Correo = "cobros@lasabana.example",
                    Referencia = "2",
                    FechaVencimiento = new DateTime(2026, 10, 5),
                    Saldo = 24_253.19m,
                    Estado = "Pendiente",
                    Descripcion = "Saldo histórico sin asiento en mayor"
                }
            ]
        };
        var generador = new GeneradorReporteOperacion();

        var pdf = generador.Pdf("cuentas-por-cobrar", reporte, new DateTime(2026, 9, 16, 9, 0, 0));
        var excel = generador.Excel("cuentas-por-cobrar", reporte, new DateTime(2026, 9, 16, 9, 0, 0));

        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
        using var libro = new XLWorkbook(new MemoryStream(excel));
        var hoja = libro.Worksheet("Reporte");
        var tabla = hoja.Table("DetalleReporteOperacion");
        var filaCabecera = tabla.RangeAddress.FirstAddress.RowNumber;
        Assert.Equal("Cliente", hoja.Cell(filaCabecera, 1).GetString());
        Assert.Equal("Teléfono", hoja.Cell(filaCabecera, 2).GetString());
        Assert.Equal("Correo", hoja.Cell(filaCabecera, 3).GetString());
        Assert.Equal("Clínica Veterinaria La Sabana", tabla.DataRange.FirstRow().Cell(1).GetString());
        Assert.Equal("8777-6655", tabla.DataRange.FirstRow().Cell(2).GetString());
        Assert.Equal("cobros@lasabana.example", tabla.DataRange.FirstRow().Cell(3).GetString());
    }
}
