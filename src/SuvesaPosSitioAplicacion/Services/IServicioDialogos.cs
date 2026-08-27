namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Confirmaciones, avisos y errores. Punto unico.
///
/// El sistema actual tiene 1 893 llamadas a SweetAlert repartidas por 172 ficheros,
/// cada una con su propio texto de botones y sus propios colores. Aqui hay una sola
/// forma de preguntar y una sola forma de avisar: si manana cambia el aspecto o la
/// libreria, se cambia en un archivo y no en 172.
///
/// **Las Views no deben llamar a HxMessageBoxService ni a HxMessengerService
/// directamente.** Solo a esto.
/// </summary>
public interface IServicioDialogos
{
    /// <summary>Pregunta de si/no. Devuelve true solo si el usuario confirma.</summary>
    Task<bool> ConfirmarAsync(string mensaje, string? titulo = null, string? textoConfirmar = null);

    /// <summary>Confirmacion de algo que destruye datos. Mas explicita que la normal.</summary>
    Task<bool> ConfirmarPeligroAsync(string mensaje, string? titulo = null);

    /// <summary>Aviso modal que el usuario tiene que cerrar.</summary>
    Task InformarAsync(string mensaje, string? titulo = null);

    /// <summary>Error modal.</summary>
    Task ErrorAsync(string mensaje, string? titulo = null);

    /// <summary>Aviso breve que se va solo. Para confirmaciones de exito.</summary>
    void Exito(string mensaje, string? titulo = null);

    /// <summary>Aviso breve de advertencia.</summary>
    void Advertencia(string mensaje, string? titulo = null);

    /// <summary>Aviso breve de error. Para fallos que no cortan el flujo.</summary>
    void ErrorBreve(string mensaje, string? titulo = null);
}
