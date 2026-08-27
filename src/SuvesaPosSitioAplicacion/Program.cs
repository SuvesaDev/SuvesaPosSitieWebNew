using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Authentication.Cookies;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
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

builder.Services.AddScoped<IContextoSesion, ContextoSesion>();
builder.Services.AddScoped<ApiAuthHeaderHandler>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

// Espacio de trabajo por pestanas. Scope de circuito, persistido en el navegador.
// Sistema de diseno: dialogos y errores del API, cada uno en un solo sitio.
builder.Services.AddScoped<IServicioDialogos, ServicioDialogos>();
builder.Services.AddScoped<IManejadorRespuestas, ManejadorRespuestas>();

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

ClienteApi<IUsuarioApiCliente, UsuarioApiCliente>();
ClienteApi<ICentrosApiCliente, CentrosApiCliente>();
ClienteApi<IBancosApiCliente, BancosApiCliente>();
ClienteApi<IInventarioApiCliente, InventarioApiCliente>();

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

app.MapGet("/healthz", () => Results.Ok(new { estado = "ok", ola = 0 })).AllowAnonymous();

// Diagnostico de la cadena ApiConexion, solo en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/diagnostico/apiconexion", async (ISeguridad seguridad) =>
    {
        var r = await seguridad.ObtenerSucursales();
        return Results.Ok(new
        {
            esCorrecta = r.EsCorrecta,
            excepcion = r.Excepcion,
            cantidadSucursales = r.Responses?.Count
        });
    }).AllowAnonymous();
}

app.Run();
