namespace SuvesaPosSitioAplicacion.DTOs.Inventario;

/// <summary>Artículo cuyo CABYS no coincide con su marca de servicio/mercancía (espejo de api/inventario/auditoria-cabys).</summary>
public sealed class ArticuloCabysInconsistenteWeb
{
    public long Codigo { get; set; }
    public string CodArticulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Cabys { get; set; } = string.Empty;
    public bool EsServicioMarcado { get; set; }
    public bool? EsServicioSegunCabys { get; set; }
    public bool? Estado { get; set; }
    public string Problema { get; set; } = string.Empty;
}

public sealed class ResultadoAuditoriaCabysWeb
{
    public int ArticulosRevisados { get; set; }
    public List<ArticuloCabysInconsistenteWeb> Inconsistentes { get; set; } = new();
}
