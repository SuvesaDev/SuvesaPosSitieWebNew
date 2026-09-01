using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Models;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// Que el menu esconda lo que el rol no puede abrir. Rediseno V2: casa por
/// <see cref="ItemMenu.Codigo"/>, no por rotulo.
/// </summary>
public class FiltroMenuTests
{
    private sealed class SesionFalsa : IContextoSesion
    {
        private readonly HashSet<string> _codigos;

        public SesionFalsa(bool superAdmin, params string[] codigos)
        {
            EsSuperAdministrador = superAdmin;
            _codigos = new HashSet<string>(codigos, StringComparer.OrdinalIgnoreCase);
        }

        public bool Autenticado => true;
        public string? Token => "falso";
        public string? Usuario => "prueba";
        public bool EsSuperAdministrador { get; }
        public bool EsAdministrador => EsSuperAdministrador;
        public string? PerfilCodigo => EsSuperAdministrador ? "SUPER_ADMIN" : "USUARIO";
        public bool EsCostaPets => false;
        public bool EsAgenteCostaPets => false;
        public int IdSucursal => 1;
        public string? NombreSucursal => "Central";
        public bool TieneSucursal => true;
        public IReadOnlyCollection<string> Menus => Array.Empty<string>();
        public IReadOnlyCollection<PermisoFuncion> Permisos => Array.Empty<PermisoFuncion>();

        public bool EstaGobernada(string funcionCodigo) => true;
        public bool PuedeVer(string funcionCodigo) => EsSuperAdministrador || _codigos.Contains(funcionCodigo);
        public bool Puede(string funcionCodigo, AccionPantalla accion) => PuedeVer(funcionCodigo);
        public Task CargarAsync() => Task.CompletedTask;
    }

    private static ItemMenu Arbol() => new()
    {
        Titulo = "Ventas",
        Codigo = "VENTAS",
        Hijos = new ItemMenu[]
        {
            new() { Titulo = "Facturación", Codigo = "VENTAS.FACTURACION", Ruta = "/initial/billing" },
            new()
            {
                Titulo = "Presupuestos",
                Codigo = "VENTAS.PRESUPUESTOS",
                Hijos = new ItemMenu[]
                {
                    new() { Titulo = "Proformas o Cotización", Codigo = "VENTAS.PRESUPUESTOS.PROFORMAS_O_COTIZACION", Ruta = "/sales/budgets/proforma" },
                    new() { Titulo = "Seguimiento Cotizaciones", Codigo = "VENTAS.PRESUPUESTOS.SEGUIMIENTO_COTIZACIONES", Ruta = "/sales/budgets/seguimiento" }
                }
            }
        }
    };

    [Fact]
    public void SuperAdministrador_VeTodo()
        => Assert.True(FiltroMenu.EsVisible(Arbol(), new SesionFalsa(superAdmin: true)));

    [Fact]
    public void SinNingunPermiso_ElGrupoNoSeVe()
        => Assert.False(FiltroMenu.EsVisible(Arbol(), new SesionFalsa(superAdmin: false)));

    [Fact]
    public void ConUnaHojaPermitida_ElGrupoSeVe()
        => Assert.True(FiltroMenu.EsVisible(Arbol(), new SesionFalsa(false, "VENTAS.FACTURACION")));

    [Fact]
    public void ConPermisoEnNietoSolamente_LosDosGruposSeVen()
    {
        var sesion = new SesionFalsa(false, "VENTAS.PRESUPUESTOS.SEGUIMIENTO_COTIZACIONES");
        var raiz = Arbol();

        Assert.True(FiltroMenu.EsVisible(raiz, sesion));
        Assert.True(FiltroMenu.EsVisible(raiz.Hijos.Single(h => h.Titulo == "Presupuestos"), sesion));
        Assert.False(FiltroMenu.EsVisible(raiz.Hijos.Single(h => h.Titulo == "Facturación"), sesion));
    }

    [Fact]
    public void Visibles_DejaFueraLasHojasSinPermiso()
    {
        var sesion = new SesionFalsa(false, "VENTAS.PRESUPUESTOS.PROFORMAS_O_COTIZACION");
        var presupuestos = Arbol().Hijos.Single(h => h.Titulo == "Presupuestos");

        var hojas = FiltroMenu.Visibles(presupuestos.Hijos, sesion).ToList();

        Assert.Single(hojas);
        Assert.Equal("Proformas o Cotización", hojas[0].Titulo);
    }

    [Fact]
    public void ElMenuRealNoTieneTitulosDuplicadosEnLaRaiz()
    {
        var titulos = MenuSeePos.Items.Select(i => i.Titulo).ToList();
        Assert.Equal(titulos.Count, titulos.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void ElMenuRealSeCargoCompleto()
    {
        static int Contar(IEnumerable<ItemMenu> ns) => ns.Sum(n => 1 + Contar(n.Hijos));
        Assert.Equal(10, MenuSeePos.Items.Count);
        Assert.Equal(89, Contar(MenuSeePos.Items));
    }
}
