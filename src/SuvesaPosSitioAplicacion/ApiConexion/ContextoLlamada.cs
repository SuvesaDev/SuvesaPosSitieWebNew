namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Token de la llamada en curso, para que lo vea <c>ApiAuthHeaderHandler</c>.
///
/// POR QUE ESTO EXISTE
/// Los handlers de IHttpClientFactory **no** se resuelven desde el ambito de la
/// peticion: la fabrica crea el suyo y lo reutiliza unos minutos. Inyectar
/// IContextoSesion en el handler da otra instancia, vacia, y la cabecera
/// Authorization nunca sale. El sintoma es un 401 con la sesion aparentemente bien.
///
/// AsyncLocal si atraviesa ese limite, porque va con el flujo asincrono de quien
/// llama. Lo pone <see cref="ProxyBase"/> antes de cada llamada, en un solo sitio.
/// </summary>
public static class ContextoLlamada
{
    private static readonly AsyncLocal<string?> _token = new();

    public static string? Token
    {
        get => _token.Value;
        set => _token.Value = value;
    }
}
