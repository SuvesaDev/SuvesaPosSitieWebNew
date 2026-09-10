namespace SuvesaPosSitioAplicacion.DTOs.Caja;

public sealed class InstrumentoConfirmacionWebDTO
{
    public long Id { get; set; }
    public string Tipo { get; set; } = "Deposito";
    public string Estado { get; set; } = "PendienteConfirmacion";
    public decimal Monto { get; set; }
    public string Referencia { get; set; } = "";
    public long? NumApertura { get; set; }
    public string Responsable { get; set; } = "";
    public DateTime FechaCaptura { get; set; }
    public DateTime FechaAlertaUtc { get; set; }
    public DateTime FechaLimiteUtc { get; set; }
    public string? Motivo { get; set; }
}

public sealed class FiltroInstrumentosConfirmacionWebDTO { public string? Estado { get; set; } public string? Tipo { get; set; } public long? NumApertura { get; set; } }
public sealed class CambioEstadoInstrumentoWebDTO { public string Estado { get; set; } = ""; public string? Motivo { get; set; } public string? Evidencia { get; set; } }
public sealed class ResumenInstrumentosCajaWebDTO { public decimal Confirmados { get; set; } public decimal Pendientes { get; set; } public decimal NoConfirmados { get; set; } public decimal ChequesPendientes { get; set; } public bool TienePendientes { get; set; } }
