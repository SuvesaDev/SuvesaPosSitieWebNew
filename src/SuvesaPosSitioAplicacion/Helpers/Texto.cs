using System.Globalization;
using System.Text;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Utilidades de texto para busquedas: quita tildes, pasa a minusculas y recorta,
/// para que "Jose" encuentre "José" en los filtros de las rejillas.
///
/// (Antes esto vivia en <c>Security.NombrePantalla</c> y se usaba tambien para casar
/// permisos por rotulo. Con el rediseno V2 los permisos casan por codigo, asi que
/// ese uso desaparecio y queda solo el normalizador de busqueda.)
/// </summary>
public static class Texto
{
    public static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var descompuesto = valor.Trim().Normalize(NormalizationForm.FormD);
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
}
