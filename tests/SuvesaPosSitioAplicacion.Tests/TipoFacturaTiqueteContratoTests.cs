using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// W1 (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md): el DTO manual de tipos de documento lleva
/// <c>EsTiquete</c> y lo serializa como <c>esTiquete</c>, igual que el
/// <c>TipoFacturaDTO.esTiquete</c> del API, para que el round-trip por el proxy no lo
/// pierda. El proxy usa <see cref="JsonSerializerDefaults.Web"/>.
/// </summary>
public class TipoFacturaTiqueteContratoTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Fact]
    public void EsTiquete_SeSerializaComo_esTiquete()
    {
        var json = JsonSerializer.Serialize(
            new TipoFacturaFiscalDTO { Codigo = 40, Descripcion = "Tiquete", Uso = UsoTipoDocumento.Facturacion, EsTiquete = true },
            Web);

        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("esTiquete", out var v));
        Assert.True(v.GetBoolean());
    }

    [Fact]
    public void Deserializa_esTiquete_DelCuerpoDelApi()
    {
        // Forma en que el API expone el tipo (ResponseGeneric.responses[i]).
        const string cuerpo = """
            { "id": 3, "codigo": 40, "descripcion": "Tiquete", "uso": 1, "activo": true, "esTiquete": true }
            """;

        var dto = JsonSerializer.Deserialize<TipoFacturaFiscalDTO>(cuerpo, Web);

        Assert.NotNull(dto);
        Assert.True(dto!.EsTiquete);
        Assert.Equal(UsoTipoDocumento.Facturacion, dto.Uso);
    }

    [Fact]
    public void ClienteViejo_SinElCampo_NoRompe_YQuedaEnFalse()
    {
        const string cuerpo = """{ "id": 1, "codigo": 1, "descripcion": "Factura", "uso": 1, "activo": true }""";

        var dto = JsonSerializer.Deserialize<TipoFacturaFiscalDTO>(cuerpo, Web);

        Assert.NotNull(dto);
        Assert.False(dto!.EsTiquete);
    }
}
