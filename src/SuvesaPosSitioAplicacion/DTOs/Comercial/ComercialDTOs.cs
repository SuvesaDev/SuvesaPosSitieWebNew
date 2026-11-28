namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/* (DTOs/ComercialDTOs.cs del API). Se sustituyen por los generados al
// correr tools/actualizar-contratos.sh contra un API desplegado que ya los exponga.

/// <summary>Umbral o tope configurable del módulo comercial.</summary>
public sealed class ParametroComercialDTO
{
    public string Clave { get; set; } = string.Empty;
    /// <summary>Nulo = sin configurar (solo los parámetros que lo permiten).</summary>
    public decimal? Valor { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>"dias", "meses" o "porcentaje".</summary>
    public string Unidad { get; set; } = string.Empty;
    public bool PermiteVacio { get; set; }
}

public sealed class ActualizarParametroComercialRequest
{
    public decimal? Valor { get; set; }
}

/// <summary>Tope de cartera vencida (%) propio de una ruta.</summary>
public sealed class TopeCarteraRutaDTO
{
    public int IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>Día entre semana que no cuenta como hábil (feriado o cierre).</summary>
public sealed class DiaNoHabilDTO
{
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    /// <summary>"Feriado" o "Manual".</summary>
    public string Origen { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public sealed class ResumenMesHabilDTO
{
    public int Mes { get; set; }
    public int DiasHabiles { get; set; }
}

public sealed class CalendarioHabilAnioDTO
{
    public int Anio { get; set; }
    public List<DiaNoHabilDTO> Dias { get; set; } = new();
    public List<ResumenMesHabilDTO> Meses { get; set; } = new();
}
