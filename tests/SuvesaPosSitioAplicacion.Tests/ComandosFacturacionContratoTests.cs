using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// W3/W6: los contratos a mano de los comandos y del estado de cuenta deben coincidir con
/// los esquemas del API (camelCase, Web defaults) para que el round-trip por el proxy no
/// pierda campos.
/// </summary>
public class ComandosFacturacionContratoTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Comando_SerializaClaveIdempotenciaYVenta()
    {
        var venta = new FacturaDTO { IdSerie = 400, Total = 11300f, CodCliente = "3101" };
        var json = JsonSerializer.Serialize(new ComandoFacturacionDTO { ClaveIdempotencia = "abc", Venta = venta }, Web);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("abc", doc.RootElement.GetProperty("claveIdempotencia").GetString());
        Assert.Equal(400, doc.RootElement.GetProperty("venta").GetProperty("idSerie").GetInt32());
    }

    [Fact]
    public void Resultado_DeserializaElCuerpoDelApi()
    {
        const string cuerpo = """
        {
          "idVenta": 501, "numeroOperativo": "12345", "total": 11300.0,
          "estadoComercial": "Confirmada", "estadoPago": "Pagada", "estadoFiscal": "Pendiente",
          "idSerie": 400, "vencimiento": null, "saldoPendiente": 0.0, "fueReintento": true
        }
        """;
        var r = JsonSerializer.Deserialize<ResultadoOperacionFacturacionDTO>(cuerpo, Web)!;

        Assert.Equal(501, r.IdVenta);
        Assert.Equal("Pagada", r.EstadoPago);
        Assert.True(r.FueReintento);
        Assert.Equal(0.0, r.SaldoPendiente);
    }

    [Fact]
    public void PreventaContado_Comando_SerializaPagosYClave()
    {
        var cmd = new FacturarPreventaContadoComandoDTO
        {
            ClaveIdempotencia = "lote:700", IdPreventa = 700, Usuario = "cajero", IdApertura = 5, IdSucursal = 1,
            Pagos = new() { new() { FormaPago = "EFE", Monto = 12000m }, new() { FormaPago = "TAR", Monto = 3000m, Referencia = "x" } },
        };
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(cmd, Web));
        Assert.Equal(700, doc.RootElement.GetProperty("idPreventa").GetInt64());
        Assert.Equal(2, doc.RootElement.GetProperty("pagos").GetArrayLength());
        Assert.Equal("EFE", doc.RootElement.GetProperty("pagos")[0].GetProperty("formaPago").GetString());
    }

    [Fact]
    public void PreventaContado_Resultado_Deserializa()
    {
        const string cuerpo = """
        { "idVenta": 700, "numFactura": 55.0, "total": 10000.0, "totalPagado": 12000.0,
          "vuelto": 2000.0, "estadoFiscal": "NoAplica", "fueReintento": true }
        """;
        var r = JsonSerializer.Deserialize<FacturarPreventaContadoResultadoDTO>(cuerpo, Web)!;
        Assert.Equal(700, r.IdVenta);
        Assert.Equal(2000m, r.Vuelto);
        Assert.Equal("NoAplica", r.EstadoFiscal);
        Assert.True(r.FueReintento);
    }

    [Fact]
    public void EstadoCuenta_DeserializaTramosYDetalle()
    {
        const string cuerpo = """
        {
          "idCliente": 7777, "nombre": "ACME", "fechaCorte": "2026-09-05T00:00:00",
          "codMonedaBase": 1, "limiteAprobado": 1000000.0, "saldoTotal": 15000.0,
          "creditoAFavor": 0.0, "disponible": 985000.0,
          "porVencer": 1000.0, "vencido1a30": 2000.0, "vencido31a60": 3000.0,
          "vencido61a90": 4000.0, "vencido91oMas": 5000.0,
          "detalle": [ { "idVenta": 1, "numFactura": 10.0, "fecha": "2026-01-01T00:00:00",
                         "vence": "2026-01-16T00:00:00", "codMoneda": 1, "montoOriginal": 5000.0,
                         "notasCreditoAplicadas": 0.0, "pagosAplicados": 0.0, "saldoActual": 5000.0 } ]
        }
        """;
        var e = JsonSerializer.Deserialize<EstadoCuentaClienteDTO>(cuerpo, Web)!;

        Assert.Equal(15000.0, e.SaldoTotal);
        Assert.Equal(5000.0, e.Vencido91oMas);
        Assert.Single(e.Detalle);
        Assert.Equal(5000.0, e.Detalle[0].SaldoActual);
    }

    [Fact]
    public void DevolucionInterna_ConservaLineaOrigenYLote()
    {
        var comando = new DevolucionInternaComandoWebDTO
        {
            ClaveIdempotencia = "dev-int:77",
            IdVentaOrigen = 77,
            Usuario = "cajero",
            AnularOrigen = false,
            Lineas = new()
            {
                new() { IdVentaDetalle = 701, Cantidad = 2, IdStockLote = 91 }
            }
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(comando, Web));
        var linea = doc.RootElement.GetProperty("lineas")[0];
        Assert.Equal(701, linea.GetProperty("idVentaDetalle").GetInt64());
        Assert.Equal(91, linea.GetProperty("idStockLote").GetInt64());
        Assert.False(doc.RootElement.GetProperty("anularOrigen").GetBoolean());
    }

    [Fact]
    public void Factura_DeserializaNaturalezaParaEscogerLaDevolucionCorrecta()
    {
        const string cuerpo = """{ "id": 77, "naturalezaFiscalDoc": "Interna" }""";
        var factura = JsonSerializer.Deserialize<FacturaDTO>(cuerpo, Web)!;

        Assert.Equal("Interna", factura.NaturalezaFiscalDoc);
    }
}
