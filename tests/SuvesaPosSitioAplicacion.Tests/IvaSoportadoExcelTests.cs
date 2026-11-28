using ClosedXML.Excel;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class IvaSoportadoExcelTests
{
    private static IvaSoportadoResultado Reporte() => new()
    {
        IdEmpresa = 7, Emisor = "Emisor", Identificacion = "3101", Desde = new DateTime(2026, 10, 1), Hasta = new DateTime(2026, 10, 31),
        Detalle =
        {
            new IvaSoportadoFila { Fecha = new DateTime(2026, 10, 2), Origen = "Compra", Documento = "F-1", Proveedor = "ISEMMA PARTNERS SRL", Cedula = "3101555666",
                                   Direccion = "100 m sur del parque", Telefono = "22223333", IvaTotal = 260 },
        },
    };

    private static XLWorkbook Abrir(byte[] bytes) => new(new MemoryStream(bytes));

    [Fact]
    public void ElDetalle_TraeCedulaDireccionYTelefonoDelProveedor_SinHojaLineasSiNoSePiden()
    {
        using var libro = Abrir(GeneradorIvaExcel.IvaSoportado(Reporte(), DateTime.Now));
        var d = libro.Worksheet("Detalle");
        Assert.Equal("Cédula", d.Cell(1, 5).GetString());
        Assert.Equal("3101555666", d.Cell(2, 5).GetString());
        Assert.Equal("100 m sur del parque", d.Cell(2, 6).GetString());
        Assert.Equal("22223333", d.Cell(2, 7).GetString());
        Assert.False(libro.Worksheets.Contains("Líneas"));
    }

    [Fact]
    public void HojaLineas_TraeCostoDescuentoImpuestoYTotal()
    {
        var lineas = new IvaSoportadoLineasPeriodo
        {
            Lineas = { new IvaSoportadoLineaPeriodo { Fecha = new DateTime(2026, 10, 2), Origen = "Compra", Documento = "F-1", Proveedor = "ISEMMA", Cedula = "3101555666",
                                                      Codigo = "A1", Descripcion = "Alimento 400g", Cantidad = 2, CostoUnitario = 1000, Subtotal = 2000, PorcentajeImpuesto = 13, Impuesto = 260, Total = 2260 } },
        };
        using var libro = Abrir(GeneradorIvaExcel.IvaSoportado(Reporte(), DateTime.Now, lineas));
        var l = libro.Worksheet("Líneas");
        Assert.Equal("Costo unitario", l.Cell(1, 9).GetString());
        Assert.Equal("Alimento 400g", l.Cell(2, 7).GetString());
        Assert.Equal(1000, l.Cell(2, 9).GetValue<decimal>());
        Assert.Equal(2260, l.Cell(2, 14).GetValue<decimal>());
    }
}
