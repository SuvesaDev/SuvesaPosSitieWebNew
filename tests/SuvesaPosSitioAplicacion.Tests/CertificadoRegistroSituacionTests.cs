using SuvesaPosSitioAplicacion.DTOs.Certificados;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>Lo que ve el usuario en la lista de certificados según sus fechas.</summary>
public class CertificadoRegistroSituacionTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 28);

    private static CertificadoRegistroWebDTO Cert(DateOnly vence, DateOnly? aviso, string estado = "Vigente")
        => new() { FechaVencimiento = vence, FechaAviso = aviso, Estado = estado };

    [Fact]
    public void AntesDelAviso_EstaVigente()
        => Assert.Equal("Vigente", Cert(new DateOnly(2028, 1, 1), new DateOnly(2027, 1, 1)).Situacion(Hoy));

    [Fact]
    public void DesdeLaFechaDeAviso_PorVencer_IncluidoElMismoDia()
    {
        Assert.Equal("Por vencer", Cert(new DateOnly(2027, 1, 1), Hoy).Situacion(Hoy));
        Assert.Equal("Por vencer", Cert(Hoy, new DateOnly(2026, 1, 1)).Situacion(Hoy));   // vence hoy: todavía no está vencido
    }

    [Fact]
    public void PasadoElVencimiento_Vencido()
        => Assert.Equal("Vencido", Cert(new DateOnly(2026, 10, 27), new DateOnly(2025, 10, 27)).Situacion(Hoy));

    [Theory]
    [InlineData("Renovado")]
    [InlineData("Baja")]
    public void ElHistorial_MuestraSuEstado_AunqueLaFechaYaPaso(string estado)
        => Assert.Equal(estado, Cert(new DateOnly(2020, 1, 1), new DateOnly(2019, 1, 1), estado).Situacion(Hoy));
}
