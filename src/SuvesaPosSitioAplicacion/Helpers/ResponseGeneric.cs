namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>Respuesta con datos. El API devuelve el suyo en <c>responses</c>; aqui se conserva el nombre.</summary>
public class ResponseGeneric<T> : Response
{
    public T? Responses { get; init; }

    public ResponseGeneric(T? responses) : base()
    {
        Responses = responses;
    }

    public ResponseGeneric(Exception excepcion) : base(excepcion) { }

    public ResponseGeneric(string excepcion) : base(excepcion) { }

    public ResponseGeneric(string excepcion, IReadOnlyList<string> erroresValidacion)
        : base(excepcion, erroresValidacion) { }
}
