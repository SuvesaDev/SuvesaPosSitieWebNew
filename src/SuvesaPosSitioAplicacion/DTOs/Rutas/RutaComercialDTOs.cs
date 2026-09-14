namespace SuvesaPosSitioAplicacion.DTOs.Rutas;

public sealed class RutaComercialDTO
{
    public int IdRuta { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public int? IdSucursal { get; set; }
    public bool Activo { get; set; } = true;
    public List<RutaAgenteDTO> Agentes { get; set; } = new();
}

public sealed class RutaAgenteDTO
{
    public string IdUsuario { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public bool EsPredeterminado { get; set; }
    public bool EsRutaPredeterminada { get; set; }
}
