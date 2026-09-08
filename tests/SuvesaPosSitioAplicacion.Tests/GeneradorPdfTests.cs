using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Que el PDF se genere de verdad. Compilar no basta: QuestPDF valida el diseno en
/// tiempo de ejecucion y falla ahi si la tabla esta mal armada.
/// </summary>
public class GeneradorPdfTests
{
    static GeneradorPdfTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private static ReporteTabular Reporte(int filas) => new(
        Titulo: "Cuentas por pagar",
        Subtitulo: "Facturas pendientes por proveedor",
        Encabezados: new[] { "Proveedor", "Factura", "Fecha", "Monto", "Saldo" },
        Filas: Enumerable.Range(1, filas)
            .Select(i => (IReadOnlyList<string>)new[]
            {
                $"Proveedor {i}", $"F-{i:0000}", "27/08/2026", "125.000,00", "80.000,00"
            })
            .ToList(),
        Totales: new[] { "", "", "", "Total", "1.000.000,00" })
    {
        ColumnasNumericas = new HashSet<int> { 3, 4 }
    };

    [Fact]
    public void GeneraUnPdfValido()
    {
        var bytes = new GeneradorPdfQuestPdf().Tabla(Reporte(10));

        Assert.NotEmpty(bytes);

        // Todo PDF empieza por %PDF-
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(bytes, 0, 5));
    }

    [Fact]
    public void ConMuchasFilas_PaginaSinRomperse()
    {
        // El pie lleva "pagina X de Y": si la tabla no puede partirse, QuestPDF
        // lanza excepcion al pasar de una pagina.
        var bytes = new GeneradorPdfQuestPdf().Tabla(Reporte(400));

        Assert.True(bytes.Length > 10_000, "Un reporte de 400 filas deberia pesar mas.");
    }

    [Fact]
    public void SinFilas_SigueGenerando()
    {
        // Un reporte vacio no puede reventar: es un caso normal, no un error.
        var bytes = new GeneradorPdfQuestPdf().Tabla(Reporte(0));

        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void SinTotales_SigueGenerando()
    {
        var sinTotales = new ReporteTabular(
            "Listado", null,
            new[] { "Codigo", "Descripcion" },
            new[] { (IReadOnlyList<string>)new[] { "001", "Articulo" } });

        Assert.NotEmpty(new GeneradorPdfQuestPdf().Tabla(sinTotales));
    }
}
