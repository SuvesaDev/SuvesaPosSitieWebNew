using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

// REDISENO_TIPOS_SERIES_CONDICION.md: el Tipo de Documento ya no lleva
// Contado/Credito ni el switch de documento electrónico — eso vive ahora en
// la Serie de Facturación (Views/Parametros/SeriesFacturacionFiscal.razor).
public partial class TiposFacturaFiscal
{
    private const string Titulo = "Tipos de Factura";

    private HxModal _modal = default!;
    private List<TipoFacturaFiscalDTO> _tipos = new();
    private TipoFacturaFiscalDTO? _edicion;
    private bool _esNuevo, _guardando, _cargado;
    private int _filtroUso;
    private readonly List<string> _avisos = new();

    private string TituloModal => _esNuevo ? "Nuevo tipo de documento" : "Editar tipo de documento";

    private List<TipoFacturaFiscalDTO> Filtrados => _tipos
        .Where(t => _filtroUso == 0 || (int)t.Uso == _filtroUso)
        .OrderBy(t => t.Uso).ThenBy(t => t.Descripcion)
        .ToList();

    protected override async Task OnInitializedAsync() => await CargarTipos();

    private async Task CargarTipos()
    {
        if (_cargado) return;
        _tipos = (await Respuestas.DatoAsync(await Api.Obtener(), "consultar los tipos de documento"))?.ToList() ?? new();
        _cargado = true;
    }

    private async Task Recargar()
    {
        _cargado = false;
        await CargarTipos();
    }

    private async Task Agregar()
    {
        _esNuevo = true;
        _edicion = new TipoFacturaFiscalDTO { Uso = UsoTipoDocumento.Facturacion, Activo = true };
        RecalcularAvisos();
        await _modal.ShowAsync();
    }

    private async Task Editar(TipoFacturaFiscalDTO tipo)
    {
        _esNuevo = false;
        _edicion = new TipoFacturaFiscalDTO
        {
            Id = tipo.Id,
            Codigo = tipo.Codigo,
            Descripcion = tipo.Descripcion,
            Uso = tipo.Uso,
            Activo = tipo.Activo,
        };
        RecalcularAvisos();
        await _modal.ShowAsync();
    }

    private void RecalcularAvisos()
    {
        _avisos.Clear();
        if (_edicion is null) return;

        if (_edicion.Codigo <= 0) _avisos.Add("Indique el código interno.");
        if (string.IsNullOrWhiteSpace(_edicion.Descripcion)) _avisos.Add("Indique la descripción.");
    }

    private async Task Guardar()
    {
        if (_edicion is null || _guardando) return;
        _edicion.Descripcion = _edicion.Descripcion?.Trim();
        RecalcularAvisos();
        if (_avisos.Count > 0) return;

        _guardando = true;
        var r = _esNuevo ? await Api.Crear(_edicion) : await Api.Actualizar(_edicion);
        _guardando = false;
        if (await Respuestas.CorrectaAsync(r, "guardar el tipo de documento"))
        {
            Dialogos.Exito(_esNuevo ? "Tipo de documento creado." : "Tipo de documento actualizado.");
            await _modal.HideAsync();
            await Recargar();
        }
    }

    private static string UsoTexto(UsoTipoDocumento u) => u switch
    {
        UsoTipoDocumento.Facturacion => "Facturación",
        UsoTipoDocumento.Devolucion => "Devolución",
        UsoTipoDocumento.Compra => "Compra",
        UsoTipoDocumento.Consignacion => "Consignación",
        _ => u.ToString()
    };

    private static string UsoBadge(UsoTipoDocumento u) => u switch
    {
        UsoTipoDocumento.Facturacion => "text-bg-primary",
        UsoTipoDocumento.Devolucion => "text-bg-danger",
        UsoTipoDocumento.Compra => "text-bg-secondary",
        UsoTipoDocumento.Consignacion => "text-bg-info",
        _ => "text-bg-light"
    };

    private static string UsoAyuda(UsoTipoDocumento u) => u switch
    {
        UsoTipoDocumento.Facturacion => "Aparece en Facturación. La condición (contado/crédito) y el documento electrónico se configuran por Serie.",
        UsoTipoDocumento.Devolucion => "Aparece solo en Devoluciones de venta.",
        UsoTipoDocumento.Compra => "Aparece en Compras.",
        UsoTipoDocumento.Consignacion => "Para las series de consignación.",
        _ => string.Empty
    };
}
