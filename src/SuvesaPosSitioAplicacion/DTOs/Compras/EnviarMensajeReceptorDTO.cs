namespace SuvesaPosSitioAplicacion.DTOs.Compras;

/// <summary>Espejo exacto de ApiSuvesaPos.DTOs.EnviarMensajeReceptorDTO — lo que el sitio
/// manda al aceptar/rechazar una compra ante Hacienda desde Compra.razor. El resto del
/// Mensaje Receptor (Clave, cédulas, totales) lo resuelve el API a partir de la Compra.</summary>
public sealed class EnviarMensajeReceptorDTO
{
    /// <summary>1 = Aceptado, 2 = Aceptado parcialmente, 3 = Rechazado.</summary>
    public int Mensaje { get; set; }
    public string? DetalleMensaje { get; set; }
    /// <summary>01 Genera crédito IVA, 02 Genera crédito parcial, 03 Bienes de capital,
    /// 04 Gasto corriente (no genera crédito), 05 Proporcionalidad.</summary>
    public string? CondicionImpuesto { get; set; }
    public decimal? MontoTotalImpuestoAcreditar { get; set; }
    public decimal? MontoTotalDeGastoAplicable { get; set; }
    public string TipoIdentificacionEmisor { get; set; } = string.Empty;
    public DateTimeOffset FechaEmisionDoc { get; set; }
}

/// <summary>Espejo de ApiSuvesaPos.DTOs.ResultadoMensajeReceptorDTO.</summary>
public sealed class ResultadoMensajeReceptorDTO
{
    public bool EsValido { get; set; }
    public bool EsDuplicada { get; set; }
    public long? IdEmision { get; set; }
    public string Estado { get; set; } = string.Empty;
    public IReadOnlyList<string> Errores { get; set; } = Array.Empty<string>();
}
