using System.Text.Json;

namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Lee el envelope <c>{ status, currentException, validationErrors, responses }</c>
/// del API en las clases de ProxyClass escritas a mano (endpoints que no están en
/// el contrato NSwag). Mismo criterio que <see cref="EnvelopeApi"/> pero sin
/// depender de los tipos generados.
/// </summary>
public static class LecturaEnvelope
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            return new ResponseGeneric<T>($"El API respondió {(int)respuesta.StatusCode}: {Recortar(cuerpo)}");

        Envelope<T>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json);
        }
        catch (JsonException ex)
        {
            return new ResponseGeneric<T>($"Respuesta ilegible del API: {ex.Message}");
        }

        if (envelope is null)
            return new ResponseGeneric<T>("El API devolvió una respuesta vacía.");

        if (envelope.Status == 0)
            return new ResponseGeneric<T>(envelope.Responses);

        var errores = envelope.ValidationErrors ?? Array.Empty<string>();
        var mensaje = envelope.CurrentException
                      ?? (errores.Count > 0 ? string.Join(" ", errores) : "El API devolvió un estado de error sin detalle.");
        return new ResponseGeneric<T>(mensaje, errores);
    }

    private static string Recortar(string s) => s.Length <= 500 ? s : s[..500] + "…";

    private sealed class Envelope<T>
    {
        public int Status { get; init; }
        public string? CurrentException { get; init; }
        public IReadOnlyList<string>? ValidationErrors { get; init; }
        public T? Responses { get; init; }
    }
}
