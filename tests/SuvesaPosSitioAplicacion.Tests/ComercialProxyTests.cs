using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Comercial;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Security;
using SuvesaPosSitioAplicacion.Services;
using SuvesaPosSitioAplicacion.Views.Reportes;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Contrato de los proxies del módulo comercial (parámetros y calendario hábil) contra el API
/// (api/comercial/*): rutas, verbos, cuerpos y el JSON que serializa ApiSuvesaPos.
/// </summary>
public sealed class ComercialProxyTests
{
    private sealed class Captura : HttpMessageHandler
    {
        private readonly string _json;
        public Captura(string json) => _json = json;
        public HttpMethod? Metodo { get; private set; }
        public string? Ruta { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Metodo = request.Method;
            Ruta = request.RequestUri?.PathAndQuery;
            Cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_json, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class Fabrica(HttpClient cliente) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal("SeePosApi", name);
            return cliente;
        }
    }

    private static (T Proxy, Captura Handler) Nuevo<T>(string json, Func<IHttpClientFactory, T> crear)
    {
        var handler = new Captura(json);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://seepos.test/") };
        return (crear(new Fabrica(http)), handler);
    }

    private static (ParametrosComerciales Proxy, Captura Handler) Parametros(string json)
        => Nuevo(json, f => new ParametrosComerciales(f, new SesionFalsa(), NullLogger<ParametrosComerciales>.Instance));

    private static (CalendarioHabil Proxy, Captura Handler) Calendario(string json)
        => Nuevo(json, f => new CalendarioHabil(f, new SesionFalsa(), NullLogger<CalendarioHabil>.Instance));

    private static (CatalogosComerciales Proxy, Captura Handler) Catalogos(string json)
        => Nuevo(json, f => new CatalogosComerciales(f, new SesionFalsa(), NullLogger<CatalogosComerciales>.Instance));

    [Theory]
    [InlineData(TipoCatalogoComercial.Canales, "canales")]
    [InlineData(TipoCatalogoComercial.Especies, "especies")]
    [InlineData(TipoCatalogoComercial.FuncionesTerapeuticas, "funciones-terapeuticas")]
    public async Task ListarCatalogo_UsaElSegmentoDelCatalogoYElFiltroDeInactivos(TipoCatalogoComercial tipo, string segmento)
    {
        var (proxy, handler) = Catalogos("""{"status":0,"responses":[{"id":1,"nombre":"Clínica","activo":true,"orden":1}]}""");

        var r = await proxy.Listar(tipo, incluirInactivos: true);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal($"/api/comercial/catalogos/{segmento}?incluirInactivos=true", handler.Ruta);
        Assert.Equal("Clínica", Assert.Single(r.Responses!).Nombre);
    }

    [Fact]
    public async Task CrearYActualizarValorDeCatalogo_UsanPostYPut()
    {
        var (crea, hCrea) = Catalogos("""{"status":0,"responses":{"id":5,"nombre":"Antiparasitario","activo":true,"orden":1}}""");
        var c = await crea.Crear(TipoCatalogoComercial.FuncionesTerapeuticas, new CatalogoComercialDTO { Nombre = "Antiparasitario", Activo = true, Orden = 1 });
        Assert.True(c.EsCorrecta, c.Excepcion);
        Assert.Equal(HttpMethod.Post, hCrea.Metodo);
        Assert.Equal("/api/comercial/catalogos/funciones-terapeuticas", hCrea.Ruta);
        Assert.Contains("\"nombre\":\"Antiparasitario\"", hCrea.Cuerpo);

        var (actualiza, hActualiza) = Catalogos("""{"status":0,"responses":{"id":2,"nombre":"Agro","activo":false,"orden":2}}""");
        var a = await actualiza.Actualizar(TipoCatalogoComercial.Canales, 2, new CatalogoComercialDTO { Id = 2, Nombre = "Agro", Activo = false, Orden = 2 });
        Assert.True(a.EsCorrecta, a.Excepcion);
        Assert.Equal(HttpMethod.Put, hActualiza.Metodo);
        Assert.Equal("/api/comercial/catalogos/canales/2", hActualiza.Ruta);
        Assert.Contains("\"activo\":false", hActualiza.Cuerpo);
    }

    [Fact]
    public async Task UnNombreRepetidoLlegaComoErrorDelCatalogo()
    {
        var (proxy, _) = Catalogos("""{"status":1,"currentException":"Ya existe «Finca» en este catálogo."}""");

        var r = await proxy.Crear(TipoCatalogoComercial.Canales, new CatalogoComercialDTO { Nombre = "Finca" });

        Assert.False(r.EsCorrecta);
        Assert.Contains("Ya existe", r.Excepcion);
    }

    private static (ClasificacionComercial Proxy, Captura Handler) Clasificacion(string json)
        => Nuevo(json, f => new ClasificacionComercial(f, new SesionFalsa(), NullLogger<ClasificacionComercial>.Instance));

    [Fact]
    public async Task ListarClientes_EnviaSoloLosFiltrosPresentes()
    {
        var (proxy, handler) = Clasificacion("""
            {"status":0,"responses":{"total":1,"pagina":2,"tamano":25,"items":[
              {"idCliente":7,"cedula":"3-101-7","nombre":"Mundo Mascotas","idCanal":null,"pendiente":true,
               "sucursales":[{"idClienteSucursal":11,"sucursal":"Heredia","nombreComercial":"MM Heredia","idCanalPropio":null,"idCanalEfectivo":null}]}]}}
            """);

        var r = await proxy.ListarClientes(new FiltroClasificacionClientesDTO { Texto = "mundo mascotas", SoloPendientes = true, IdCanal = 3, Pagina = 2, Tamano = 25 });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/clasificacion/clientes?Texto=mundo%20mascotas&SoloPendientes=true&IdCanal=3&Pagina=2&Tamano=25", handler.Ruta);
        var c = Assert.Single(r.Responses!.Items);
        Assert.True(c.Pendiente);
        Assert.Equal(11, Assert.Single(c.Sucursales).IdClienteSucursal);
    }

    [Fact]
    public async Task ListarClientes_SinFiltrosSoloPideLaPagina()
    {
        var (proxy, handler) = Clasificacion("""{"status":0,"responses":{"total":0,"pagina":1,"tamano":50,"items":[]}}""");

        await proxy.ListarClientes(new FiltroClasificacionClientesDTO());

        Assert.Equal("/api/comercial/clasificacion/clientes?Pagina=1&Tamano=50", handler.Ruta);
    }

