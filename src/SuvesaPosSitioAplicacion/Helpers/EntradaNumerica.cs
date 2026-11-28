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

    /// <summary>
    /// Escribe un importe o porcentaje (2 decimales) para un campo numerico con
    /// <c>step="0.01"</c>. Los DTO guardan <c>float</c>, y <c>12.35f</c> convertido a
    /// <c>double</c> es <c>12.350000381469727</c>: el navegador lo rechaza por no ser
    /// multiplo de 0.01. Se redondea en <c>decimal</c> antes de escribir.
    /// </summary>
    public static string Importe(double? valor) =>
        valor is { } v && double.IsFinite(v)
            ? Math.Round((decimal)v, 2).ToString(CultureInfo.InvariantCulture)
            : string.Empty;

    /// <summary>Redondea a 2 decimales (en decimal, sin la basura del float).</summary>
    public static float Redondear(double valor) =>
        double.IsFinite(valor) ? (float)Math.Round((decimal)valor, 2) : 0f;
}
