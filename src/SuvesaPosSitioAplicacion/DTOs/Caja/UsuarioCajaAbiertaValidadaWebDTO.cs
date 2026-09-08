namespace SuvesaPosSitioAplicacion.DTOs.Caja;

/// <summary>
/// Resultado atómico de la clave interna: identifica al cajero y su apertura
/// sin mezclar el Id de la tabla de usuarios con el Id estable del login.
/// </summary>
public sealed class UsuarioCajaAbiertaValidadaWebDTO
{
    public long IdUsuario { get; init; }
    public string? Login { get; init; }
    public string? Nombre { get; init; }
    public long NumeroApertura { get; init; }
    public long NumeroCaja { get; init; }
}
