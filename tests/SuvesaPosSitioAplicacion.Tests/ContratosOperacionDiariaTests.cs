using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SuvesaPosSitioAplicacion.ApiConexion;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.DTOs.Consignacion;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Produccion;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Contratos que el sitio usa en una jornada real de supermercado/carnicería.
/// No llaman una API externa: el handler captura verbo, ruta y JSON para evitar
/// que una refactorización deje al sitio apuntando a una operación equivocada.
/// </summary>
public class ContratosOperacionDiariaTests
{
    [Fact]
    public async Task Consignacion_DeCarnePorCliente_UsaElCicloDeBoletaConteoYPrefactura()
    {
        var handler = new RegistroHttpHandler(_ => """{"status":0,"responses":{}}""");
        var cliente = new ConsignacionInvApiCliente(Http(handler), new SesionPrueba("operador-carnes"));

        await cliente.RegistrarBoletaAsync(new BoletaConsignacionRequest
        {
            Tipo = 1,
            IdCliente = 401,
            Documento = "ING-CAR-00048",
            Motivo = "Ingreso de cortes refrigerados",
            Lineas = [new() { IdArticulo = 8001, IdStockLote = 55, Cantidad = 18.5 }]
        });
        await cliente.RegistrarConteoAsync(new ConteoConsignacionRequest
        {
            IdCliente = 401,
            Completo = true,
            Lineas = [new() { IdArticulo = 8001, IdStockLote = 55, Fisico = 17.75 }]
        });
        await cliente.GenerarPrefacturaAsync(new GenerarPrefacturaConsignacion
        {
            IdConteo = 18,
            Factura = new() { CodCliente = "401", Detalle = [] }
        });
        await cliente.AprobarPrefacturaAsync(91);
        await cliente.FacturarPrefacturaAsync(new FacturarPrefacturaConsignacion { IdPrefactura = 91, Condicion = 1 });

        Assert.Collection(handler.Solicitudes,
            r => Assert.Equal(("POST", "/ConsignacionInventario/RegistrarBoleta"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/RegistrarConteo"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/GenerarPrefactura"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/AprobarPrefactura?idPrefactura=91"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/FacturarPrefactura"), r.VerboYRuta));

        Assert.Contains("18.5", handler.Solicitudes[0].Cuerpo);
        Assert.Contains("17.75", handler.Solicitudes[1].Cuerpo);
        Assert.Contains("idConteo", handler.Solicitudes[2].Cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConsignacionDeCarne_DesdeLaCentralHastaFacturaCredito_ConservaLoteCantidadYEstados()
    {
        // 20 kg salen de la bodega normal hacia la central; se consignan al
        // cliente, el conteo físico deja 17,75 kg y por tanto se facturan 2,25 kg.
        const long cliente = 401;
        const long articulo = 8001;
        const long lote = 55;
        const long conteo = 300;
        const long prefactura = 940;
        var handler = new RegistroHttpHandler(_ => """{"status":0,"responses":{}}""");
        var sesion = new SesionPrueba("operador-carnes");
        var api = new ConsignacionInvApiCliente(Http(handler), sesion);
        var factura = new FacturaDTO
        {
            CodCliente = cliente.ToString(),
            IdSucursal = 3,
            IdEmpresa = 1,
            NumApertura = 88,
            Preventa = true,
            EsConsignacion = true,
            SubTotal = 11_925f,
            ImpVenta = 1_550.25f,
            Total = 13_475.25f,
            Detalle =
            [
                new()
                {
                    CodArticulo = articulo, Descripcion = "Carne de res para guisar", Cantidad = 2.25f,
                    PrecioUnit = 5_300f, Impuesto = 13, MontoImpuesto = 1_550.25f,
                    SubtotalGravado = 11_925f, SubTotal = 11_925f, IdBodega = 61, Lote = lote, NumeroLote = "RES-0904"
                }
            ]
        };

        await api.AbrirBodegaCentralAsync(new AbrirBodegaCentralConsignacion { IdSucursal = 3 });
        await api.ReponerCentralAsync(new ReponerCentralConsignacionRequest
        {
            IdSucursal = 3, IdBodegaOrigen = 2, Documento = "REP-CAR-00001",
            Lineas = [new() { IdArticulo = articulo, IdStockLote = lote, Cantidad = 20 }]
        });
        await api.AbrirBodegaAsync(new AbrirBodegaConsignacion { IdCliente = cliente, IdSucursal = 3, Observaciones = "Cliente de consignación de carnes" });
        await api.RegistrarBoletaAsync(new BoletaConsignacionRequest
        {
            Tipo = 1, IdCliente = cliente, Documento = "ING-CAR-00049", Motivo = "Entrega semanal de carne refrigerada",
            Lineas = [new() { IdArticulo = articulo, IdStockLote = lote, Cantidad = 20 }]
        });
        await api.ExistenciaAsync(cliente);
        await api.RegistrarConteoAsync(new ConteoConsignacionRequest
        {
            IdCliente = cliente, Completo = true, Agente = "Carnicería Central",
            Lineas = [new() { IdArticulo = articulo, IdStockLote = lote, Fisico = 17.75, PrecioUnitario = 5_300 }]
        });
        await api.KardexAsync(new KardexConsignacionFiltro { IdCliente = cliente, Desde = new DateTime(2026, 9, 4), Hasta = new DateTime(2026, 9, 4) });
        await api.GenerarPrefacturaAsync(new GenerarPrefacturaConsignacion { IdConteo = conteo, Factura = factura });
        await api.AprobarPrefacturaAsync(prefactura);
        await api.FacturarPrefacturaAsync(new FacturarPrefacturaConsignacion { IdPrefactura = prefactura, Condicion = 2, IdPlazo = 15 });
        await api.PrefacturaAsync(prefactura);
        await api.KardexAsync(new KardexConsignacionFiltro { IdCliente = cliente });

        Assert.Collection(handler.Solicitudes,
            r => Assert.Equal(("POST", "/ConsignacionInventario/AbrirBodegaCentral"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/ReponerCentral"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/AbrirBodega"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/RegistrarBoleta"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/Existencia"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/RegistrarConteo"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/Kardex"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/GenerarPrefactura"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/AprobarPrefactura?idPrefactura=940"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/FacturarPrefactura"), r.VerboYRuta),
            r => Assert.Equal(("GET", "/ConsignacionInventario/Prefactura?id=940"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/ConsignacionInventario/Kardex"), r.VerboYRuta));

        using var conteoJson = JsonDocument.Parse(handler.Solicitudes[5].Cuerpo);
        using var facturaJson = JsonDocument.Parse(handler.Solicitudes[7].Cuerpo);
        using var facturacionJson = JsonDocument.Parse(handler.Solicitudes[9].Cuerpo);
        Assert.Equal(17.75d, conteoJson.RootElement.GetProperty("lineas")[0].GetProperty("fisico").GetDouble());
        Assert.Equal(2.25d, facturaJson.RootElement.GetProperty("factura").GetProperty("detalle")[0].GetProperty("cantidad").GetDouble());
        Assert.True(facturaJson.RootElement.GetProperty("factura").GetProperty("esConsignacion").GetBoolean());
        Assert.Equal(2, facturacionJson.RootElement.GetProperty("condicion").GetInt32());
        Assert.Equal(15, facturacionJson.RootElement.GetProperty("idPlazo").GetInt32());
        Assert.Equal(12, sesion.Cargas);
    }

    [Fact]
    public async Task Consignacion_NoRegistraBoletaSiLaReposicionDesdeLaBodegaCentralFalla()
    {
        var handler = new RegistroHttpHandler(request => request.RequestUri!.AbsolutePath == "/ConsignacionInventario/ReponerCentral"
            ? """{"status":1,"currentException":"Carne de res no tiene existencia suficiente en la bodega de origen."}"""
            : """{"status":0,"responses":{}}""");
        var api = new ConsignacionInvApiCliente(Http(handler), new SesionPrueba("operador-carnes"));

        var respuesta = await api.ReponerCentralAsync(new ReponerCentralConsignacionRequest
        {
            IdSucursal = 3, IdBodegaOrigen = 2, Documento = "REP-CAR-SIN-STOCK",
            Lineas = [new() { IdArticulo = 8001, IdStockLote = 55, Cantidad = 20 }]
        });

        Assert.Equal(ResponseStatus._1, respuesta.Status);
        Assert.Contains("existencia suficiente", respuesta.CurrentException, StringComparison.OrdinalIgnoreCase);
        Assert.Single(handler.Solicitudes);
        Assert.Equal(("POST", "/ConsignacionInventario/ReponerCentral"), handler.Solicitudes[0].VerboYRuta);
    }

    [Fact]
    public async Task Produccion_DeHamburguesaRespetaLotesYConvierteSoloLaCantidadSolicitada()
    {
        var handler = new RegistroHttpHandler(_ => """{"status":0,"responses":{"id":77,"lineas":[]}}""");
        var cliente = new ProduccionApiCliente(Http(handler), new SesionPrueba("carnicero"));
        var solicitud = new CalculoProduccionRequest
        {
            IdArticuloTerminado = 9001,
            Bodega = 3,
            CantidadAProducir = 25,
            NumeroLoteProducido = "HAM-20260904",
            VencimientoLoteProducido = new DateOnly(2026, 9, 8),
            Insumos =
            [
                new() { IdArticuloInsumo = 7001, Lotes = [new() { IdStockLote = 11, Cantidad = 15 }] },
                new() { IdArticuloInsumo = 7002, Lotes = [new() { IdStockLote = 12, Cantidad = 10 }] }
            ]
        };

        await cliente.CalcularAsync(solicitud);
        await cliente.ConvertirAsync(solicitud);

        Assert.Collection(handler.Solicitudes,
            r => Assert.Equal(("POST", "/Produccion/Calcular"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/Produccion/Convertir"), r.VerboYRuta));
        Assert.All(handler.Solicitudes, r =>
        {
            Assert.Contains("HAM-20260904", r.Cuerpo, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("cantidadAProducir", r.Cuerpo, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("25", r.Cuerpo);
        });
    }

    [Fact]
    public async Task Series_CobroCreditoYOrdenCompra_ConservanSusComandosDeNegocio()
    {
        var handler = new RegistroHttpHandler(request => request.RequestUri!.AbsolutePath switch
        {
            "/api/series-operativas" when request.Method == HttpMethod.Get => """{"status":0,"responses":[]}""",
            "/api/series-operativas" => """{"status":0,"responses":33}""",
            "/api/cobros-credito" => """{"status":0,"responses":{"idCobro":90,"numeroRecibo":501,"aplicaciones":[]}}""",
            "/api/ordenes-compra" => """{"status":0,"responses":{"orden":71,"consecutivo":71,"lineas":[]}}""",
            "/SeriesFacturacion/Crear" => """{"status":0,"responses":{"idSerie":12,"secuencia":340,"numeroTerminal":1}}""",
            _ => """{"status":0,"responses":{}}"""
        });
        var sesion = new SesionPrueba("cajero-01");
        var fabrica = new FabricaHttp(Http(handler));
        using var loggerFactory = LoggerFactory.Create(_ => { });

        var series = new SeriesOperativas(fabrica, sesion, loggerFactory.CreateLogger<SeriesOperativas>());
        var seriesFiscales = new SeriesFacturacionFiscales(fabrica, sesion, loggerFactory.CreateLogger<SeriesFacturacionFiscales>());
        var cobros = new CobrosCredito(fabrica, sesion, loggerFactory.CreateLogger<CobrosCredito>());
        var ordenes = new OrdenesCompraFlujo(fabrica, sesion, loggerFactory.CreateLogger<OrdenesCompraFlujo>());

        await series.Listar(tipo: 2, idEmisor: 1, idSucursal: 3);
        await series.Guardar(new SerieOperativaWebDTO
        {
            Tipo = 2, IdEmisor = 1, IdSucursal = 3, Prefijo = "RC", Activa = true, EsPredeterminada = true
        });
        await seriesFiscales.Crear(new SerieFacturacionFiscalDTO
        {
            IdEmisor = 1,
            IdSucursal = 3,
            NumeroTerminal = 1,
            IdTipoFactura = 1,
            Secuencia = 340,
            Descripcion = "Factura electrónica contado Central",
            EmisionV44Habilitada = true
        });
        await cobros.Cobrar(new CobroCreditoComandoWebDTO
        {
            ClaveIdempotencia = "cobro-diario-0001",
            IdCliente = 401,
            IdApertura = 88,
            IdSucursal = 3,
            Facturas = [new() { IdVenta = 501, Monto = 42_500m }],
            FormasPago = [new() { CodigoFormaPago = "01", MontoRecibido = 42_500m }]
        });
        await ordenes.Crear(new CrearOrdenCompraWebDTO
        {
            IdProveedor = 22,
            IdSucursal = 3,
            IdEmisor = 1,
            Credito = true,
            Plazo = 15,
            Lineas = [new() { CodArticulo = 7001, Descripcion = "Carne molida", Cantidad = 40, CostoUnitario = 3_800, PorcImpuesto = 13 }]
        });

        Assert.Collection(handler.Solicitudes,
            r => Assert.Equal(("GET", "/api/series-operativas?tipo=2&idEmisor=1&idSucursal=3"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/api/series-operativas"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/SeriesFacturacion/Crear"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/api/cobros-credito"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/api/ordenes-compra"), r.VerboYRuta));

        using var serieFiscalJson = JsonDocument.Parse(handler.Solicitudes[2].Cuerpo);
        Assert.Equal("Factura electrónica contado Central", serieFiscalJson.RootElement.GetProperty("descripcion").GetString());
        Assert.Contains("cobro-diario-0001", handler.Solicitudes[3].Cuerpo);
        Assert.Contains("42500", handler.Solicitudes[3].Cuerpo);
        Assert.Contains("Carne molida", handler.Solicitudes[4].Cuerpo);
        Assert.Equal(5, sesion.Cargas);
    }

    [Fact]
    public async Task PreventaDeAbarrotes_SeConsultaSeEmiteYSeImprimeConElTipoCorrecto()
    {
        var handler = new RegistroHttpHandler(request => request.RequestUri!.AbsolutePath switch
        {
            "/venta/PreventasPendientesPorCliente" => """{"status":0,"responses":[{"id":501,"cliente":"María Rodríguez","total":30278.35,"codigoFe":"04","esCredito":false}]}""",
            "/api/comprobantes-electronicos/v44/pos/ventas/501/tiquetes/emitir" => """{"esValido":true,"estado":"aceptado","clave":"50604092600310112345600100001010000012345123456789"}""",
            "/api/impresion/tiquete-electronico/501/pdf" => "%PDF-prueba",
            _ => """{"status":0,"responses":{}}"""
        });
        var sesion = new SesionPrueba("cajero-01");
        var fabrica = new FabricaHttp(Http(handler));
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var preventas = new AbonoCobrarPreventas(fabrica, sesion, loggerFactory.CreateLogger<AbonoCobrarPreventas>());
        var impresion = new ImpresionDocumentos(fabrica, sesion, loggerFactory.CreateLogger<ImpresionDocumentos>());

        var pendientes = await preventas.PreventasPendientes(401);
        var emision = await preventas.EmitirTiquete(501);
        var pdf = await impresion.Pdf("tiquete-electronico", 501, "termico80", copia: false);

        Assert.True(pendientes.EsCorrecta);
        Assert.Equal("tiquete-electronico", pendientes.Responses!.Single().SlugImpresion);
        Assert.True(emision.EsCorrecta);
        Assert.True(emision.Responses!.EsValido);
        Assert.True(pdf.EsCorrecta);
        Assert.StartsWith("%PDF", Encoding.UTF8.GetString(pdf.Responses!));
        Assert.Collection(handler.Solicitudes,
            r => Assert.Equal(("POST", "/venta/PreventasPendientesPorCliente?codCliente=401"), r.VerboYRuta),
            r => Assert.Equal(("POST", "/api/comprobantes-electronicos/v44/pos/ventas/501/tiquetes/emitir"), r.VerboYRuta),
            r => Assert.Equal(("GET", "/api/impresion/tiquete-electronico/501/pdf?copia=false&formato=termico80"), r.VerboYRuta));
        Assert.Equal(3, sesion.Cargas);
    }

    private static HttpClient Http(HttpMessageHandler handler) => new(handler) { BaseAddress = new Uri("https://seepos.test/") };

    private sealed class RegistroHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string> _respuesta;
        public List<SolicitudCapturada> Solicitudes { get; } = [];

        public RegistroHttpHandler(Func<HttpRequestMessage, string> respuesta) => _respuesta = respuesta;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cuerpo = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Solicitudes.Add(new SolicitudCapturada(request.Method.Method, request.RequestUri!.PathAndQuery, cuerpo));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_respuesta(request), Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record SolicitudCapturada(string Verbo, string Ruta, string Cuerpo)
    {
        public (string Verbo, string Ruta) VerboYRuta => (Verbo, Ruta);
    }

    private sealed class FabricaHttp(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => cliente;
    }

    private sealed class SesionPrueba(string? usuario) : IContextoSesion
    {
        public int Cargas { get; private set; }
        public bool Autenticado => true;
        public string? Token => "token-prueba";
        public string? Usuario => usuario;
        public bool EsSuperAdministrador => false;
        public bool EsAdministrador => false;
        public string? PerfilCodigo => "USUARIO";
        public bool EsCostaPets => false;
        public bool EsAgenteCostaPets => false;
        public bool PermitirExistenciaNegativa => false;
        public int IdSucursal => 3;
        public string? NombreSucursal => "Central";
        public bool TieneSucursal => true;
        public IReadOnlyCollection<string> Menus => Array.Empty<string>();
        public IReadOnlyCollection<PermisoFuncion> Permisos => Array.Empty<PermisoFuncion>();
        public bool PuedeVer(string funcionCodigo) => true;
        public bool EstaGobernada(string funcionCodigo) => true;
        public bool Puede(string funcionCodigo, AccionPantalla accion) => true;
        public Task CargarAsync() { Cargas++; return Task.CompletedTask; }
    }
}
