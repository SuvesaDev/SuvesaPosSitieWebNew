using ClosedXML.Excel;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class IvaRepercutidoExcelTests
{
    private static IvaRepercutidoResultado Reporte() => new()
    {
        IdEmpresa = 7, Emisor = "Emisor", Identificacion = "3101", Desde = new DateTime(2026, 10, 1), Hasta = new DateTime(2026, 10, 31),
        Detalle =
        {
            new IvaDetalleFila { Fecha = new DateTime(2026, 10, 2), Tipo = "Factura", Documento = "001", Cliente = "Vet Los Pinos SA", Cedula = "3101123456",
                                 NombreFantasia = "Vet Los Pinos", Ubicacion = "San José, Escazú, San Rafael", Base = 1000, Iva = 130 },
        },
    };

    private static IvaLineasPeriodo Lineas(bool costo) => new()
    {
        IncluyeCosto = costo,
        Lineas = { new IvaLineaPeriodo { Fecha = new DateTime(2026, 10, 2), Tipo = "Factura", Documento = "001", Cliente = "Vet Los Pinos SA", Cedula = "3101123456",
                                         CodArticulo = "A1", Descripcion = "Alimento BARF", Cantidad = 2, PrecioUnitario = 600, Descuento = 200, Subtotal = 1000,
                                         PorcentajeImpuesto = 13, Impuesto = 130, Total = 1130, Costo = costo ? 600 : null, Utilidad = costo ? 400 : null } },
    };

    private static XLWorkbook Abrir(byte[] bytes) => new(new MemoryStream(bytes));

    [Fact]
    public void ElDetalle_TraeCedulaFantasiaYUbicacion()
    {
        using var libro = Abrir(GeneradorIvaExcel.IvaRepercutido(Reporte(), DateTime.Now));
        var d = libro.Worksheet("Detalle");
        Assert.Equal("Cédula", d.Cell(1, 6).GetString());
        Assert.Equal("3101123456", d.Cell(2, 6).GetString());
        Assert.Equal("Vet Los Pinos", d.Cell(2, 7).GetString());
        Assert.Equal("San José, Escazú, San Rafael", d.Cell(2, 8).GetString());
        Assert.False(libro.Worksheets.Contains("Líneas"));
    }

    [Fact]
    public void HojaLineas_ConCosto_TraeCostoYUtilidad()
    {
        using var libro = Abrir(GeneradorIvaExcel.IvaRepercutido(Reporte(), DateTime.Now, Lineas(costo: true)));
        var l = libro.Worksheet("Líneas");
        var cabeceras = Enumerable.Range(1, 20).Select(i => l.Cell(1, i).GetString()).Where(x => x.Length > 0).ToList();
        Assert.Contains("Costo", cabeceras);
        Assert.Contains("Utilidad", cabeceras);
        Assert.Contains("Precio de venta", cabeceras);
        Assert.Equal("Alimento BARF", l.Cell(2, 7).GetString());
        Assert.Equal(2, l.Cell(2, 8).GetValue<decimal>());
    }

    [Fact]
    public void HojaLineas_SinPermiso_NoTrae_ColumnasDeCosto()
    {
        using var libro = Abrir(GeneradorIvaExcel.IvaRepercutido(Reporte(), DateTime.Now, Lineas(costo: false)));
        var l = libro.Worksheet("Líneas");
        var cabeceras = Enumerable.Range(1, 20).Select(i => l.Cell(1, i).GetString()).Where(x => x.Length > 0).ToList();
        Assert.DoesNotContain("Costo", cabeceras);
        Assert.DoesNotContain("Utilidad", cabeceras);
    }
}
