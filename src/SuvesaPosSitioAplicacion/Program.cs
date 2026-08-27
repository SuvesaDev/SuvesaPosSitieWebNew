using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
using SuvesaPosSitioAplicacion.Views.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Componentes de interfaz sobre Bootstrap 5.
builder.Services.AddHxServices();
builder.Services.AddHxMessenger();
builder.Services.AddHxMessageBoxHost();

// Sesion del usuario, con scope de circuito. El token vive aqui y nunca sale al navegador.
builder.Services.AddScoped<IContextoSesion, ContextoSesion>();
builder.Services.AddScoped<ApiAuthHeaderHandler>();

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

// ProxyClass: lo unico que ven las Views.
builder.Services.AddScoped<ISeguridad, Seguridad>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/healthz", () => Results.Ok(new { estado = "ok", ola = 0 }));

// Diagnostico de la cadena ApiConexion, solo en desarrollo. Comprueba que
// DI -> HttpClient -> ApiAuthHeaderHandler -> cliente generado -> EnvelopeApi
// funciona y que un fallo del API vuelve como ResponseGeneric, no como excepcion.
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
    });
}

app.Run();
