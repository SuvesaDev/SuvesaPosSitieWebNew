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
                new FilaReporteOperacionWebDTO { Fecha = new DateTime(2026, 9, 1, 9, 10, 0), Referencia = "1", Entidad = "Cliente A", Descripcion = "Factura", Cantidad = 1m, Monto = 12000m, Saldo = 0m, Estado = "Aceptado" },
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
        Assert.Equal("Monto por estado", libro.Worksheet("Gráficas").Cell(16, 1).GetString());
        Assert.Equal(3, libro.Worksheet("Reporte").Table("DetalleReporteOperacion").DataRange.RowCount());
    }
}
