using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Impresion;

/// <summary>
/// Espejo editable del esquema <c>ConfiguracionJson</c> del API
/// (MOTOR_PLANTILLAS_IMPRESION_API.md §4.4). El editor de la web enlaza sus
/// controles a estas propiedades y luego se serializa tal cual.
/// </summary>
public sealed class ConfiguracionPlantillaModelo
{
    public int Version { get; set; } = 1;
    public MargenesModelo MargenesMm { get; set; } = new();
    public FuenteModelo Fuente { get; set; } = new();
    public EncabezadoModelo Encabezado { get; set; } = new();
    public BloqueCamposModelo Receptor { get; set; } = new();
    public BloqueCamposModelo Meta { get; set; } = new();
    public DetalleModelo Detalle { get; set; } = new();
    public TotalesModelo Totales { get; set; } = new();
    public PieModelo Pie { get; set; } = new();
    public LeyendasModelo Leyendas { get; set; } = new();

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public static ConfiguracionPlantillaModelo Desde(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<ConfiguracionPlantillaModelo>(json, Json) ?? new(); }
        catch (JsonException) { return new(); }
    }

    public string AJson() => JsonSerializer.Serialize(this, Json);
}

public sealed class MargenesModelo
{
    public double Sup { get; set; } = 15;
    public double Inf { get; set; } = 15;
    public double Izq { get; set; } = 15;
    public double Der { get; set; } = 15;
}

public sealed class FuenteModelo
{
    public string Familia { get; set; } = "Helvetica";
    public double TamanoBase { get; set; } = 9;
}

public sealed class EncabezadoModelo
{
    public bool MostrarLogo { get; set; } = true;
    public string AlineacionLogo { get; set; } = "izquierda";
    public double AltoLogoMm { get; set; } = 18;
    public bool MostrarDatosEmisor { get; set; } = true;
    public List<string> LineasTexto { get; set; } = new();
}

public sealed class BloqueCamposModelo
{
    public bool Mostrar { get; set; } = true;
    public List<CampoModelo> Campos { get; set; } = new();
}

public sealed class CampoModelo
{
    public string Clave { get; set; } = "";
    public string Etiqueta { get; set; } = "";
    public bool Visible { get; set; } = true;
    public int Orden { get; set; }
}

public sealed class DetalleModelo
{
    public List<ColumnaModelo> Columnas { get; set; } = new();
}

public sealed class ColumnaModelo
{
    public string Clave { get; set; } = "";
    public string Etiqueta { get; set; } = "";
    public bool Visible { get; set; } = true;
    public double AnchoRel { get; set; } = 1;
    public string Alineacion { get; set; } = "izquierda";
    public int Orden { get; set; }
}

public sealed class TotalesModelo
{
    public List<CampoModelo> Filas { get; set; } = new();
}

public sealed class PieModelo
{
    public List<string> LineasTexto { get; set; } = new();
    public bool MostrarResolucion { get; set; }
    public string TextoResolucion { get; set; } = "";
    public bool MostrarDatosBancarios { get; set; }
    public bool MostrarPaginado { get; set; } = true;
}

public sealed class LeyendasModelo
{
    public string Original { get; set; } = "ORIGINAL";
    public string Copia { get; set; } = "COPIA";
}
