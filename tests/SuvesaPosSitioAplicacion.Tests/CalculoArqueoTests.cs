using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El arqueo compara lo declarado por el cajero contra lo que el sistema
/// registro; si el total mezcla colones y dolares sin convertir, deja de servir
/// para detectar faltantes de caja.
/// </summary>
public class CalculoArqueoTests
{
    [Fact]
    public void SoloColones_TipoDeCambioNoAfecta()
    {
        var total = CalculoArqueo.Total(colones: 50_000m, dolares: 0m, tipoCambio: 520m);

        Assert.Equal(50_000m, total);
    }

    [Fact]
    public void ConDolares_SeConviertenAntesDeSumar()
    {
        // Caso que reproduce el bug real: antes del arreglo, Total sumaba
        // colones + dolares directo (50 000 + 100 = 50 100), como si $1 valiera
        // ₡1. El total correcto convierte los dolares primero.
        var total = CalculoArqueo.Total(colones: 50_000m, dolares: 100m, tipoCambio: 520m);

        Assert.Equal(102_000m, total);   // 50 000 + (100 * 520)
        Assert.NotEqual(50_100m, total); // lo que daba el calculo con el bug
    }

    [Fact]
    public void SoloDolares_SinColones()
    {
        var total = CalculoArqueo.Total(colones: 0m, dolares: 200m, tipoCambio: 515.50m);

        Assert.Equal(103_100m, total);
    }
}
