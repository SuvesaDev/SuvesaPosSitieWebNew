namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/panel-metas y api/comercial/atribucion-sac (DTOs/PanelMetasDTOs.cs del API).

public sealed class AvanceMetaDTO
{
    public long IdMeta { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public int? IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public string? IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public string TipoDimension { get; set; } = string.Empty;
    public string? NombreDimension { get; set; }
    public decimal? MontoMeta { get; set; }
    public decimal? UnidadesMeta { get; set; }
    public int? ClientesNuevosMeta { get; set; }
    public decimal MontoReal { get; set; }
    public decimal UnidadesReal { get; set; }
    public int ClientesNuevosReal { get; set; }
    public decimal? CumplimientoPct { get; set; }
    public decimal? ProyeccionMonto { get; set; }
    public decimal? ProyeccionPct { get; set; }
    public decimal? CumplimientoUnidadesPct { get; set; }
    public decimal? CumplimientoClientesPct { get; set; }
}

public sealed class CarteraVencidaDTO
{
    public string Nivel { get; set; } = string.Empty;
    public int? IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public string? IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public decimal VencidoAlCorte { get; set; }
    public decimal Arrastre { get; set; }
    public decimal PorCobrarMes { get; set; }
    public decimal Pct { get; set; }
    public decimal? TopePct { get; set; }
    public bool SuperaTope { get; set; }
}

public sealed class PanelMetasDTO
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public DateTime? Corte { get; set; }
    public int DiasHabilesTotales { get; set; }
    public int DiasHabilesTranscurridos { get; set; }
    public int DiasHabilesRestantes { get; set; }
    public bool ProyeccionConfiable { get; set; }
    public bool MesFuturo { get; set; }
    public bool EsSupervision { get; set; }
    public List<AvanceMetaDTO> Metas { get; set; } = new();
    public List<CarteraVencidaDTO> Carteras { get; set; } = new();
}

public sealed class ColaboradorAtribucionDTO
{
    public string IdUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsTitular { get; set; }
    public bool EsServicioAlCliente { get; set; }
    public bool SumaAMetaRuta { get; set; } = true;
}

public sealed class GuardarAtribucionRequest
{
    public bool SumaAMetaRuta { get; set; }
}
