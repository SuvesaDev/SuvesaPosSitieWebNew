using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class EstadoCuentaPdfTests
{
    static EstadoCuentaPdfTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void EstadoCuenta_ConSaldosYAntiguedad_GeneraPdfValido()
    {
        var reporte = new EstadoCuentaPdf(
            NombreCliente: "Almacén La Canasta",
            IdentificacionCliente: "3101123456",
            FechaCorte: new DateTime(2026, 9, 5),
            LimiteAprobado: 100000m,
            SaldoAbierto: 24253.19m,
            CreditoAFavor: 0m,
            Disponible: 75746.81m,
            PorVencer: 24253.19m,
            Vencido1a30: 0m,
            Vencido31a60: 0m,
            Vencido61a90: 0m,
            Vencido91oMas: 0m,
            Detalle:
            [
                new LineaEstadoCuentaPdf(
                    Factura: "1",
                    Consecutivo: "00100150010000000001",
                    Fecha: new DateTime(2026, 9, 5),
                    Vence: new DateTime(2026, 10, 5),
                    Original: 24253.19m,
                    NotasCredito: 0m,
                    Pagado: 0m,
                    Saldo: 24253.19m,
                    EstadoMh: "Aceptado")
            ]);

        var pdf = new GeneradorPdfQuestPdf().EstadoCuenta(reporte);

        Assert.True(pdf.Length > 800);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