    [Fact]
    public async Task AsignarCanal_EnviaPutConLosItemsYElCanalNulo()
    {
        var (proxy, handler) = Clasificacion("""{"status":0,"responses":{"actualizados":2}}""");

        var r = await proxy.AsignarCanal(new AsignarCanalRequest
        {
            IdCanal = null,
            Items = { new AsignarCanalItemDTO { IdCliente = 7 }, new AsignarCanalItemDTO { IdCliente = 7, IdClienteSucursal = 11 } },
        });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(2, r.Responses!.Actualizados);
        Assert.Equal(HttpMethod.Put, handler.Metodo);
        Assert.Equal("/api/comercial/clasificacion/clientes/canal", handler.Ruta);
        Assert.Contains("\"idCanal\":null", handler.Cuerpo);
        Assert.Contains("\"idClienteSucursal\":11", handler.Cuerpo);
    }

    [Fact]
    public async Task ListarArticulos_EnviaLosFiltrosYLeeLaClasificacion()
    {
        var (proxy, handler) = Clasificacion("""
            {"status":0,"responses":{"total":1,"pagina":1,"tamano":50,"items":[
              {"idInventario":100,"codArticulo":"ART-100","descripcion":"Antiparasitario","codMarca":1,"marca":"Lab Uno",
               "especies":[{"idEspecie":3,"esPrincipal":true},{"idEspecie":2,"esPrincipal":false}],"funciones":[1,2],"margenMinimoPct":25.5}]}}
            """);

        var r = await proxy.ListarArticulos(new FiltroClasificacionArticulosDTO { CodMarca = 1, IdFamilia = 4, SinEspecie = true, SinMargen = true });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal("/api/comercial/clasificacion/articulos?CodMarca=1&IdFamilia=4&SinEspecie=true&SinMargen=true&Pagina=1&Tamano=50", handler.Ruta);
        var a = Assert.Single(r.Responses!.Items);
        Assert.Equal(3, Assert.Single(a.Especies, e => e.EsPrincipal).IdEspecie);
        Assert.Equal(new[] { 1, 2 }, a.Funciones.ToArray());
        Assert.Equal(25.5m, a.MargenMinimoPct);
    }

