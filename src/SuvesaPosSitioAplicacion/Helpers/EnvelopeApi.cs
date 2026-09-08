using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Traduce el envelope que genera NSwag desde el OpenAPI del API
/// (<c>XxxResponseGeneric</c> con status / currentException / validationErrors / responses)
/// al <see cref="ResponseGeneric{T}"/> de la casa.
///
/// Existe para que las clases de ProxyClass no repitan el mismo bloque 321 veces
/// y para que las Views trabajen siempre con el mismo tipo.
/// </summary>
public static class EnvelopeApi
{
    /// <summary>Status 0 es exito. Cualquier otro valor es fallo de negocio.</summary>
    private const ResponseStatus Correcto = ResponseStatus._0;

    public static ResponseGeneric<T> A<T>(
        ResponseStatus status,
        string? currentException,
        ICollection<string>? validationErrors,
        T? responses)
    {
        if (status == Correcto)
        {
            return new ResponseGeneric<T>(responses);
        }

        var errores = validationErrors?.ToList() ?? new List<string>();
        var mensaje = currentException
                      ?? (errores.Count > 0 ? string.Join(" ", errores) : "El API devolvio un estado de error sin detalle.");

        return new ResponseGeneric<T>(mensaje, errores);
    }
}
