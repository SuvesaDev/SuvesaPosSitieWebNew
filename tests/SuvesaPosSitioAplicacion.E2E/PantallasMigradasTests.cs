using Microsoft.Playwright;
using Xunit.Abstractions;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Cada pantalla migrada se abre, consulta al API y se pinta sin errores.
///
/// Es la parte de la "definicion de hecho" que faltaba: hasta ahora las pantallas se
/// daban por buenas porque compilaban, y compilar no dice nada sobre si el endpoint
/// responde lo que se espera. Una prueba por pantalla seria mucho ruido; una
/// parametrizada cubre lo mismo y crece anadiendo una linea.
/// </summary>
[Collection(ColeccionE2E.Nombre)]
public class PantallasMigradasTests
{
    private readonly AplicacionEnPruebas _app;
    private readonly NavegadorEnPruebas _navegador;
    private readonly ITestOutputHelper _salida;

    public PantallasMigradasTests(
        AplicacionEnPruebas app, NavegadorEnPruebas navegador, ITestOutputHelper salida)
    {
        _app = app;
        _navegador = navegador;
        _salida = salida;
    }

    public static TheoryData<string, string> Pantallas() => new()
    {
        { "/", "Ola 0 — Cimientos" },
        { "/initial/inventory", "Inventarios" },
        { "/initial/customers", "Clientes" },
        { "/buys/providers", "Proveedores" },
        { "/parameters/bank", "Bancos" },
        { "/sales/budgets/proforma", "Proformas o Cotización" },
        { "/sales/collect", "Abono Cobrar" },
        { "/buys/countswihoutpay", "Cuentas por pagar" },
        { "/sales/budgets/seguimiento", "Seguimiento Cotizaciones" },
        { "/moduloReportes", "Módulo Reportes" },
        { "/initial/documents", "Documentos Emitidos" },
        { "/initial/consultAlbaranes", "Consulta Albaranes" },
        { "/buys/consignment/following", "Seguimiento de Consignaciones" },
        { "/initial/cash/deposits/consultdeposits", "Consulta Depósitos" },
        { "/parameters/family", "Familias" },
        { "/parameters/category", "Categorias" },
        { "/parameters/presentations", "Presentaciones" },
        { "/parameters/users", "Usuarios" },
        { "/buys/orders/checkorders", "Consultar Pedidos" },
        { "/utilities/magitemslist", "Lista articulos MAG" },
        { "/moduloInventario", "Módulo Inventario" }
    };

    [Theory]
    [MemberData(nameof(Pantallas))]
    public async Task SeAbreSinErrores(string ruta, string textoEsperado)
    {
        if (!CredencialesPrueba.Hay)
        {
            return;   // Sin usuario de pruebas no hay nada que abrir.
        }

        var pagina = await EntrarAsync();

        var errores = new List<string>();
        pagina.Console += (_, m) =>
        {
            if (m.Type == "error")
            {
                errores.Add(m.Text);
            }
        };

        await pagina.GotoAsync(ruta);

        // Por rol y nombre accesible. Con texto suelto casaba con el <strong> del
        // aviso de "solo escritorio", que vive en el DOM oculto, y la prueba fallaba
        // con la pantalla perfectamente pintada.
        await Assertions.Expect(
            pagina.GetByRole(AriaRole.Heading, new() { Name = textoEsperado, Exact = true }))
            .ToBeVisibleAsync(new() { Timeout = 30_000 });

        // Que consultar al API no reviente: el manejador convierte cualquier fallo
        // en un alert-danger, asi que su ausencia es la senal de que fue bien.
        await pagina.WaitForTimeoutAsync(1500);

        var fallos = pagina.Locator(".alert-danger");

        if (await fallos.CountAsync() > 0)
        {
            _salida.WriteLine($"{ruta}: {await fallos.First.InnerTextAsync()}");
        }

        Assert.Equal(0, await fallos.CountAsync());
        Assert.True(errores.Count == 0, string.Join("\n", errores));
    }

    private async Task<IPage> EntrarAsync()
    {
        var ctx = await _navegador.ContextoNuevoAsync(_app.Url);
        var p = await ctx.NewPageAsync();

        await p.GotoAsync("/cuenta/ingresar");
        await p.GetByLabel("Usuario").FillAsync(CredencialesPrueba.Usuario!);
        await p.GetByLabel("Contrasena").FillAsync(CredencialesPrueba.Password!);
        await p.GetByRole(AriaRole.Button, new() { Name = "Ingresar" }).ClickAsync();
        await p.WaitForURLAsync("**/cuenta/sucursal", new() { Timeout = 60_000 });

        var opciones = p.Locator("select option");
        await Assertions.Expect(opciones.Nth(1)).ToBeAttachedAsync(new() { Timeout = 60_000 });
        await p.Locator("select").SelectOptionAsync((await opciones.Nth(1).GetAttributeAsync("value"))!);
        await p.GetByRole(AriaRole.Button, new() { Name = "Ingresar" }).ClickAsync();

        await p.WaitForURLAsync(u => !u.Contains("/cuenta/"), new() { Timeout = 60_000 });
        await Assertions.Expect(p.GetByRole(AriaRole.Button, new() { Name = "Salir" }))
            .ToBeVisibleAsync(new() { Timeout = 60_000 });

        return p;
    }
}
