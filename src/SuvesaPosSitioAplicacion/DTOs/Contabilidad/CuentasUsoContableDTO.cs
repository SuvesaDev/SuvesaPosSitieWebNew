using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.DTOs.Contabilidad;

public sealed class CuentasUsoContableDTO
{
    [JsonPropertyName("idEmisor")]
    public int IdEmisor { get; set; }

    [JsonPropertyName("idCuentaRetencion")]
    public long? IdCuentaRetencion { get; set; }

    [JsonPropertyName("idCuentaBancoDolares")]
    public long? IdCuentaBancoDolares { get; set; }

    [JsonPropertyName("idCuentaTransito")]
    public long? IdCuentaTransito { get; set; }

    [JsonPropertyName("idCuentaComisionesPorPagar")]
    public long? IdCuentaComisionesPorPagar { get; set; }
}

public sealed class CuentasUsoContableDTOResponseGeneric
{
    [JsonPropertyName("status")]
    public ResponseStatus Status { get; set; }

    [JsonPropertyName("currentException")]
    public string? CurrentException { get; set; }

    [JsonPropertyName("validationErrors")]
    public ICollection<string>? ValidationErrors { get; set; }

    [JsonPropertyName("responses")]
    public CuentasUsoContableDTO? Responses { get; set; }
}
