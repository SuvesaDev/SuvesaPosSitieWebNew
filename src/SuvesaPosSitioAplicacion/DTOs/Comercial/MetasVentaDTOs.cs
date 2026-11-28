namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/metas (DTOs/MetasVentaDTOs.cs del API).

public static class NivelMeta
{
    public const string Ruta = "Ruta";
    public const string Usuario = "Usuario";
}

public static class DimensionMeta
{
    public const string Global = "Global";
    public const string Categoria = "Categoria";
    public const string Familia = "Familia";
    public const string Marca = "Marca";
    public const string Especie = "Especie";
    public const string FuncionTerapeutica = "FuncionTerapeutica";

    public static readonly (string Valor, string Texto)[] Todas =
    {
        (Global, "Total (todo lo vendido)"),
        (Categoria, "Categoría"),
        (Familia, "Familia"),
        (Marca, "Marca o laboratorio"),
        (Especie, "Especie"),
        (FuncionTerapeutica, "Función terapéutica"),
    };

    public static string Texto(string valor) => Todas.FirstOrDefault(d => d.Valor == valor).Texto ?? valor;
}

/// <summary>Meta de ventas de un mes, de una ruta (equipo) o de una persona (Agente o SAC).</summary>
public sealed class MetaVentaDTO
{
    public long Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public int? IdRuta { get; set; }
    public string? NombreRuta { get; set; }
    public string? IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public string TipoDimension { get; set; } = DimensionMeta.Global;
    public int? IdDimension { get; set; }
    public string? NombreDimension { get; set; }
    public decimal? MontoMeta { get; set; }
    public decimal? UnidadesMeta { get; set; }
    public int? ClientesNuevosMeta { get; set; }
    /// <summary>Solo lectura: el mes ya inició.</summary>
    public bool Bloqueada { get; set; }
    /// <summary>Obligatorio al cambiar una meta de un mes ya iniciado.</summary>
    public string? Motivo { get; set; }
}

public sealed class FiltroMetasDTO
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string? Nivel { get; set; }
    public int? IdRuta { get; set; }
    public string? IdUsuario { get; set; }
}

public sealed class MetasMesDTO
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public bool Bloqueado { get; set; }
    public List<MetaVentaDTO> Metas { get; set; } = new();
}

public sealed class MetaVentaCambioDTO
{
    public long Id { get; set; }
    public string Accion { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public string? NombreRuta { get; set; }
    public string? NombreUsuario { get; set; }
    public string TipoDimension { get; set; } = string.Empty;
    public string? NombreDimension { get; set; }
    public decimal? MontoAnterior { get; set; }
    public decimal? UnidadesAnterior { get; set; }
    public int? ClientesNuevosAnterior { get; set; }
    public decimal? MontoNuevo { get; set; }
    public decimal? UnidadesNuevo { get; set; }
    public int? ClientesNuevosNuevo { get; set; }
    public bool EnMesBloqueado { get; set; }
    public string? Motivo { get; set; }
    public string? Usuario { get; set; }
    public DateTime FechaUtc { get; set; }
}
