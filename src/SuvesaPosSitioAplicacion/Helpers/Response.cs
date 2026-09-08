namespace SuvesaPosSitioAplicacion.Helpers;

/// <summary>
/// Envoltura estandar de toda respuesta que sale de ApiConexion hacia las Views.
/// Espejo del patron de FCRCASitioAplicacion: las Views nunca ven una excepcion
/// ni un HttpResponseMessage, solo este objeto.
/// </summary>
public class Response
{
    public bool EsCorrecta { get; init; }
    public string? Excepcion { get; init; }
    public IReadOnlyList<string> ErroresValidacion { get; init; } = Array.Empty<string>();

    public Response()
    {
        EsCorrecta = true;
    }

    public Response(Exception excepcion)
    {
        EsCorrecta = false;
        Excepcion = excepcion.Message;
    }

    public Response(string excepcion)
    {
        EsCorrecta = false;
        Excepcion = excepcion;
    }

    public Response(string excepcion, IReadOnlyList<string> erroresValidacion)
    {
        EsCorrecta = false;
        Excepcion = excepcion;
        ErroresValidacion = erroresValidacion;
    }
}
