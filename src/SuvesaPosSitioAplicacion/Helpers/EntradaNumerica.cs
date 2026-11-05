using System.Globalization;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Lectura y escritura de cantidades en los <c>&lt;input type="number"&gt;</c>.
///
/// El navegador entrega y espera SIEMPRE el valor con punto decimal ("0.5"), sin
/// importar el idioma de la pantalla. La aplicacion corre con cultura es-CR (coma
/// decimal), asi que <c>double.TryParse(valor)</c> sin cultura rechazaba "0.5" y la
/// pantalla lo trataba como un valor invalido. Punto unico: toda lectura o escritura
/// de un campo numerico del navegador pasa por aqui.
/// </summary>
public static class EntradaNumerica
{
    /// <summary>Lee lo que entrega un campo numerico del navegador (punto decimal).</summary>
    public static bool TryLeer(string? texto, out double valor) =>
        double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out valor) && double.IsFinite(valor);

    /// <summary>Escribe un valor para un campo numerico del navegador (punto decimal).</summary>
    public static string Texto(double? valor) => valor?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}
