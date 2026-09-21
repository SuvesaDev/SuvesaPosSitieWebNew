using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Comisiones;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Views.Reportes;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Contexto independiente de una pestaña de reporte. Se conserva mientras la
/// pestaña esté abierta y se descarta al cerrarla; no se serializan miles de filas
/// al navegador.
/// </summary>
public sealed class EstadoReportePestana
{
    public required string Tipo { get; init; }
    public bool Inicializado { get; set; }
    public bool CatalogosCargados { get; set; }
    public bool FiltrosAvanzadosAbiertos { get; set; }

    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? IdSucursal { get; set; }
    public int? IdEmpresa { get; set; }
    public long? IdCliente { get; set; }
    public int? IdProveedor { get; set; }
    public long? IdArticulo { get; set; }
    public int? IdBodega { get; set; }
    public string NumeroLote { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string EstadoComision { get; set; } = string.Empty;
    public string IdAgente { get; set; } = string.Empty;
    public int? IdFamilia { get; set; }
    public string TipoVenta { get; set; } = string.Empty;
    public bool IncluirAnuladas { get; set; }

    public ReporteOperacionWebDTO? Reporte { get; set; }
    public ComisionConsultaDTO Comisiones { get; set; } = new();
    public DateTime? ConsultadoEn { get; set; }
    public FiltroReporteOperacionWebDTO? FiltroAplicado { get; set; }
    public string EstadoLiquidacionAplicado { get; set; } = string.Empty;
    public GridUserState EstadoRejilla { get; set; } = new(0, []);

    public List<SucursalDTO> Sucursales { get; set; } = [];
    public List<EmpresaDTO> Empresas { get; set; } = [];
    public List<FiltranClienteDTO> Clientes { get; set; } = [];
    public List<ProveedorDTO> Proveedores { get; set; } = [];
    public List<InventarioDTO> Articulos { get; set; } = [];
    public List<Bodega> Bodegas { get; set; } = [];
    public List<FamiliaDTO> Familias { get; set; } = [];

    public bool TieneResultado => Reporte is not null || Comisiones.Movimientos.Count > 0 || ConsultadoEn.HasValue;

    public void CopiarFiltrosDesde(EstadoReportePestana origen, ReporteCatalogo destino)
    {
        // Un desglose debe heredar el alcance que produjo el indicador, no cambios
        // aún no consultados que el usuario haya dejado escritos en el formulario.
        var aplicado = origen.FiltroAplicado;
        if (destino.Tiene(FiltroReporteVisible.Periodo))
        {
            Desde = aplicado?.Desde ?? origen.Desde;
            Hasta = aplicado?.Hasta ?? origen.Hasta;
        }
        if (destino.Tiene(FiltroReporteVisible.Sucursal)) IdSucursal = aplicado?.IdSucursal ?? origen.IdSucursal;
        if (destino.Tiene(FiltroReporteVisible.Empresa)) IdEmpresa = aplicado?.IdEmpresa ?? origen.IdEmpresa;
        if (destino.Tiene(FiltroReporteVisible.Cliente)) IdCliente = aplicado?.IdCliente ?? origen.IdCliente;
        if (destino.Tiene(FiltroReporteVisible.Proveedor)) IdProveedor = aplicado?.IdProveedor ?? origen.IdProveedor;
        if (destino.Tiene(FiltroReporteVisible.Articulo)) IdArticulo = aplicado?.IdArticulo ?? origen.IdArticulo;
        if (destino.Tiene(FiltroReporteVisible.Bodega)) IdBodega = aplicado?.IdBodega ?? origen.IdBodega;
        if (destino.Tiene(FiltroReporteVisible.Lote)) NumeroLote = aplicado?.NumeroLote ?? origen.NumeroLote;
        Inicializado = true;
    }

    public void LimpiarFiltrosNoAplicables(ReporteCatalogo reporte)
    {
        if (!reporte.Tiene(FiltroReporteVisible.Sucursal)) IdSucursal = null;
        if (!reporte.Tiene(FiltroReporteVisible.Empresa)) IdEmpresa = null;
        if (!reporte.Tiene(FiltroReporteVisible.Cliente)) IdCliente = null;
        if (!reporte.Tiene(FiltroReporteVisible.Proveedor)) IdProveedor = null;
        if (!reporte.Tiene(FiltroReporteVisible.Articulo)) IdArticulo = null;
        if (!reporte.Tiene(FiltroReporteVisible.Bodega)) IdBodega = null;
        if (!reporte.Tiene(FiltroReporteVisible.Lote)) NumeroLote = string.Empty;
        if (!reporte.Tiene(FiltroReporteVisible.Texto)) Texto = string.Empty;
        if (!reporte.Tiene(FiltroReporteVisible.Agente)) IdAgente = string.Empty;
        if (!reporte.Tiene(FiltroReporteVisible.Familia)) IdFamilia = null;
        if (!reporte.Tiene(FiltroReporteVisible.TipoVenta)) TipoVenta = string.Empty;
        if (!reporte.Tiene(FiltroReporteVisible.IncluirAnuladas)) IncluirAnuladas = false;
        if (!reporte.Tiene(FiltroReporteVisible.EstadoLiquidacion)) EstadoComision = string.Empty;
    }
}

public interface IEstadoReportes
{
    EstadoReportePestana Obtener(string tipo);
    void Descartar(string tipo);
    void DescartarTodos();
}

public sealed class EstadoReportes : IEstadoReportes, IDisposable
{
    private readonly Dictionary<string, EstadoReportePestana> _estados = new(StringComparer.OrdinalIgnoreCase);
    private readonly IEstadoEspacioTrabajo _espacio;

    public EstadoReportes(IEstadoEspacioTrabajo espacio)
    {
        _espacio = espacio;
        _espacio.PestanaCerrada += AlCerrarPestana;
        _espacio.TodasCerradas += DescartarTodos;
    }

    public EstadoReportePestana Obtener(string tipo)
    {
        if (!_estados.TryGetValue(tipo, out var estado))
        {
            estado = new EstadoReportePestana { Tipo = tipo };
            _estados[tipo] = estado;
        }
        return estado;
    }

    public void Descartar(string tipo) => _estados.Remove(tipo);

    public void DescartarTodos() => _estados.Clear();

    private void AlCerrarPestana(string ruta)
    {
        const string prefijo = "/moduloReportes/";
        if (!ruta.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase)) return;
        var tipo = ruta[prefijo.Length..].Split('?', '#')[0].Trim('/');
        if (tipo.Length > 0) Descartar(tipo);
    }

    public void Dispose()
    {
        _espacio.PestanaCerrada -= AlCerrarPestana;
        _espacio.TodasCerradas -= DescartarTodos;
    }
}
