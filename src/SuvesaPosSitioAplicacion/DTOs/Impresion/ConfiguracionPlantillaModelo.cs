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
    public int Version { get; set; } = 2;
    public TemaDocumentoModelo Tema { get; set; } = new();
    public LayoutDocumentoModelo Layout { get; set; } = new();
    public QrDocumentoModelo Qr { get; set; } = new();
    public MontoEnLetrasModelo MontoEnLetras { get; set; } = new();
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
        try
        {
            var config = JsonSerializer.Deserialize<ConfiguracionPlantillaModelo>(json, Json) ?? new();
            return NormalizarV2(config);
        }
        catch (JsonException) { return new(); }
    }

    public string AJson() => JsonSerializer.Serialize(this, Json);

    private static ConfiguracionPlantillaModelo NormalizarV2(ConfiguracionPlantillaModelo config)
    {
        config.Version = Math.Max(2, config.Version);
        config.Tema ??= new();
        config.Layout ??= new();
        config.Qr ??= new();
        config.MontoEnLetras ??= new();
        return config;
    }
}

public sealed class TemaDocumentoModelo
{
    public string Nombre { get; set; } = "corporativo";
    public string ColorPrimario { get; set; } = "#1072A9";
    public string ColorSecundario { get; set; } = "#EEF5F8";
    public string ColorTotal { get; set; } = "#0D5B88";
    public string ColorTexto { get; set; } = "#1F2933";
}

public sealed class LayoutDocumentoModelo
{
    public string Preset { get; set; } = "corporativo-a4";
    public bool EncabezadoDosColumnas { get; set; } = true;
    public bool TotalesDestacados { get; set; } = true;
}

public sealed class QrDocumentoModelo
{
    public bool Mostrar { get; set; }
    public string? Payload { get; set; }
    public string Etiqueta { get; set; } = "Consulta del documento";
}

public sealed class MontoEnLetrasModelo
{
    public bool Mostrar { get; set; }
    public string Etiqueta { get; set; } = "Monto en letras";
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
    public string Familia { get; set; } = "Lato";
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
