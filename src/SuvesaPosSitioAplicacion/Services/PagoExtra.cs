namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Línea ADICIONAL de una forma de pago que se repite: el cliente paga con más de un depósito o cheque.
/// La primera línea de cada forma sigue viviendo en los diccionarios del panel (monto, referencia, número
/// de cheque, banco por código); cada línea extra es una de estas. El cobro final es la primera línea más
/// todas las extras de ese código.
/// </summary>
public sealed class PagoExtra
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    /// <summary>Referencia (depósito, tarjeta). En un cheque no se usa: ver <see cref="NumeroCheque"/>.</summary>
    public string? Referencia { get; set; }
    public string? NumeroCheque { get; set; }
    public int? IdBanco { get; set; }
}
