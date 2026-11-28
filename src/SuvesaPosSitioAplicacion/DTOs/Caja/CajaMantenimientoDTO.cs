namespace SuvesaPosSitioAplicacion.DTOs.Caja;

/// <summary>Fila del mantenimiento de cajas: la caja y, si está abierta, quién la abrió.</summary>
public sealed class CajaMantenimientoDTO
{
    public long IdCaja { get; init; }
    public long NumCaja { get; init; }
    public long NumApertura { get; init; }
    public string? UsuarioApertura { get; init; }
    public DateTime? FechaApertura { get; init; }
}
