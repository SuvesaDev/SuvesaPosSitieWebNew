using Microsoft.Extensions.Logging.Abstractions;
using SuvesaPosSitioAplicacion.Models;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El espacio de trabajo por pestanas es la pieza a medida del sistema y define
/// como se usa el punto de venta. Estas pruebas fijan la semantica portada desde
/// tabsReducer.js.
/// </summary>
public class EstadoEspacioTrabajoTests
{
    private sealed class AlmacenFalso : IAlmacenEspacioTrabajo
    {
        public EstadoEspacioGuardado? Guardado { get; set; }

        public Task<EstadoEspacioGuardado?> LeerAsync() => Task.FromResult(Guardado);

        public Task GuardarAsync(EstadoEspacioGuardado estado)
        {
            Guardado = estado;
            return Task.CompletedTask;
        }
    }

    private static (EstadoEspacioTrabajo estado, AlmacenFalso almacen) Crear()
    {
        var almacen = new AlmacenFalso();
        var estado = new EstadoEspacioTrabajo(
            almacen,
            NullLogger<EstadoEspacioTrabajo>.Instance);
        return (estado, almacen);
    }

    [Fact]
    public void Abrir_LaMismaPantallaDosVeces_NoDuplicaLaPestana()
    {
        var (estado, _) = Crear();

        estado.Abrir("Clientes", "/initial/customers");
        estado.Abrir("Clientes", "/initial/customers");

        Assert.Single(estado.Pestanas);
        Assert.Equal("Clientes", estado.Actual!.Titulo);
    }

    [Fact]
    public void Abrir_Venta_SiempreAbreUnaNuevaYNumera()
    {
        var (estado, _) = Crear();

        estado.Abrir("Venta", "/initial/billing");
        estado.Abrir("Venta", "/initial/billing");
        estado.Abrir("Venta", "/initial/billing");

        Assert.Equal(3, estado.Pestanas.Count);
        Assert.Equal(new[] { "Venta # 1", "Venta # 2", "Venta # 3" },
                     estado.Pestanas.Select(p => p.Titulo));
        Assert.Equal("/initial/billing/3", estado.Actual!.Ruta);
    }

    [Fact]
    public void Abrir_PantallaYaAbierta_LaTraeAlFrente()
    {
        var (estado, _) = Crear();

        estado.Abrir("Clientes", "/initial/customers");
        estado.Abrir("Inventarios", "/initial/inventory");
        estado.Abrir("Clientes", "/initial/customers");

        Assert.Equal(2, estado.Pestanas.Count);
        Assert.Equal("Clientes", estado.Actual!.Titulo);
    }

    [Fact]
    public void Cerrar_LaActiva_PasaALaAnterior()
    {
        var (estado, _) = Crear();

        var primera = estado.Abrir("Clientes", "/initial/customers");
        var segunda = estado.Abrir("Inventarios", "/initial/inventory");

        estado.Cerrar(segunda.Id);

        Assert.Single(estado.Pestanas);
        Assert.Equal(primera.Id, estado.Actual!.Id);
    }

    [Fact]
    public void Cerrar_LaPrimeraSiendoActiva_PasaALaQueQuedaPrimera()
    {
        var (estado, _) = Crear();

        var primera = estado.Abrir("Clientes", "/initial/customers");
        var segunda = estado.Abrir("Inventarios", "/initial/inventory");
        estado.Seleccionar(primera.Id);

        estado.Cerrar(primera.Id);

        Assert.Equal(segunda.Id, estado.Actual!.Id);
    }

    [Fact]
    public void Cerrar_LaUltima_DejaElEspacioVacio()
    {
        var (estado, _) = Crear();

        var unica = estado.Abrir("Clientes", "/initial/customers");
        estado.Cerrar(unica.Id);

        Assert.Empty(estado.Pestanas);
        Assert.Null(estado.Actual);
    }

    [Fact]
    public void Cerrar_UnaPestana_NoArrastraALaDeNombreParecido()
    {
        // En el sistema actual las pestanas se buscan con name.includes(...), asi que
        // cerrar "Clientes" tambien cerraba "Clientes Frecuentes". Aqui van por id.
        var (estado, _) = Crear();

        var clientes = estado.Abrir("Clientes", "/initial/customers");
        estado.Abrir("Clientes Frecuentes", "/parameters/usualcustomers");

        estado.Cerrar(clientes.Id);

        Assert.Single(estado.Pestanas);
        Assert.Equal("Clientes Frecuentes", estado.Pestanas[0].Titulo);
    }

    [Fact]
    public void CerrarTodas_VaciaYReiniciaElContadorDeVentas()
    {
        var (estado, _) = Crear();

        estado.Abrir("Venta", "/initial/billing");
        estado.Abrir("Venta", "/initial/billing");
        estado.CerrarTodas();

        estado.Abrir("Venta", "/initial/billing");

        Assert.Single(estado.Pestanas);
        Assert.Equal("Venta # 1", estado.Actual!.Titulo);
    }

    [Fact]
    public async Task Restaurar_DevuelveLasPestanasYLaSeleccionada()
    {
        var (primero, almacen) = Crear();
        primero.Abrir("Clientes", "/initial/customers");
        var venta = primero.Abrir("Venta", "/initial/billing");

        // Otro circuito, mismo navegador.
        var segundo = new EstadoEspacioTrabajo(almacen, NullLogger<EstadoEspacioTrabajo>.Instance);
        await segundo.RestaurarAsync();

        Assert.Equal(2, segundo.Pestanas.Count);
        Assert.Equal(venta.Id, segundo.Actual!.Id);
    }

    [Fact]
    public async Task Restaurar_ConservaElContadorDeVentas()
    {
        var (primero, almacen) = Crear();
        primero.Abrir("Venta", "/initial/billing");
        primero.Abrir("Venta", "/initial/billing");

        var segundo = new EstadoEspacioTrabajo(almacen, NullLogger<EstadoEspacioTrabajo>.Instance);
        await segundo.RestaurarAsync();
        segundo.Abrir("Venta", "/initial/billing");

        Assert.Equal("Venta # 3", segundo.Actual!.Titulo);
    }

    [Fact]
    public void Cambio_SeDisparaAlAbrirYAlCerrar()
    {
        var (estado, _) = Crear();
        var avisos = 0;
        estado.Cambio += () => avisos++;

        var p = estado.Abrir("Clientes", "/initial/customers");
        estado.Cerrar(p.Id);

        Assert.Equal(2, avisos);
    }
}