    [Fact]
    public async Task GuardarArticulo_EnviaPutConEspeciesFuncionesYMargen()
    {
        var (proxy, handler) = Clasificacion("""{"status":0,"responses":{"idInventario":100,"codArticulo":"ART-100","descripcion":"x","especies":[],"funciones":[],"margenMinimoPct":null}}""");

        var r = await proxy.GuardarArticulo(100, new GuardarClasificacionArticuloRequest
        {
            Especies = { new EspecieArticuloDTO { IdEspecie = 2, EsPrincipal = true } }, Funciones = { 1 }, MargenMinimoPct = 20,
        });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Put, handler.Metodo);
        Assert.Equal("/api/comercial/clasificacion/articulos/100", handler.Ruta);
        Assert.Contains("\"esPrincipal\":true", handler.Cuerpo);
        Assert.Contains("\"margenMinimoPct\":20", handler.Cuerpo);
    }

    [Fact]
    public async Task AsignarMasiva_EnviaPostYElRechazoDelApiLlegaComoError()
    {
        var (ok, hOk) = Clasificacion("""{"status":0,"responses":{"actualizados":12}}""");
        var r = await ok.AsignarMasiva(new AsignarClasificacionMasivaRequest { Filtro = new FiltroClasificacionArticulosDTO { CodMarca = 1 }, AgregarEspecies = { 2 } });
        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(12, r.Responses!.Actualizados);
        Assert.Equal(HttpMethod.Post, hOk.Metodo);
        Assert.Equal("/api/comercial/clasificacion/articulos/asignar", hOk.Ruta);
        Assert.Contains("\"agregarEspecies\":[2]", hOk.Cuerpo);

        var (mal, _) = Clasificacion("""{"status":1,"currentException":"El filtro alcanza más de 5000 artículos; acótelo antes de asignar."}""");
        var e = await mal.AsignarMasiva(new AsignarClasificacionMasivaRequest { AgregarEspecies = { 2 } });
        Assert.False(e.EsCorrecta);
        Assert.Contains("5000", e.Excepcion);
    }

    [Fact]
    public async Task OpcionesFiltroArticulos_LeeMarcasFamiliasYCategorias()
    {
        var (proxy, handler) = Clasificacion("""{"status":0,"responses":{"marcas":[{"id":1,"nombre":"Lab Uno"}],"familias":[{"id":4,"nombre":"Antiparasitarios"}],"categorias":[]}}""");

        var r = await proxy.OpcionesFiltroArticulos();

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal("/api/comercial/clasificacion/articulos/opciones-filtro", handler.Ruta);
        Assert.Equal("Lab Uno", Assert.Single(r.Responses!.Marcas).Nombre);
        Assert.Equal("Antiparasitarios", Assert.Single(r.Responses.Familias).Nombre);
    }

    private static (CoberturasRuta Proxy, Captura Handler) Coberturas(string json)
        => Nuevo(json, f => new CoberturasRuta(f, new SesionFalsa(), NullLogger<CoberturasRuta>.Instance));

    [Fact]
    public async Task ListarCoberturas_EnviaSiempreLosDosInterruptoresYLaRutaSiHay()
    {
        var (proxy, handler) = Coberturas("""
            {"status":0,"responses":[{"idCobertura":5,"idRuta":10,"nombreRuta":"Ruta Norte","idUsuarioAusente":"ana","nombreAusente":"Ana",
             "idUsuarioCubre":"carlos","nombreCubre":"Carlos","desde":"2026-12-01T00:00:00","hasta":"2026-12-07T00:00:00","motivo":"Vacaciones","activo":true}]}
            """);

        var r = await proxy.Listar(new FiltroCoberturasDTO { IdRuta = 10, SoloVigentes = false, IncluirAnuladas = true });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/coberturas?SoloVigentes=false&IncluirAnuladas=true&IdRuta=10", handler.Ruta);
        var c = Assert.Single(r.Responses!);
        Assert.Equal("Carlos", c.NombreCubre);
        Assert.Equal(new DateTime(2026, 12, 7), c.Hasta);
    }

    [Fact]
    public async Task ListarCoberturas_SinRutaNoLaEnvia()
    {
        var (proxy, handler) = Coberturas("""{"status":0,"responses":[]}""");

        await proxy.Listar(new FiltroCoberturasDTO());

        Assert.Equal("/api/comercial/coberturas?SoloVigentes=true&IncluirAnuladas=false", handler.Ruta);
    }

    [Fact]
    public async Task CrearActualizarYAnularCobertura_UsanPostPutYDelete()
    {
        var cuerpo = """{"status":0,"responses":{"idCobertura":5,"idRuta":10,"idUsuarioAusente":"ana","idUsuarioCubre":"carlos","desde":"2026-12-01T00:00:00","hasta":"2026-12-07T00:00:00","activo":true}}""";
        var dto = new CoberturaRutaDTO { IdRuta = 10, IdUsuarioAusente = "ana", IdUsuarioCubre = "carlos", Desde = new DateTime(2026, 12, 1), Hasta = new DateTime(2026, 12, 7), Motivo = "Vacaciones" };

        var (crea, hCrea) = Coberturas(cuerpo);
        Assert.True((await crea.Crear(dto)).EsCorrecta);
        Assert.Equal(HttpMethod.Post, hCrea.Metodo);
        Assert.Equal("/api/comercial/coberturas", hCrea.Ruta);
        Assert.Contains("\"idUsuarioCubre\":\"carlos\"", hCrea.Cuerpo);
        Assert.Contains("2026-12-07", hCrea.Cuerpo);

        var (actualiza, hActualiza) = Coberturas(cuerpo);
        Assert.True((await actualiza.Actualizar(5, dto)).EsCorrecta);
        Assert.Equal(HttpMethod.Put, hActualiza.Metodo);
        Assert.Equal("/api/comercial/coberturas/5", hActualiza.Ruta);

        var (anula, hAnula) = Coberturas("""{"status":0,"responses":true}""");
        Assert.True((await anula.Anular(5)).EsCorrecta);
        Assert.Equal(HttpMethod.Delete, hAnula.Metodo);
        Assert.Equal("/api/comercial/coberturas/5", hAnula.Ruta);
    }

    [Fact]
    public async Task UnaCoberturaQueSeCruzaLlegaComoErrorYLosUsuariosElegiblesSeLeen()
    {
        var (mal, _) = Coberturas("""{"status":1,"currentException":"Ya hay una cobertura de esa persona en esa ruta que se cruza con las fechas indicadas."}""");
        var e = await mal.Crear(new CoberturaRutaDTO { IdRuta = 10 });
        Assert.False(e.EsCorrecta);
        Assert.Contains("se cruza", e.Excepcion);

        var (ok, handler) = Coberturas("""{"status":0,"responses":[{"idUsuario":"bea","nombre":"Bea","esAgente":false,"esServicioAlCliente":true}]}""");
        var u = await ok.UsuariosElegibles();
        Assert.Equal("/api/comercial/coberturas/usuarios-elegibles", handler.Ruta);
        Assert.True(Assert.Single(u.Responses!).EsServicioAlCliente);
    }

    private static (MetasVenta Proxy, Captura Handler) Metas(string json)
        => Nuevo(json, f => new MetasVenta(f, new SesionFalsa(), NullLogger<MetasVenta>.Instance));

    [Fact]
    public async Task ListarMetasDelMes_EnviaMesYFiltrosPresentesYLeeElBloqueo()
    {
        var (proxy, handler) = Metas("""
            {"status":0,"responses":{"anio":2026,"mes":11,"bloqueado":true,"metas":[
              {"id":1,"anio":2026,"mes":11,"nivel":"Ruta","idRuta":10,"nombreRuta":"Ruta Norte","tipoDimension":"Global","montoMeta":5000000,"unidadesMeta":null,"clientesNuevosMeta":4,"bloqueada":true},
              {"id":2,"anio":2026,"mes":11,"nivel":"Usuario","idUsuario":"ana","nombreUsuario":"Ana","tipoDimension":"Especie","idDimension":2,"nombreDimension":"Caninos","montoMeta":900000,"bloqueada":true}]}}
            """);

        var r = await proxy.ListarMes(new FiltroMetasDTO { Anio = 2026, Mes = 11, Nivel = "Usuario", IdUsuario = "ana" });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/metas?Anio=2026&Mes=11&Nivel=Usuario&IdUsuario=ana", handler.Ruta);
        Assert.True(r.Responses!.Bloqueado);
        Assert.Equal(2, r.Responses.Metas.Count);
        Assert.Equal(4, r.Responses.Metas[0].ClientesNuevosMeta);
        Assert.Equal("Caninos", r.Responses.Metas[1].NombreDimension);
    }

    private static (PanelMetas Proxy, Captura Handler) Panel(string json)
        => Nuevo(json, f => new PanelMetas(f, new SesionFalsa(), NullLogger<PanelMetas>.Instance));

    [Fact]
    public async Task ConsultarPanelDeMetas_EnviaMesYLeeAvanceYCartera()
    {
        var (proxy, handler) = Panel("""
            {"status":0,"responses":{"anio":2026,"mes":11,"corte":"2026-11-12T00:00:00","diasHabilesTotales":21,"diasHabilesTranscurridos":9,"diasHabilesRestantes":12,"proyeccionConfiable":true,"mesFuturo":false,"esSupervision":true,
              "metas":[{"idMeta":1,"nivel":"Ruta","idRuta":10,"nombreRuta":"Ruta Norte","tipoDimension":"Global","montoMeta":18000,"montoReal":5800,"cumplimientoPct":32.2,"proyeccionMonto":13533.33,"proyeccionPct":75.2}],
              "carteras":[{"nivel":"Ruta","idRuta":10,"nombreRuta":"Ruta Norte","vencidoAlCorte":1500,"arrastre":1000,"porCobrarMes":1500,"pct":60,"topePct":50,"superaTope":true}]}}
            """);

        var r = await proxy.Consultar(2026, 11);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/panel-metas?anio=2026&mes=11", handler.Ruta);
        Assert.Equal(9, r.Responses!.DiasHabilesTranscurridos);
        Assert.Equal(75.2m, Assert.Single(r.Responses.Metas).ProyeccionPct);
        Assert.True(Assert.Single(r.Responses.Carteras).SuperaTope);
    }

    [Fact]
    public async Task AtribucionDelSac_ListaConGetYGuardaConPut()
    {
        var (lista, hLista) = Panel("""{"status":0,"responses":[{"idUsuario":"carlos","nombre":"Carlos","esTitular":false,"esServicioAlCliente":true,"sumaAMetaRuta":true}]}""");
        var l = await lista.ListarAtribucion(10);
        Assert.True(l.EsCorrecta, l.Excepcion);
        Assert.Equal("/api/comercial/atribucion-sac/10", hLista.Ruta);
        Assert.True(Assert.Single(l.Responses!).EsServicioAlCliente);

        var (guarda, hGuarda) = Panel("""{"status":0,"responses":{"idUsuario":"carlos","nombre":"Carlos","esTitular":false,"esServicioAlCliente":true,"sumaAMetaRuta":false}}""");
        var g = await guarda.GuardarAtribucion(10, "carlos", false);
        Assert.True(g.EsCorrecta, g.Excepcion);
        Assert.Equal(HttpMethod.Put, hGuarda.Metodo);
        Assert.Equal("/api/comercial/atribucion-sac/10/carlos", hGuarda.Ruta);
        Assert.Contains("\"sumaAMetaRuta\":false", hGuarda.Cuerpo);
    }

    private static (Faltantes Proxy, Captura Handler) Falt(string json)
        => Nuevo(json, f => new Faltantes(f, new SesionFalsa(), NullLogger<Faltantes>.Instance));

    [Fact]
    public async Task CrearFaltante_EnviaPostConElProductoYLaCantidad()
    {
        var (proxy, handler) = Falt("""{"status":0,"responses":{"id":5,"idUsuarioSolicitante":"u1","producto":"Collar XL","esCatalogado":false,"cantidad":3,"estado":"Pendiente","origen":"Modal"}}""");

        var r = await proxy.Crear(new CrearSolicitudFaltanteRequest { Descripcion = "Collar XL", Cantidad = 3m, IdCliente = 7, Origen = "Facturacion" });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/comercial/faltantes", handler.Ruta);
        Assert.Contains("\"descripcion\":\"Collar XL\"", handler.Cuerpo);
        Assert.Contains("\"origen\":\"Facturacion\"", handler.Cuerpo);
        Assert.Equal(EstadoSolicitudFaltante.Pendiente, r.Responses!.Estado);
    }

    [Fact]
    public async Task ConsultarFaltantes_MisBandejaEHistorialUsanSusRutas()
    {
        var (mis, hMis) = Falt("""{"status":0,"responses":[{"id":1,"producto":"X","estado":"EnOrdenCompra","cantidad":2,"numeroOrdenCompra":"OC-9"}]}""");
        Assert.Equal("OC-9", Assert.Single((await mis.Mis(true)).Responses!).NumeroOrdenCompra);
        Assert.Equal("/api/comercial/faltantes/mis?soloAbiertas=true", hMis.Ruta);

        var (bandeja, hBandeja) = Falt("""{"status":0,"responses":[{"producto":"X","unidadesPorAbastecer":5,"clientesEsperando":2,"idsSolicitud":[1,2]}]}""");
        Assert.Equal(2, Assert.Single((await bandeja.Bandeja()).Responses!).ClientesEsperando);
        Assert.Equal("/api/comercial/faltantes/bandeja", hBandeja.Ruta);

        var (historial, hHistorial) = Falt("""{"status":0,"responses":{"solicitudes":3,"montoVentaPerdida":6000,"tasaRecuperacionPct":50,"porProducto":[],"porMarca":[],"noCatalogados":[]}}""");
        var h = await historial.Historial(new FiltroHistorialFaltantesDTO { Desde = new DateTime(2026, 11, 1), Hasta = new DateTime(2026, 11, 30), IdRuta = 10 });
        Assert.Equal(6000m, h.Responses!.MontoVentaPerdida);
        Assert.Equal("/api/comercial/faltantes/historial?Desde=2026-11-01&Hasta=2026-11-30&IdRuta=10", hHistorial.Ruta);
    }

    [Fact]
    public async Task GestionarFaltantes_ConcretarCancelarYLigarOrdenUsanPut()
    {
        var cuerpo = """{"status":0,"responses":{"id":5,"producto":"X","estado":"VentaConcretada","cantidad":1}}""";
        var (concreta, hConcreta) = Falt(cuerpo);
        Assert.True((await concreta.Concretar(5, 99)).EsCorrecta);
        Assert.Equal((HttpMethod.Put, "/api/comercial/faltantes/5/concretar"), (hConcreta.Metodo, hConcreta.Ruta));
        Assert.Contains("\"idVenta\":99", hConcreta.Cuerpo);

        var (cancela, hCancela) = Falt(cuerpo);
        Assert.True((await cancela.Cancelar(5, "ya no lo quiere")).EsCorrecta);
        Assert.Equal("/api/comercial/faltantes/5/cancelar", hCancela.Ruta);
        Assert.Contains("ya no lo quiere", hCancela.Cuerpo);

        var (liga, hLiga) = Falt("""{"status":0,"responses":2}""");
        var n = await liga.VincularOrdenCompra(new VincularOrdenCompraRequest { IdsSolicitud = { 1, 2 }, NumeroOrdenCompra = "OC-1", FechaEstimadaArribo = new DateTime(2026, 12, 5) });
        Assert.Equal(2, n.Responses);
        Assert.Equal((HttpMethod.Put, "/api/comercial/faltantes/orden-compra"), (hLiga.Metodo, hLiga.Ruta));
        Assert.Contains("\"numeroOrdenCompra\":\"OC-1\"", hLiga.Cuerpo);
    }

    [Fact]
    public async Task CrearYActualizarMeta_UsanPostYPutConElMotivo()
    {
        var cuerpo = """{"status":0,"responses":{"id":7,"anio":2026,"mes":12,"nivel":"Ruta","idRuta":10,"tipoDimension":"Global","montoMeta":1000,"bloqueada":false}}""";
        var meta = new MetaVentaDTO { Anio = 2026, Mes = 12, Nivel = NivelMeta.Ruta, IdRuta = 10, TipoDimension = DimensionMeta.Global, MontoMeta = 1000m, Motivo = "Ajuste" };

        var (crea, hCrea) = Metas(cuerpo);
        Assert.True((await crea.Crear(meta)).EsCorrecta);
        Assert.Equal(HttpMethod.Post, hCrea.Metodo);
        Assert.Equal("/api/comercial/metas", hCrea.Ruta);
        Assert.Contains("\"tipoDimension\":\"Global\"", hCrea.Cuerpo);
        Assert.Contains("\"motivo\":\"Ajuste\"", hCrea.Cuerpo);

        var (actualiza, hActualiza) = Metas(cuerpo);
        Assert.True((await actualiza.Actualizar(7, meta)).EsCorrecta);
        Assert.Equal(HttpMethod.Put, hActualiza.Metodo);
        Assert.Equal("/api/comercial/metas/7", hActualiza.Ruta);
    }

    [Fact]
    public async Task EliminarMeta_EnviaElMotivoEnLaCadenaDeConsultaSoloSiHay()
    {
        var (sin, hSin) = Metas("""{"status":0,"responses":true}""");
        Assert.True((await sin.Eliminar(7)).EsCorrecta);
        Assert.Equal(HttpMethod.Delete, hSin.Metodo);
        Assert.Equal("/api/comercial/metas/7", hSin.Ruta);

        var (con, hCon) = Metas("""{"status":0,"responses":true}""");
        await con.Eliminar(7, "  Meta duplicada  ");
        Assert.Equal("/api/comercial/metas/7?motivo=Meta%20duplicada", hCon.Ruta);
    }

    [Fact]
    public async Task UnMesBloqueadoLlegaComoErrorYLasOpcionesEIHistorialSeLeen()
    {
        var (mal, _) = Metas("""{"status":1,"currentException":"El mes 11/2026 ya inició: sus metas no se pueden cambiar."}""");
        var e = await mal.Actualizar(7, new MetaVentaDTO());
        Assert.False(e.EsCorrecta);
        Assert.Contains("ya inició", e.Excepcion);

        var (opciones, hOpc) = Metas("""{"status":0,"responses":[{"id":2,"nombre":"Caninos"}]}""");
        var o = await opciones.OpcionesDimension("Especie");
        Assert.Equal("/api/comercial/metas/dimensiones/Especie", hOpc.Ruta);
        Assert.Equal("Caninos", Assert.Single(o.Responses!).Nombre);

        var (historial, hHis) = Metas("""
            {"status":0,"responses":[{"id":9,"accion":"Modificar","anio":2026,"mes":11,"nivel":"Ruta","nombreRuta":"Ruta Norte","tipoDimension":"Global",
              "montoAnterior":1000000,"montoNuevo":2000000,"enMesBloqueado":true,"motivo":"Ajuste","usuario":"gerente","fechaUtc":"2026-11-27T15:00:00Z"}]}
            """);
        var h = await historial.Historial(2026, 11);
        Assert.Equal("/api/comercial/metas/historial?anio=2026&mes=11", hHis.Ruta);
        var c = Assert.Single(h.Responses!);
        Assert.True(c.EnMesBloqueado);
        Assert.Equal(2_000_000m, c.MontoNuevo);
    }

    // ---------------- Fase 1: filtros y columnas comerciales en los reportes de ventas ----------------

    private static readonly FiltroReporteVisible FiltrosComerciales =
        FiltroReporteVisible.Canal | FiltroReporteVisible.RutaVenta | FiltroReporteVisible.Ejecutor
        | FiltroReporteVisible.RolOrigen | FiltroReporteVisible.Marca | FiltroReporteVisible.Especie;

    [Fact]
    public void SoloLosReportesDeVentasYClientesOfrecenLosFiltrosComerciales_YComoAvanzados()
    {
        var conFiltros = CatalogoReportes.Todos.Where(r => (r.Filtros & FiltrosComerciales) != 0).Select(r => r.Tipo).OrderBy(t => t).ToArray();

        Assert.Equal(new[] { "clientes", "ventas", "ventas-detalle", "ventas-horas" }, conFiltros);
        // Clientes solo ofrece la cartera: ruta y canal. Quien facturó y el rol son de ventas.
        Assert.Equal(FiltroReporteVisible.Canal | FiltroReporteVisible.RutaVenta, CatalogoReportes.Buscar("clientes")!.Filtros & FiltrosComerciales);
        foreach (var r in CatalogoReportes.Todos)
            Assert.Equal(FiltroReporteVisible.Ninguno, r.FiltrosBasicos & FiltrosComerciales);   // ninguno estorba en la vista básica
        // La marca y la especie se miden por línea: solo Ventas detalladas.
        var marcaEspecie = FiltroReporteVisible.Marca | FiltroReporteVisible.Especie;
        Assert.Equal(new[] { "ventas-detalle" }, CatalogoReportes.Todos.Where(r => (r.Filtros & marcaEspecie) != 0).Select(r => r.Tipo).ToArray());
    }

    [Fact]
    public void UnReporteSinFiltrosComercialesLimpiaLosQueQuedaronDeOtro()
    {
        var estado = new EstadoReportePestana { Tipo = "caja", IdCanal = 2, IdRutaVenta = 10, IdEjecutor = "bea", RolOrigen = "SAC", CodMarca = 1, IdEspecie = 3 };

        estado.LimpiarFiltrosNoAplicables(CatalogoReportes.Buscar("caja")!);

        Assert.Equal((null, null, string.Empty, string.Empty, null, null), (estado.IdCanal, estado.IdRutaVenta, estado.IdEjecutor, estado.RolOrigen, estado.CodMarca, estado.IdEspecie));

        var ventas = new EstadoReportePestana { Tipo = "ventas-detalle", IdCanal = 2, IdRutaVenta = 10, IdEjecutor = "bea", RolOrigen = "SAC", CodMarca = 1, IdEspecie = 3 };
        ventas.LimpiarFiltrosNoAplicables(CatalogoReportes.Buscar("ventas-detalle")!);
        Assert.Equal((2, 10, "bea", "SAC", 1, 3), (ventas.IdCanal, ventas.IdRutaVenta, ventas.IdEjecutor, ventas.RolOrigen, ventas.CodMarca, ventas.IdEspecie));
    }

    private static (ReportesOperacion Proxy, Captura Handler) Reportes(string json)
        => Nuevo(json, f => new ReportesOperacion(f, new SesionFalsa(), NullLogger<ReportesOperacion>.Instance));

    [Fact]
    public async Task LaConsultaDeVentasEnviaLosFiltrosComercialesPresentes()
    {
        var (proxy, handler) = Reportes("""{"status":0,"responses":{"titulo":"Ventas","fuente":"x","indicadores":[],"filas":[],"totalRegistros":0,"pagina":1,"tamanoPagina":25,"limitado":false}}""");

        var r = await proxy.Consultar("ventas-detalle", new FiltroReporteOperacionWebDTO
        {
            IdCanal = 2, IdRuta = 10, IdEjecutor = "bea", RolOrigen = "SAC", CodMarca = 1, IdEspecie = 3, TamanoPagina = 25,
        });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal("/api/reportes-operacion/ventas-detalle?idCanal=2&idRuta=10&idEjecutor=bea&rolOrigen=SAC&codMarca=1&idEspecie=3&pagina=1&tamanoPagina=25", handler.Ruta);
    }

    [Fact]
    public async Task SinFiltrosComercialesLaConsultaEsLaDeSiempre()
    {
        var (proxy, handler) = Reportes("""{"status":0,"responses":{"titulo":"Ventas","fuente":"x","indicadores":[],"filas":[],"totalRegistros":0,"pagina":1,"tamanoPagina":25,"limitado":false}}""");

        await proxy.Consultar("ventas", new FiltroReporteOperacionWebDTO { IdAgente = "ana", TamanoPagina = 25 });

        Assert.Equal("/api/reportes-operacion/ventas?idAgente=ana&pagina=1&tamanoPagina=25", handler.Ruta);
    }

    [Fact]
    public async Task LasFilasTraenLasColumnasComerciales()
    {
        var (proxy, _) = Reportes("""
            {"status":0,"responses":{"titulo":"Ventas","fuente":"x","indicadores":[{"etiqueta":"Pronto pago concedido","valor":30}],"totalRegistros":1,"pagina":1,"tamanoPagina":25,"limitado":false,
             "filas":[{"referencia":"2","entidad":"Mundo Mascotas","monto":2000,"origen":"SAC","ejecutor":"Bea","ruta":"Ruta Norte","sucursalCliente":"Cartago","prontoPago":30}]}}
            """);

        var r = await proxy.Consultar("ventas", new FiltroReporteOperacionWebDTO());

        var fila = Assert.Single(r.Responses!.Filas);
        Assert.Equal(("SAC", "Bea", "Ruta Norte", "Cartago", 30m), (fila.Origen, fila.Ejecutor, fila.Ruta, fila.SucursalCliente, fila.ProntoPago));
        Assert.Equal("Pronto pago concedido", Assert.Single(r.Responses.Indicadores).Etiqueta);
    }

    [Fact]
    public async Task LasFilasDeClientesTraenComportamientoCanalQuienAtiendeYAvisos()
    {
        var (proxy, handler) = Reportes("""
            {"status":0,"responses":{"titulo":"Clientes y comportamiento","fuente":"x","indicadores":[{"etiqueta":"Clientes nuevos","valor":2,"formato":"cantidad"}],"totalRegistros":1,"pagina":1,"tamanoPagina":25,"limitado":false,
             "filas":[{"referencia":"4","entidad":"Cliente Con Baja","ruta":"Ruta Norte","canal":"Clínica","atiende":"Ana · Carlos cubre a Ana hasta 04/12","comportamiento":"En recuperación","alerta":"Compra bajó 70%"}]}}
            """);

        var r = await proxy.Consultar("clientes", new FiltroReporteOperacionWebDTO { IdRuta = 10, IdCanal = 1, TamanoPagina = 25 });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal("/api/reportes-operacion/clientes?idCanal=1&idRuta=10&pagina=1&tamanoPagina=25", handler.Ruta);
        var f = Assert.Single(r.Responses!.Filas);
        Assert.Equal(("Ruta Norte", "Clínica", "Ana · Carlos cubre a Ana hasta 04/12", "En recuperación", "Compra bajó 70%"), (f.Ruta, f.Canal, f.Atiende, f.Comportamiento, f.Alerta));
        Assert.Equal("Clientes nuevos", Assert.Single(r.Responses.Indicadores).Etiqueta);
    }

    [Fact]
    public async Task LasOpcionesComercialesParaReportesSeLeenSinPermisoDeAdministracion()
    {
        var (proxy, handler) = Catalogos("""{"status":0,"responses":{"canales":[{"id":1,"nombre":"Clínica"}],"marcas":[{"id":2,"nombre":"Lab Uno"}],"especies":[{"id":3,"nombre":"Felinos"}]}}""");

        var r = await proxy.OpcionesParaReportes();

        Assert.Equal("/api/comercial/catalogos/opciones-reportes", handler.Ruta);
        Assert.Equal("Clínica", Assert.Single(r.Responses!.Canales).Nombre);
        Assert.Equal("Lab Uno", Assert.Single(r.Responses.Marcas).Nombre);
        Assert.Equal("Felinos", Assert.Single(r.Responses.Especies).Nombre);
    }

    // ---------------- Fase 3: centro de alertas ----------------

    private static (AlertasComerciales Proxy, Captura Handler) Alertas(string json)
        => Nuevo(json, f => new AlertasComerciales(f, new SesionFalsa(), NullLogger<AlertasComerciales>.Instance));

    [Fact]
    public async Task LaBandejaEnviaElFiltroYLeeLasAlertasConSuUnicoEstado()
    {
        var (proxy, handler) = Alertas("""
            {"status":0,"responses":{"total":2,"pagina":1,"tamano":25,"items":[
              {"id":9,"tipo":"ClienteInactivo","severidad":"Critica","titulo":"Cliente inactivo","detalle":"Cliente 2: 70 días sin comprar.","idCliente":2,"idRuta":10,"nombreRuta":"Ruta Norte",
               "estado":"Gestionada","fechaCreacionUtc":"2026-11-27T12:00:00Z","gestionadaPor":"Ana","fechaGestionUtc":"2026-11-27T15:00:00Z","notaGestion":"Llamé","destinatarios":[]}]}}
            """);

        var r = await proxy.Bandeja(new FiltroAlertasComercialesDTO { Estado = "Gestionada", Tipo = TipoAlertaComercial.ClienteInactivo, Pagina = 2, Tamano = 10 });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/alertas?Estado=Gestionada&Tipo=ClienteInactivo&Pagina=2&Tamano=10", handler.Ruta);
        var a = Assert.Single(r.Responses!.Items);
        Assert.Equal(("Gestionada", "Ana", "Llamé", "Ruta Norte"), (a.Estado, a.GestionadaPor, a.NotaGestion, a.NombreRuta));
    }

    [Fact]
    public async Task ElResumenYLaGestionUsanSusRutas()
    {
        var (resumen, hResumen) = Alertas("""{"status":0,"responses":{"pendientes":4,"pendientesCriticas":1}}""");
        var r = await resumen.Resumen();
        Assert.Equal("/api/comercial/alertas/resumen", hResumen.Ruta);
        Assert.Equal((4, 1), (r.Responses!.Pendientes, r.Responses.PendientesCriticas));

        var (gestiona, hGestiona) = Alertas("""{"status":0,"responses":{"id":9,"tipo":"ClienteInactivo","severidad":"Critica","titulo":"x","detalle":"y","estado":"Gestionada"}}""");
        Assert.True((await gestiona.Gestionar(9, "Llame al cliente")).EsCorrecta);
        Assert.Equal(HttpMethod.Put, hGestiona.Metodo);
        Assert.Equal("/api/comercial/alertas/9/gestionar", hGestiona.Ruta);
        Assert.Contains("\"nota\":\"Llame al cliente\"", hGestiona.Cuerpo);
    }

    [Fact]
    public async Task LaSupervisionTraeLosDestinatariosYPermiteGestionarYGenerar()
    {
        var (sup, hSup) = Alertas("""{"status":0,"responses":{"total":1,"pagina":1,"tamano":25,"items":[{"id":3,"tipo":"FaltaInventario","severidad":"Preventiva","titulo":"Falta de inventario","detalle":"ART-200","estado":"Pendiente","destinatarios":["Sofia","Carlos"]}]}}""");
        var lista = await sup.Supervision(new FiltroAlertasComercialesDTO { Estado = "Todas" });
        Assert.Equal("/api/comercial/alertas/supervision?Estado=Todas&Pagina=1&Tamano=25", hSup.Ruta);
        Assert.Equal(new[] { "Sofia", "Carlos" }, Assert.Single(lista.Responses!.Items).Destinatarios.ToArray());

        var (gestiona, hGestiona) = Alertas("""{"status":0,"responses":{"id":3,"tipo":"x","severidad":"x","titulo":"x","detalle":"x","estado":"Gestionada"}}""");
        await gestiona.GestionarComoSupervisor(3, null);
        Assert.Equal("/api/comercial/alertas/supervision/3/gestionar", hGestiona.Ruta);

        var (genera, hGenera) = Alertas("""{"status":0,"responses":{"creadas":3,"actualizadas":0,"cerradas":1}}""");
        var g = await genera.GenerarAhora();
        Assert.Equal(HttpMethod.Post, hGenera.Metodo);
        Assert.Equal("/api/comercial/alertas/supervision/generar", hGenera.Ruta);
        Assert.Equal((3, 0, 1), (g.Responses!.Creadas, g.Responses.Actualizadas, g.Responses.Cerradas));
    }

    [Fact]
    public async Task UnaAlertaQueYaNoAplicaLlegaComoErrorSinExcepcion()
    {
        var (proxy, _) = Alertas("""{"status":1,"currentException":"La alerta ya no aplica: se cerró sola porque la situación se resolvió."}""");

        var r = await proxy.Gestionar(9, null);

        Assert.False(r.EsCorrecta);
        Assert.Contains("ya no aplica", r.Excepcion);
    }

    [Fact]
    public async Task ListarParametros_LeeLaListaDelEnvelope()
    {
        var (proxy, handler) = Parametros("""
            {"status":0,"responses":[
              {"clave":"DIAS_PRODUCTO_POR_VENCER","valor":180,"descripcion":"Días","unidad":"dias","permiteVacio":false},
              {"clave":"TOPE_CARTERA_VENCIDA_PCT","valor":null,"descripcion":"Tope","unidad":"porcentaje","permiteVacio":true}]}
            """);

        var r = await proxy.Listar();

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/parametros", handler.Ruta);
        Assert.Equal(2, r.Responses!.Count);
        Assert.Equal(180m, r.Responses[0].Valor);
        Assert.Null(r.Responses[1].Valor);
        Assert.True(r.Responses[1].PermiteVacio);
    }

    [Fact]
    public async Task ActualizarParametro_EnviaPutConLaClaveEnLaRutaYElValorEnElCuerpo()
    {
        var (proxy, handler) = Parametros("""{"status":0,"responses":{"clave":"PCT_DISMINUCION_COMPRA","valor":25,"descripcion":"x","unidad":"porcentaje","permiteVacio":false}}""");

        var r = await proxy.Actualizar("PCT_DISMINUCION_COMPRA", 25m);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Put, handler.Metodo);
        Assert.Equal("/api/comercial/parametros/PCT_DISMINUCION_COMPRA", handler.Ruta);
        Assert.Contains("\"valor\":25", handler.Cuerpo);
    }

    [Fact]
    public async Task ActualizarParametro_ConValorVacioEnviaNull()
    {
        var (proxy, handler) = Parametros("""{"status":0,"responses":{"clave":"TOPE_CARTERA_VENCIDA_PCT","valor":null,"descripcion":"x","unidad":"porcentaje","permiteVacio":true}}""");

        var r = await proxy.Actualizar("TOPE_CARTERA_VENCIDA_PCT", null);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Contains("\"valor\":null", handler.Cuerpo);
    }

    [Fact]
    public async Task ElRechazoDelApiLlegaComoErrorSinExcepcion()
    {
        var (proxy, _) = Parametros("""{"status":1,"currentException":"Indique un número entero de días entre 1 y 3650."}""");

        var r = await proxy.Actualizar("DIAS_CLIENTE_INACTIVO", 0m);

        Assert.False(r.EsCorrecta);
        Assert.Contains("días", r.Excepcion);
    }

    [Fact]
    public async Task TopesPorRuta_ListaGuardaYQuita()
    {
        var (lista, hLista) = Parametros("""{"status":0,"responses":[{"idRuta":3,"nombreRuta":"Ruta Norte","porcentaje":3.5}]}""");
        var l = await lista.ListarTopesRuta();
        Assert.True(l.EsCorrecta, l.Excepcion);
        Assert.Equal("/api/comercial/parametros/topes-ruta", hLista.Ruta);
        Assert.Equal(3.5m, Assert.Single(l.Responses!).Porcentaje);

        var (guarda, hGuarda) = Parametros("""{"status":0,"responses":{"idRuta":3,"nombreRuta":"Ruta Norte","porcentaje":4}}""");
        var g = await guarda.GuardarTopeRuta(3, 4m);
        Assert.True(g.EsCorrecta, g.Excepcion);
        Assert.Equal(HttpMethod.Put, hGuarda.Metodo);
        Assert.Equal("/api/comercial/parametros/topes-ruta/3", hGuarda.Ruta);
        Assert.Contains("\"porcentaje\":4", hGuarda.Cuerpo);

        var (quita, hQuita) = Parametros("""{"status":0,"responses":true}""");
        var q = await quita.QuitarTopeRuta(3);
        Assert.True(q.EsCorrecta, q.Excepcion);
        Assert.Equal(HttpMethod.Delete, hQuita.Metodo);
        Assert.Equal("/api/comercial/parametros/topes-ruta/3", hQuita.Ruta);
    }

    [Fact]
    public async Task ObtenerAnio_LeeDiasYResumenDeMeses()
    {
        var (proxy, handler) = Calendario("""
            {"status":0,"responses":{"anio":2026,
              "dias":[{"fecha":"2026-05-01T00:00:00","descripcion":"Día del Trabajo","origen":"Feriado","activo":true}],
              "meses":[{"mes":5,"diasHabiles":20}]}}
            """);

        var r = await proxy.ObtenerAnio(2026);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Get, handler.Metodo);
        Assert.Equal("/api/comercial/calendario/2026", handler.Ruta);
        Assert.Equal(new DateTime(2026, 5, 1), Assert.Single(r.Responses!.Dias).Fecha);
        Assert.Equal(20, Assert.Single(r.Responses.Meses).DiasHabiles);
    }

    [Fact]
    public async Task GuardarDia_EnviaPutConElDiaEnElCuerpo()
    {
        var (proxy, handler) = Calendario("""{"status":0,"responses":{"fecha":"2026-11-30T00:00:00","descripcion":"Inventario general","origen":"Manual","activo":true}}""");

        var r = await proxy.GuardarDia(new DiaNoHabilDTO { Fecha = new DateTime(2026, 11, 30), Descripcion = "Inventario general", Activo = true });

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Put, handler.Metodo);
        Assert.Equal("/api/comercial/calendario", handler.Ruta);
        Assert.Contains("\"descripcion\":\"Inventario general\"", handler.Cuerpo);
        Assert.Contains("2026-11-30", handler.Cuerpo);
    }

    [Fact]
    public async Task QuitarDia_UsaLaFechaEnFormatoIsoSinHora()
    {
        var (proxy, handler) = Calendario("""{"status":0,"responses":true}""");

        var r = await proxy.QuitarDia(new DateTime(2026, 5, 1, 13, 45, 0));

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Delete, handler.Metodo);
        Assert.Equal("/api/comercial/calendario/2026-05-01", handler.Ruta);
    }

    [Fact]
    public async Task GenerarFeriados_EnviaPostYDevuelveLosNuevos()
    {
        var (proxy, handler) = Calendario("""
            {"status":0,"responses":[{"fecha":"2026-04-02T00:00:00","descripcion":"Jueves Santo","origen":"Feriado","activo":true}]}
            """);

        var r = await proxy.GenerarFeriados(2026);

        Assert.True(r.EsCorrecta, r.Excepcion);
        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("/api/comercial/calendario/2026/generar-feriados", handler.Ruta);
        Assert.Equal("Jueves Santo", Assert.Single(r.Responses!).Descripcion);
    }

    [Fact]
    public void ElMenuTieneLasDosPantallasComercialesConLaRutaYElCodigoDelApi()
    {
        var grupo = MenuSeePos.Items.Single(i => i.Codigo == "COMERCIAL");
        var parametros = grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.PARAMETROS");
        var calendario = grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.CALENDARIO");

        Assert.Equal("/comercial/parametros", parametros.Ruta);
        Assert.Equal("/comercial/calendario", calendario.Ruta);
        Assert.Equal("/comercial/catalogos", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.CATALOGOS").Ruta);
        Assert.Equal("/comercial/clasificacion-clientes", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.CLASIFICACION_CLIENTES").Ruta);
        Assert.Equal("/comercial/clasificacion-articulos", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.CLASIFICACION_ARTICULOS").Ruta);
        Assert.Equal("/comercial/coberturas", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.COBERTURAS").Ruta);
        Assert.Equal("/comercial/metas", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.METAS").Ruta);
        Assert.Equal("/comercial/alertas", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.ALERTAS").Ruta);
        Assert.Equal("/comercial/alertas-supervision", grupo.Hijos.Single(h => h.Codigo == "COMERCIAL.ALERTAS_SUPERVISION").Ruta);
    }

    private sealed class SesionFalsa : IContextoSesion
    {
        public bool Autenticado => true;
        public string? Token => "token-prueba";
        public string? Usuario => "pruebas";
        public bool EsSuperAdministrador => true;
        public bool EsAdministrador => true;
        public string? PerfilCodigo => "SUPER_ADMIN";
        public bool EsCostaPets => false;
        public bool EsAgente => false;
        public bool EsServicioAlCliente => false;
        public bool PermitirExistenciaNegativa => false;
        public bool HabilitaImportaciones => false;
        public int IdSucursal => 1;
        public string? NombreSucursal => "Central";
        public bool TieneSucursal => true;
        public IReadOnlyCollection<string> Menus => [];
        public IReadOnlyCollection<PermisoFuncion> Permisos => [];
        public bool PuedeVer(string funcionCodigo) => true;
        public bool EstaGobernada(string funcionCodigo) => true;
        public bool Puede(string funcionCodigo, AccionPantalla accion) => true;
        public Task CargarAsync() => Task.CompletedTask;
    }
}
