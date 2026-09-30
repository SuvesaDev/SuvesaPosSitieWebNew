using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano a DTOs generados (igual que FacturaDTO.Pagos.cs). Al
// regenerar los contratos el generado ya trae `numeroCheque`/`idBanco` y este archivo
// se borra.
//
// Todas las formas de pago Cheque/Depósito exigen su número de instrumento — sin él,
// un pago pendiente de confirmación queda sin forma de identificarse/conciliarse.

public partial class CobroDocumentosDTO
{
    /// <summary>Solo forma de pago Cheque: número del cheque del cliente.</summary>
    [JsonPropertyName("numeroCheque")]
    public string? NumeroCheque { get; set; }

    /// <summary>Solo forma de pago Cheque: banco emisor (catálogo EntidadesBancaria.IdBanco).</summary>
    [JsonPropertyName("idBanco")]
    public int? IdBanco { get; set; }
}

public partial class EntregaCuentaDTO
{
    /// <summary>Solo forma de pago Cheque: número del cheque del cliente.</summary>
    [JsonPropertyName("numeroCheque")]
    public string? NumeroCheque { get; set; }

    /// <summary>Solo forma de pago Cheque: banco emisor (catálogo EntidadesBancaria.IdBanco).</summary>
    [JsonPropertyName("idBanco")]
    public int? IdBanco { get; set; }
}
