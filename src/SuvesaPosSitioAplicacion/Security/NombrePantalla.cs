using System.Globalization;
using System.Text;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Normaliza el nombre de una pantalla para comparar permisos.
///
/// POR QUE HACE FALTA
/// Los permisos casan por titulo, no por ruta, y el API y el menu no escriben igual
/// los mismos titulos. Medido contra el API real:
///
///     el API manda        el menu tiene
///     Facturacion         Facturación
///     Consignacion        Consignación
///
/// Comparando en crudo, esas pantallas desaparecen del menu para cualquier rol que
/// no sea administrador. Aqui se ignoran tildes, mayusculas y espacios de sobra.
///
/// Es un parche del lado del sitio: lo correcto seria que el API y el menu usaran
/// los mismos textos. Mientras no ocurra, esto evita el fallo.
/// </summary>
public static class NombrePantalla
{
    /// <summary>Comparador que trata "Facturacion" y "Facturación" como el mismo nombre.</summary>
    public static readonly IEqualityComparer<string> Comparador = new ComparadorSinTildes();

    /// <summary>Quita tildes, pasa a minusculas y recorta.</summary>
    public static string Normalizar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        var descompuesto = nombre.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);

        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    private sealed class ComparadorSinTildes : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => Normalizar(x) == Normalizar(y);

        public int GetHashCode(string obj) => Normalizar(obj).GetHashCode();
    }
}
