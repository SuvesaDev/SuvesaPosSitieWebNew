namespace SuvesaPosSitioAplicacion.Models;

public sealed class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool MostrarRequestId => !string.IsNullOrEmpty(RequestId);
}
