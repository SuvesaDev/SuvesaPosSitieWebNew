using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Models;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Que el menu esconda lo que el rol no puede abrir. Es la primera barrera que ve
/// el usuario; la de verdad esta en el servidor, pero esta no debe fallar.
/// </summary>
public class FiltroMenuTests
{
    private sealed class SesionFalsa : IContextoSesion
    {
        private readonly HashSet<string> _pantallas;

        public SesionFalsa(bool administrador, params string[] pantallas)
        {
            EsAdministrador = administrador;
            _pantallas = new HashSet<string>(pantallas, StringComparer.OrdinalIgnoreCase);
        }

        public bool Autenticado => true;
        public string? Token => "falso";
        public string? Usuario => "prueba";
        public bool EsAdministrador { get; }
        public bool EsCostaPets => false;
        public bool EsAgenteCostaPets => false;
        public int IdSucursal => 1;
        public string? NombreSucursal => "Central";
        public bool TieneSucursal => true;
        public IReadOnlyCollection<string> Menus => Array.Empty<string>();
        public IReadOnlyCollection<PermisoPantalla> Permisos => Array.Empty<PermisoPantalla>();

        public bool EstaGobernada(string pantalla) => true;

        public bool PuedeVer(string pantalla) => EsAdministrador || _pantallas.Contains(pantalla);

        public bool Puede(string pantalla, AccionPantalla accion) => PuedeVer(pantalla);

        public Task CargarAsync() => Task.CompletedTask;
    }

    private static ItemMenu Arbol() => new()
    {
        Titulo = "Ventas",
        Hijos = new ItemMenu[]
        {
            new() { Titulo = "Facturación", Ruta = "/initial/billing" },
            new()
            {
                Titulo = "Presupuestos",
                Hijos = new ItemMenu[]
                {
                    new() { Titulo = "Proformas o Cotización", Ruta = "/sales/budgets/proforma" },
                    new() { Titulo = "Seguimiento Cotizaciones", Ruta = "/sales/budgets/seguimiento" }
                }
            }
        }
    };

    [Fact]
    public void Administrador_VeTodo()
    {
        var visible = FiltroMenu.EsVisible(Arbol(), new SesionFalsa(administrador: true));

        Assert.True(visible);
    }

    [Fact]
    public void SinNingunPermiso_ElGrupoNoSeVe()
    {
        var visible = FiltroMenu.EsVisible(Arbol(), new SesionFalsa(administrador: false));

        Assert.False(visible);
    }

    [Fact]
    public void ConUnaHojaPermitida_ElGrupoSeVe()
    {
        var sesion = new SesionFalsa(false, "Facturación");

        Assert.True(FiltroMenu.EsVisible(Arbol(), sesion));
    }

    [Fact]
    public void ConPermisoEnNietoSolamente_LosDosGruposSeVen()
    {
        // El permiso esta a tres niveles: el filtro debe subir hasta la raiz.
        var sesion = new SesionFalsa(false, "Seguimiento Cotizaciones");
        var raiz = Arbol();

        Assert.True(FiltroMenu.EsVisible(raiz, sesion));

        var presupuestos = raiz.Hijos.Single(h => h.Titulo == "Presupuestos");
        Assert.True(FiltroMenu.EsVisible(presupuestos, sesion));

        var facturacion = raiz.Hijos.Single(h => h.Titulo == "Facturación");
        Assert.False(FiltroMenu.EsVisible(facturacion, sesion));
    }

    [Fact]
    public void Visibles_DejaFueraLasHojasSinPermiso()
    {
        var sesion = new SesionFalsa(false, "Proformas o Cotización");
        var presupuestos = Arbol().Hijos.Single(h => h.Titulo == "Presupuestos");

        var hojas = FiltroMenu.Visibles(presupuestos.Hijos, sesion).ToList();

        Assert.Single(hojas);
        Assert.Equal("Proformas o Cotización", hojas[0].Titulo);
    }

    [Fact]
    public void ElMenuRealNoTieneTitulosDuplicadosEnLaRaiz()
    {
        // Los titulos son la llave de permisos; repetirlos haria ambiguo el filtrado.
        var titulos = MenuSeePos.Items.Select(i => i.Titulo).ToList();

        Assert.Equal(titulos.Count, titulos.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void TitulosRepetidosEnElMenu_EstanIdentificados()
    {
        // Los permisos casan por titulo, no por ruta. Cuando dos pantallas distintas
        // comparten titulo, un solo permiso gobierna las dos: dar "Devoluciones"
        // abre a la vez la de ventas y la de compras.
        //
        // Se hereda del sistema actual, donde no molesta porque el menu no se filtra.
        // Aqui si se filtra, asi que la lista queda fijada: si aparece un titulo
        // repetido nuevo, esta prueba falla y obliga a decidir que hacer con el.
        //
        // Los 4 de aqui, revisados uno por uno (Ola 5):
        //   - "Devoluciones": las dos rutas (/initial/repayment y /sales/repayment)
        //     apuntan al MISMO componente en React, asi que aqui tambien apuntan a
        //     la misma pantalla (DevolucionesVenta.razor con dos @page). Resuelto:
        //     compartir el permiso es correcto, son la misma pantalla.
        //   - "Facturación": una de las dos rutas (/sales/billing) no tiene ruta
        //     real en el enrutador de React — es un enlace muerto del menu
        //     original. La que funciona (/initial/billing) ya esta migrada.
        //   - "Bodegas" y "Toma": ambas apariciones, en ambos casos, son mockup
        //     puro (sin ninguna llamada real al API en ningun lado). No hay
        //     pantalla real detras de ninguna, asi que no hay nada que decidir
        //     todavia.
        static IEnumerable<string> Todos(IEnumerable<ItemMenu> ns) =>
            ns.SelectMany(n => new[] { n.Titulo }.Concat(Todos(n.Hijos)));

        var repetidos = Todos(MenuSeePos.Items)
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(
            new[] { "Bodegas", "Devoluciones", "Facturación", "Toma" },
            repetidos);
    }

    [Fact]
    public void ElMenuRealSeCargoCompleto()
    {
        static int Contar(IEnumerable<ItemMenu> ns) =>
            ns.Sum(n => 1 + Contar(n.Hijos));

        // 8 raices portadas de SidebarData.jsx mas Consignación, que se anadio
        // porque sus rutas existian sin entrada de menu.
        Assert.Equal(9, MenuSeePos.Items.Count);
        Assert.Equal(86, Contar(MenuSeePos.Items));
    }
}
