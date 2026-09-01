namespace SuvesaPosSitioAplicacion.DTOs.Parametros;

public sealed class BodegaMantenimientoDTO
{
    public int IdBodega { get; set; }
    public string NombreBodega { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public bool? Bloqueada { get; set; }
    public bool? Estado { get; set; } = true;
    public bool? EsCostaPets { get; set; }
}

public sealed class AreaMantenimientoDTO
{
    public decimal IdArea { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public int IdSucursal { get; set; }
}
