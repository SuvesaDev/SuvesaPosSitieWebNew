using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Authentication;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using SuvesaPosSitioAplicacion.ApiConexion;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
using SuvesaPosSitioAplicacion.Services;
using SuvesaPosSitioAplicacion.Views.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Componentes de interfaz sobre Bootstrap 5.
builder.Services.AddHxServices();
builder.Services.AddHxMessenger();
builder.Services.AddHxMessageBoxHost();

// MudBlazor: solo por MudDataGrid en consultas pesadas. Registra sus servicios
// (popover, resize, keyinterceptor); el CSS/JS y los proveedores van aparte, y
// solo se activan en las pantallas que montan <MudIsla>.
builder.Services.AddMudServices();

// ---------------------------------------------------------------------------
// Sesion y autorizacion
// ---------------------------------------------------------------------------

// Respalda el almacen de tickets. En una sola instancia basta con memoria;
// para varias, cambiar por Redis o SQL sin tocar AlmacenTickets.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<AlmacenTickets>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/cuenta/ingresar";
        opciones.LogoutPath = "/cuenta/salir";
        opciones.AccessDeniedPath = "/cuenta/sin-permiso";
        opciones.ExpireTimeSpan = TimeSpan.FromHours(12);
        opciones.SlidingExpiration = true;

        opciones.Cookie.Name = "seepos.sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// El ticket completo (token del API y ~82 permisos) se guarda en servidor.
