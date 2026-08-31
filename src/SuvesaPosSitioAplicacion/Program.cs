using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
ClienteApi<ICentrosApiCliente, CentrosApiCliente>();
ClienteApi<IBancosApiCliente, BancosApiCliente>();
ClienteApi<IInventarioApiCliente, InventarioApiCliente>();
ClienteApi<ISubFamiliasApiCliente, SubFamiliasApiCliente>();
ClienteApi<IBodegaApiCliente, BodegaApiCliente>();
ClienteApi<ICalculadoraProduccionLotesApiCliente, CalculadoraProduccionLotesApiCliente>();
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
ClienteApi<IConsignacionApiCliente, ConsignacionApiCliente>();
ClienteApi<ICajaApiCliente, CajaApiCliente>();
ClienteApi<IArqueoApiCliente, ArqueoApiCliente>();
ClienteApi<ICierreCajaApiCliente, CierreCajaApiCliente>();
ClienteApi<IFamiliasApiCliente, FamiliasApiCliente>();
ClienteApi<ICategoriasApiCliente, CategoriasApiCliente>();
ClienteApi<IPresentacionApiCliente, PresentacionApiCliente>();
ClienteApi<IHaciendaApiCliente, HaciendaApiCliente>();
ClienteApi<IStockLoteApiCliente, StockLoteApiCliente>();
ClienteApi<IIdentificacionApiCliente, IdentificacionApiCliente>();
ClienteApi<IConfiguracionCostaPetsApiCliente, ConfiguracionCostaPetsApiCliente>();
ClienteApi<IDevolucionVentasApiCliente, DevolucionVentasApiCliente>();
ClienteApi<IDevolucionCompraApiCliente, DevolucionCompraApiCliente>();
ClienteApi<ICobrosApiCliente, CobrosApiCliente>();
ClienteApi<IFormasPagosApiCliente, FormasPagosApiCliente>();

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
builder.Services.AddScoped<IProduccionInventario, ProduccionInventario>();
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
builder.Services.AddScoped<IDocumentosEmitidos, DocumentosEmitidos>();
builder.Services.AddScoped<IAlbaranes, Albaranes>();
builder.Services.AddScoped<IConsignaciones, Consignaciones>();
builder.Services.AddScoped<IDepositosConsulta, DepositosConsulta>();
builder.Services.AddScoped<IFamilias, Familias>();
builder.Services.AddScoped<ICategorias, Categorias>();
builder.Services.AddScoped<IPresentaciones, Presentaciones>();
builder.Services.AddScoped<IUsuarios, Usuarios>();
builder.Services.AddScoped<IRoles, Roles>();
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
            administrador = u.FindFirst(ClaimsSeePos.Administrador)?.Value
        });
    });
}

app.Run();
