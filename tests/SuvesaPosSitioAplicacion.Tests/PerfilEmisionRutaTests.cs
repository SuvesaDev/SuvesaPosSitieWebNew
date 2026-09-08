using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Services;
using static SuvesaPosSitioAplicacion.Services.PoliticaRutaFacturacion;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// W3: el perfil de emisión elegible trae `esTiquete` / `esInterna` / `naturaleza` del API
/// y la pantalla de Facturación deriva la ruta de esos campos.
/// </summary>
public class PerfilEmisionRutaTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Perfil_DeserializaLosCamposDeModalidad()
    {
        const string cuerpo = """
        { "idSerie": 400, "descripcion": "Tiquete 04", "numeroTerminal": 1, "idTipoFactura": 3,
          "codigoFe": "04", "tipoNombre": "Tiquete", "esCredito": false, "emisionV44Habilitada": true,
          "elegible": true, "motivoNoElegible": null,
          "esTiquete": true, "esInterna": false, "naturaleza": "Electronica" }
        """;
        var p = JsonSerializer.Deserialize<PerfilEmisionElegibleWebDTO>(cuerpo, Web)!;

        Assert.True(p.EsTiquete);
        Assert.False(p.EsInterna);
        Assert.Equal("Electronica", p.Naturaleza);
    }

    [Fact]
    public void DesdePerfil_TiqueteElectronico_ResuelveCobrarElectronico()
    {
        var p = new PerfilEmisionElegibleWebDTO
        {
            IdSerie = 400, EsTiquete = true, EsCredito = false, EsInterna = false,
            EmisionV44Habilitada = true, CodigoFe = "04", Elegible = true,
        };

        var r = Resolver(new EntradaSerie(p.EsTiquete, p.EsCredito, !p.EsInterna, p.EmisionV44Habilitada, p.CodigoFe));

        Assert.Equal(Ruta.CobrarTiqueteElectronico, r.Ruta);
    }

    [Fact]
    public void DesdePerfil_NoTiqueteInternoCredito_ResuelveConfirmarCredito()
    {
        var p = new PerfilEmisionElegibleWebDTO
        {
            IdSerie = 200, EsTiquete = false, EsCredito = true, EsInterna = true,
            EmisionV44Habilitada = false, CodigoFe = null, Elegible = true,
        };

        var r = Resolver(new EntradaSerie(p.EsTiquete, p.EsCredito, !p.EsInterna, p.EmisionV44Habilitada, p.CodigoFe));

        Assert.Equal(Ruta.ConfirmarCredito, r.Ruta);
    }
}