// La cookie del navegador solo lleva la llave.
builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<AlmacenTickets>((opciones, almacen) => opciones.SessionStore = almacen);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization(opciones =>
{
    // Todo exige sesion salvo lo que se marque con [AllowAnonymous].
    opciones.FallbackPolicy = opciones.DefaultPolicy;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IContextoSesion, ContextoSesion>();
builder.Services.AddScoped<ApiAuthHeaderHandler>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

// Espacio de trabajo por pestanas. Scope de circuito, persistido en el navegador.
// Sistema de diseno: dialogos y errores del API, cada uno en un solo sitio.
builder.Services.AddScoped<IServicioDialogos, ServicioDialogos>();
builder.Services.AddScoped<IManejadorRespuestas, ManejadorRespuestas>();

// Comprueba si la SPA React esta viva, para no mostrar un iframe en blanco.
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISondaLegado, SondaLegado>();

// PDF. QuestPDF exige aceptar su licencia al arrancar; la comunitaria es gratuita
// solo por debajo de cierto umbral de facturacion. VERIFICAR antes de produccion.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddSingleton<IGeneradorPdf, GeneradorPdfQuestPdf>();

builder.Services.AddScoped<IAlmacenEspacioTrabajo, AlmacenEspacioTrabajoNavegador>();
builder.Services.AddScoped<IEstadoEspacioTrabajo, EstadoEspacioTrabajo>();

// ---------------------------------------------------------------------------
// ApiConexion: un HttpClient tipado por cada cliente generado desde el OpenAPI.
// Todos comparten la URL base y el handler que inyecta el token.
// ---------------------------------------------------------------------------
var urlApi = builder.Configuration["SeePos:ApiBaseUrl"]
             ?? throw new InvalidOperationException(
                 "Falta SeePos:ApiBaseUrl. Configurelo en appsettings.Development.json o por variable de entorno.");

void ClienteApi<TInterfaz, TImplementacion>()
    where TInterfaz : class
    where TImplementacion : class, TInterfaz
{
    builder.Services.AddHttpClient<TInterfaz, TImplementacion>(c => c.BaseAddress = new Uri(urlApi))
        .AddHttpMessageHandler<ApiAuthHeaderHandler>();
}

// Excepcion puntual para endpoints cuyo contrato OpenAPI tiene un tipo incorrecto.
// Conserva la misma URL base y el mismo handler que los clientes tipados.
builder.Services.AddHttpClient("SeePosApi", c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<ApiAuthHeaderHandler>();

ClienteApi<IUsuarioApiCliente, UsuarioApiCliente>();

// Cliente hecho a mano para /seguridad/* (contratos NSwag no regenerables en local).
builder.Services.AddHttpClient<ISeguridadApiCliente, SeguridadApiCliente>(c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<ApiAuthHeaderHandler>();

// Cliente hecho a mano para el CRUD de /ConfiguracionBonificacion/* (§3.1, contratos NSwag no regenerables en local).
builder.Services.AddHttpClient<IBonificacionApiCliente, BonificacionApiCliente>(c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<ApiAuthHeaderHandler>();
// Cliente a mano para lotes / movimientos / toma física (MEJORA_LOTES_API.md).
builder.Services.AddHttpClient<ILotesApiCliente, LotesApiCliente>(c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<ApiAuthHeaderHandler>();
builder.Services.AddHttpClient<IProduccionApiCliente, ProduccionApiCliente>(c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<ApiAuthHeaderHandler>();
builder.Services.AddHttpClient<IConsignacionInvApiCliente, ConsignacionInvApiCliente>(c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<ApiAuthHeaderHandler>();
ClienteApi<ICentrosApiCliente, CentrosApiCliente>();
ClienteApi<IBancosApiCliente, BancosApiCliente>();
ClienteApi<IInventarioApiCliente, InventarioApiCliente>();
ClienteApi<ISubFamiliasApiCliente, SubFamiliasApiCliente>();
ClienteApi<IBodegaApiCliente, BodegaApiCliente>();
ClienteApi<IArticulosImagenesApiCliente, ArticulosImagenesApiCliente>();
ClienteApi<IArticulosRelacionadosApiCliente, ArticulosRelacionadosApiCliente>();
ClienteApi<ICartaExoneracionApiCliente, CartaExoneracionApiCliente>();
ClienteApi<IOrdenCompraApiCliente, OrdenCompraApiCliente>();
ClienteApi<ICotizacionApiCliente, CotizacionApiCliente>();
ClienteApi<IAbonoPagarApiCliente, AbonoPagarApiCliente>();
ClienteApi<IClienteApiCliente, ClienteApiCliente>();
ClienteApi<IGeografiaApiCliente, GeografiaApiCliente>();
ClienteApi<IProveedorApiCliente, ProveedorApiCliente>();
ClienteApi<IAbonoCobrarApiCliente, AbonoCobrarApiCliente>();
ClienteApi<IReportesApiCliente, ReportesApiCliente>();
ClienteApi<IVentaApiCliente, VentaApiCliente>();
ClienteApi<ITipoFacturaApiCliente, TipoFacturaApiCliente>();
ClienteApi<IComprasApiCliente, ComprasApiCliente>();
ClienteApi<IMonedaApiCliente, MonedaApiCliente>();
ClienteApi<IQvetApiCliente, QvetApiCliente>();
ClienteApi<ICajaApiCliente, CajaApiCliente>();
ClienteApi<IArqueoApiCliente, ArqueoApiCliente>();
ClienteApi<ICierreCajaApiCliente, CierreCajaApiCliente>();
ClienteApi<IFamiliasApiCliente, FamiliasApiCliente>();
ClienteApi<ICategoriasApiCliente, CategoriasApiCliente>();
ClienteApi<IPresentacionApiCliente, PresentacionApiCliente>();
ClienteApi<IHaciendaApiCliente, HaciendaApiCliente>();
ClienteApi<IStockLoteApiCliente, StockLoteApiCliente>();
ClienteApi<IStocksApiCliente, StocksApiCliente>();
ClienteApi<IIdentificacionApiCliente, IdentificacionApiCliente>();
ClienteApi<IConfiguracionCostaPetsApiCliente, ConfiguracionCostaPetsApiCliente>();
ClienteApi<IDevolucionVentasApiCliente, DevolucionVentasApiCliente>();
ClienteApi<IDevolucionCompraApiCliente, DevolucionCompraApiCliente>();
ClienteApi<ICobrosApiCliente, CobrosApiCliente>();
ClienteApi<IFormasPagosApiCliente, FormasPagosApiCliente>();
ClienteApi<IConfiguracionBonificacionApiCliente, ConfiguracionBonificacionApiCliente>();
ClienteApi<IClienteBonificacionApiCliente, ClienteBonificacionApiCliente>();
ClienteApi<IArticuloBonificacionApiCliente, ArticuloBonificacionApiCliente>();
ClienteApi<IAgenteventaApiCliente, AgenteventaApiCliente>();

// ---------------------------------------------------------------------------
// Convivencia: YARP sirve la SPA React bajo el mismo origen mientras queden
// pantallas sin migrar. Se retira entero al cerrar la Ola 6.
// ---------------------------------------------------------------------------
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ProxyClass: lo unico que ven las Views.
builder.Services.AddScoped<ISeguridad, Seguridad>();
builder.Services.AddScoped<IBancos, Bancos>();
builder.Services.AddScoped<IInventarioConsulta, InventarioConsulta>();
builder.Services.AddScoped<ICatalogosInventario, CatalogosInventario>();
builder.Services.AddScoped<IImagenesArticulo, ImagenesArticulo>();
builder.Services.AddScoped<IArticulosRelacionados, ArticulosRelacionados>();
builder.Services.AddScoped<ICartasExoneracion, CartasExoneracion>();
builder.Services.AddScoped<IOrdenesCompraConsulta, OrdenesCompraConsulta>();
builder.Services.AddScoped<ICotizaciones, Cotizaciones>();
builder.Services.AddScoped<ICuentasPorPagar, CuentasPorPagar>();
builder.Services.AddScoped<IClientesConsulta, ClientesConsulta>();
builder.Services.AddScoped<IGeografia, Geografia>();
builder.Services.AddScoped<IProveedoresConsulta, ProveedoresConsulta>();
builder.Services.AddScoped<ICuentasPorCobrar, CuentasPorCobrar>();
builder.Services.AddScoped<IReportes, Reportes>();
builder.Services.AddScoped<IFacturacion, Facturacion>();
builder.Services.AddScoped<ICompras, Compras>();
builder.Services.AddScoped<IBandejaDocumentos, BandejaDocumentos>();
builder.Services.AddScoped<IAlbaranes, Albaranes>();
builder.Services.AddScoped<IDepositosConsulta, DepositosConsulta>();
builder.Services.AddScoped<IFamilias, Familias>();
builder.Services.AddScoped<ICategorias, Categorias>();
builder.Services.AddScoped<IPresentaciones, Presentaciones>();
builder.Services.AddScoped<IUsuarios, Usuarios>();
builder.Services.AddScoped<IRolesPermisos, RolesPermisos>();
builder.Services.AddScoped<IPerfiles, Perfiles>();
builder.Services.AddScoped<ISucursales, Sucursales>();
builder.Services.AddScoped<IEmpresas, Empresas>();
builder.Services.AddScoped<IConfiguracion, Configuracion>();
builder.Services.AddScoped<IDevolucionesVenta, DevolucionesVentaServicio>();
builder.Services.AddScoped<IDevolucionesCompra, DevolucionesCompra>();
builder.Services.AddScoped<IOrdenesCompra, OrdenesCompra>();
builder.Services.AddScoped<IEntregasCuenta, EntregasCuenta>();
builder.Services.AddScoped<ICobros, Cobros>();
builder.Services.AddScoped<ICajaOperaciones, CajaOperaciones>();
builder.Services.AddScoped<ITiposFactura, TiposFacturaFiscal>();
builder.Services.AddScoped<ITiposIdentificacionFiscales, TiposIdentificacionFiscales>();
builder.Services.AddScoped<IImpuestosFiscales, ImpuestosFiscales>();
builder.Services.AddScoped<ITiposCobroFiscales, TiposCobroFiscales>();
builder.Services.AddScoped<IFormasPagoFiscales, FormasPagoFiscales>();
builder.Services.AddScoped<ITiposExoneracionFiscales, TiposExoneracionFiscales>();
builder.Services.AddScoped<IMonedasFiscales, MonedasFiscales>();
builder.Services.AddScoped<IDenominacionesMonedaFiscales, DenominacionesMonedaFiscales>();
builder.Services.AddScoped<IConfiguracionPlazosFiscales, ConfiguracionPlazosFiscales>();
builder.Services.AddScoped<IEmisoresFiscales, EmisoresFiscales>();
builder.Services.AddScoped<ISeriesFacturacionFiscales, SeriesFacturacionFiscales>();
builder.Services.AddScoped<IBandejaFiscal, BandejaFiscal>();
builder.Services.AddScoped<IGeografiaFiscal, GeografiaFiscal>();
builder.Services.AddScoped<ICatalogoBonificacion, CatalogoBonificacion>();
builder.Services.AddScoped<IClienteBonificacion, ClienteBonificacion>();
builder.Services.AddScoped<IArticuloBonificacion, ArticuloBonificacion>();

// Motor de correo de comprobantes (MOTOR_CORREO_COMPROBANTES_WEB.md).
builder.Services.AddScoped<IConfiguracionCorreo, ConfiguracionCorreo>();
builder.Services.AddScoped<IEnviosCorreo, EnviosCorreo>();
builder.Services.AddScoped<IAlertasAdministrador, AlertasAdministrador>();

// Motor de plantillas de impresión (MOTOR_PLANTILLAS_IMPRESION_WEB.md).
builder.Services.AddScoped<IPlantillasImpresion, PlantillasImpresion>();
builder.Services.AddScoped<IImpresionDocumentos, ImpresionDocumentos>();

// Abono Cobrar — preventas pendientes + emisión síncrona (ABONO_COBRAR_PREVENTAS_WEB.md).
builder.Services.AddScoped<IAbonoCobrarPreventas, AbonoCobrarPreventas>();

// SANEAMIENTO Fase 8 — cobro de facturas de crédito (mayor de CxC).
builder.Services.AddScoped<ICobrosCredito, CobrosCredito>();

// SANEAMIENTO Fase 8.1 — catálogo de series operativas (no fiscales).
builder.Services.AddScoped<ISeriesOperativas, SeriesOperativas>();

// SANEAMIENTO Fase 8.4 — conciliación de caja desde el mayor de movimientos.
builder.Services.AddScoped<IConciliacionCaja, ConciliacionCaja>();

// SANEAMIENTO Fase 8.2/8.3 — consultas de "Cobrar" y perfiles de emisión.
builder.Services.AddScoped<IConsultaCobros, ConsultaCobros>();
builder.Services.AddScoped<IPerfilesEmision, PerfilesEmision>();
builder.Services.AddScoped<IVentaOrquestada, VentaOrquestada>();
builder.Services.AddScoped<IFormasPagoConfig, FormasPagoConfig>();
builder.Services.AddScoped<INotaCreditoCxC, NotaCreditoCxC>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// AllowAnonymous es obligatorio: la FallbackPolicy de autorizacion se aplica a
// TODOS los endpoints, estaticos incluidos. Sin esto, el CSS y el JS se redirigen
// al login con 302 y el navegador intenta ejecutar el HTML del login como script.
app.MapStaticAssets().AllowAnonymous();
// El proxy va antes que los componentes para que /legado y /assets no caigan
// en la ruta comodin de Blazor.
app.MapReverseProxy();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Cierre de sesion. Endpoint y no pagina: no tiene nada que mostrar.
app.MapPost("/cuenta/salir", async (HttpContext ctx, IServicioAutenticacion auth) =>
{
    await auth.SalirAsync(ctx);
    return Results.Redirect("/cuenta/ingresar");
}).AllowAnonymous();

app.MapGet("/cuenta/salir", async (HttpContext ctx, IServicioAutenticacion auth) =>
{
    await auth.SalirAsync(ctx);
    return Results.Redirect("/cuenta/ingresar");
}).AllowAnonymous();

app.MapGet("/favicon.ico", () => Results.Redirect("/favicon.png")).AllowAnonymous();

// PDF de representación gráfica de un documento (MOTOR_PLANTILLAS_IMPRESION_WEB.md §4).
// El render vive en el API; este endpoint solo reenvía con el token y hace stream.
app.MapGet("/documentos/{tipo}/{id:long}/pdf", async (
    string tipo, long id, string? formato, bool copia,
    IImpresionDocumentos api) =>
{
    var r = await api.Pdf(tipo, id, formato, copia);
    if (!r.EsCorrecta || r.Responses is not { Length: > 0 } bytes)
        return Results.Problem(r.Excepcion ?? "No se pudo generar el documento.");

    var nombre = $"{tipo}-{id}.pdf";
    return Results.File(bytes, "application/pdf", nombre, enableRangeProcessing: false);
});

// Descarga de reportes en PDF. Un endpoint y no una pagina: enviar un archivo desde
// un componente interactivo obligaria a pasarlo por JS codificado en base64.
app.MapGet("/reportes/compras/pdf", async (IReportes api, IGeneradorPdf pdf) =>
{
    var r = await api.Compras();

    if (!r.EsCorrecta)
    {
        return Results.Problem(r.Excepcion ?? "No se pudo consultar el reporte.");
    }

    var compras = r.Responses ?? (ICollection<ReporteComprasDTO>)Array.Empty<ReporteComprasDTO>();

    var bytes = pdf.Tabla(new ReporteTabular(
        Titulo: "Reporte de compras",
        Subtitulo: "Facturas de compra registradas",
        Encabezados: new[] { "Factura", "Proveedor", "Fecha", "Gravado", "Impuesto", "Total" },
        Filas: compras.Select(c => (IReadOnlyList<string>)new[]
        {
            c.Factura ?? "",
            c.Nombre ?? "",
            c.Fecha.ToString("dd/MM/yyyy"),
            Formato.Importe(c.SubTotalGravado),
            Formato.Importe(c.Impuesto),
            Formato.Importe(c.TotalFactura)
        }).ToList(),
        Totales: new[] { "", "", "", "", "Total",
            Formato.Importe(compras.Sum(c => Formato.AImporte(c.TotalFactura))) })
    {
        ColumnasNumericas = new HashSet<int> { 3, 4, 5 }
    });

    return Results.File(bytes, "application/pdf", "reporte-compras.pdf");
});

app.MapGet("/reportes/cuentas-por-pagar", async (
    ICuentasPorPagar api,
    IGeneradorPdf pdf) =>
{
    var r = await api.ObtenerDeudas();

    if (!r.EsCorrecta)
    {
        return Results.Problem(r.Excepcion ?? "No se pudieron consultar las deudas.");
    }

    var filas = new List<IReadOnlyList<string>>();
    decimal total = 0;

    foreach (var proveedor in r.Responses ?? (ICollection<BuscarProveedorPendientesDTO>)Array.Empty<BuscarProveedorPendientesDTO>())
    {
        foreach (var f in proveedor.Facturas ?? (ICollection<FacturasPendientesPagoDTO>)Array.Empty<FacturasPendientesPagoDTO>())
        {
            var saldo = Formato.AImporte(f.SaldoActual ?? 0);
            total += saldo;

            filas.Add(new[]
            {
                proveedor.Nombre ?? "",
                f.NumeroFactura ?? "",
                f.Fecha.ToString("dd/MM/yyyy"),
                Formato.Importe(f.MontoFactura),
                Formato.Importe(saldo)
            });
        }
    }

    var bytes = pdf.Tabla(new ReporteTabular(
        Titulo: "Cuentas por pagar",
        Subtitulo: "Facturas pendientes por proveedor",
        Encabezados: new[] { "Proveedor", "Factura", "Fecha", "Monto", "Saldo" },
        Filas: filas,
        Totales: new[] { "", "", "", "Total", Formato.Importe(total) })
    {
        ColumnasNumericas = new HashSet<int> { 3, 4 }
    });

    return Results.File(bytes, "application/pdf", "cuentas-por-pagar.pdf");
});

app.MapGet("/healthz", () => Results.Ok(new { estado = "ok", ola = 0 })).AllowAnonymous();

// Diagnostico de sesion, solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    // SE QUEDA a proposito, contra lo que dije al empezar. Fue lo que dio el dato
    // decisivo en el fallo del token, y cuando alguien reporta un problema es mas
    // rapido que correr la suite. Solo en desarrollo, y **nunca devuelve el token**,
    // solo su largo.
    app.MapGet("/diagnostico/sesion", (HttpContext ctx) =>
    {
        var u = ctx.User;
        var token = u.FindFirst(ClaimsSeePos.Token)?.Value;

        return Results.Ok(new
        {
            autenticado = u.Identity?.IsAuthenticated ?? false,
            usuario = u.Identity?.Name,
            claims = u.Claims.Count(),
            hayToken = !string.IsNullOrWhiteSpace(token),
            largoToken = token?.Length ?? 0,
            permisos = u.FindAll(ClaimsSeePos.Permiso).Count(),
            idSucursal = u.FindFirst(ClaimsSeePos.IdSucursal)?.Value,
            esSuperAdmin = u.FindFirst(ClaimsSeePos.EsSuperAdministrador)?.Value,
            perfilCodigo = u.FindFirst(ClaimsSeePos.PerfilCodigo)?.Value
        });
    });
}

app.Run();
