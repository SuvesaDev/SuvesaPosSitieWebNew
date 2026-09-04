using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

// REDISENO_TIPOS_SERIES_CONDICION.md: la Serie lleva ahora la Condición de
// venta (Contado/Crédito, solo si el Tipo ligado es de Facturación) y el
// documento electrónico (switch + Tipo de documento electrónico + 4.4) — ya
// no se derivan del Tipo de Documento. Se quita el bloque de checks "Uso de
// la serie" (Consignación/Crédito/Recibo/Pago): el Tipo (vía su Uso) ya
// indica para qué se usa la serie.
public partial class SeriesFacturacionFiscal
{
    private const string Titulo = "Series de Facturación";

    private HxModal _modal = default!;
    private List<SerieFacturacionFiscalDTO> _items = new();
    private SeriesFacturacionCatalogosFiscalDTO? _cat;
    private SerieFacturacionFiscalDTO? _edicion;
    private bool _nuevo, _cargado;

    private int _filtroEmisor, _filtroSucursal;

    private bool _esCredito;
    // @bind sobre <select> con un bool compara contra "True"/"False" (mayúscula,
    // vía BindConverter), pero las <option> usan "true"/"false": nunca coincidía y
    // el select quedaba en blanco/sin marcar. Se pasa por un proxy string sin
    // ambigüedad (mismo arreglo que Views/Ventas/Facturacion.razor).
    private string EsCreditoTexto
    {
        get => _esCredito ? "true" : "false";
        set => _esCredito = value == "true";
    }
    private bool _requiereElectronico;
    private long _secuenciaMin;
    private readonly List<string> _avisos = new();

    private static readonly (string Codigo, string Nombre)[] CodigosElectronicos =
    {
        ("01", "Factura electrónica"),
        ("03", "Nota de crédito electrónica"),
        ("04", "Tiquete electrónico"),
    };

    private string TituloModal => _nuevo ? "Nueva serie de facturación" : "Editar serie de facturación";

    private IReadOnlyList<SerieCatalogoEmisorFiscalDTO> Emisores => _cat?.Emisores ?? (IReadOnlyList<SerieCatalogoEmisorFiscalDTO>)Array.Empty<SerieCatalogoEmisorFiscalDTO>();
    private IReadOnlyList<SerieCatalogoSucursalFiscalDTO> Sucursales => _cat?.Sucursales ?? (IReadOnlyList<SerieCatalogoSucursalFiscalDTO>)Array.Empty<SerieCatalogoSucursalFiscalDTO>();

    private List<SerieFacturacionFiscalDTO> Filtradas => _items
        .Where(x => (_filtroEmisor == 0 || x.IdEmisor == _filtroEmisor)
                 && (_filtroSucursal == 0 || x.IdSucursal == _filtroSucursal))
        .ToList();

    private SerieCatalogoSucursalFiscalDTO? SucursalSel
        => _edicion is null ? null : _cat?.Sucursales.FirstOrDefault(s => s.Id == _edicion.IdSucursal);

    private SerieCatalogoTipoFacturaFiscalDTO? TipoSel
        => _edicion?.IdTipoFactura is { } id ? _cat?.TiposFactura.FirstOrDefault(t => t.Id == id) : null;

    /// <summary>La condición (contado/crédito) solo aplica a series de Facturación.</summary>
    private bool EsUsoFacturacion => TipoSel?.Uso == "facturacion";

    protected override async Task OnInitializedAsync()
    {
        _cat = await Respuestas.DatoAsync(await Api.Catalogos(), "consultar los catálogos de series");
        await CargarSeries();
    }

    private async Task CargarSeries()
    {
        if (_cargado) return;
        _items = (await Respuestas.DatoAsync(await Api.Obtener(), "consultar las series"))?.ToList() ?? new();
        _cargado = true;
    }

    private async Task Recargar()
    {
        _cargado = false;
        await CargarSeries();
    }

    private async Task Nuevo()
    {
        if (_cat is null) return;
        _nuevo = true;
        _edicion = new SerieFacturacionFiscalDTO();
        _esCredito = false;
        _requiereElectronico = false;
        _secuenciaMin = 0;
        RecalcularAvisos();
        await _modal.ShowAsync();
    }

    private async Task Editar(SerieFacturacionFiscalDTO serie)
    {
        _nuevo = false;
        _edicion = new SerieFacturacionFiscalDTO
        {
            IdSerie = serie.IdSerie,
            Secuencia = serie.Secuencia,
            NumeroTerminal = serie.NumeroTerminal,
            IdSucursal = serie.IdSucursal,
            IdEmisor = serie.IdEmisor,
            Descripcion = serie.Descripcion,
            IdTipoFactura = serie.IdTipoFactura,
            EsCredito = serie.EsCredito,
            RequiereDocumentoElectronico = serie.RequiereDocumentoElectronico,
            CodigoFE = serie.CodigoFE,
            EmisionV44Habilitada = serie.EmisionV44Habilitada,
            NumeroSucursalFE = serie.NumeroSucursalFE,
            TieneDocumentos = serie.TieneDocumentos
        };
        _esCredito = serie.EsCredito;
        _requiereElectronico = serie.RequiereDocumentoElectronico;
        _secuenciaMin = serie.Secuencia;
        RecalcularAvisos();
        await _modal.ShowAsync();
    }

    private void AlCambiarTipo()
    {
        // Si el nuevo tipo no es de Facturación, la condición deja de aplicar.
        if (!EsUsoFacturacion) _esCredito = false;
        RecalcularAvisos();
    }

    private void AlCambiarCondicion() => RecalcularAvisos();

    private void AlToggleElectronico()
    {
        if (_edicion is null) return;
        if (!_requiereElectronico)
        {
            _edicion.CodigoFE = null;
            _edicion.EmisionV44Habilitada = false;
        }
        else if (string.IsNullOrWhiteSpace(_edicion.CodigoFE))
        {
            _edicion.CodigoFE = CodigosElectronicos[0].Codigo;
        }
        RecalcularAvisos();
    }

    private void SugerirDescripcion()
    {
        if (_edicion is null) return;
        var emisor = _cat?.Emisores.FirstOrDefault(e => e.Id == _edicion.IdEmisor)?.Nombre;
        var sucursal = SucursalSel?.Nombre;
        var tipo = TipoSel?.Descripcion;
        var partes = new[] { emisor, sucursal, tipo, _edicion.NumeroTerminal > 0 ? $"Caja {_edicion.NumeroTerminal}" : null }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var sugerida = string.Join(" · ", partes);
        if (!string.IsNullOrWhiteSpace(sugerida)) _edicion.Descripcion = sugerida.Length > 100 ? sugerida[..100] : sugerida;
    }

    private void RecalcularAvisos()
    {
        _avisos.Clear();
        if (_edicion is null) return;

        if (_edicion.IdEmisor <= 0) _avisos.Add("Seleccione el emisor.");
        if (_edicion.IdSucursal <= 0) _avisos.Add("Seleccione la sucursal.");
        else if (SucursalSel is { FEValida: false }) _avisos.Add("La sucursal no tiene número FE de 3 dígitos: configúrelo en Sucursales.");
        if (_edicion.IdTipoFactura is null or <= 0) _avisos.Add("Seleccione el tipo de documento.");
        if (_edicion.NumeroTerminal is < 0 or > 99999) _avisos.Add("La terminal/caja debe estar entre 0 y 99999.");
        if (_edicion.Secuencia < _secuenciaMin) _avisos.Add($"La secuencia no puede ser menor que {_secuenciaMin}.");
        if (string.IsNullOrWhiteSpace(_edicion.Descripcion)) _avisos.Add("Indique una descripción.");

        if (_requiereElectronico)
        {
            if (string.IsNullOrWhiteSpace(_edicion.CodigoFE)) _avisos.Add("Elija el tipo de documento electrónico.");
            if (_esCredito && _edicion.CodigoFE == "04") _avisos.Add("Un tiquete electrónico no puede ser a crédito.");
        }
        else if (_edicion.EmisionV44Habilitada)
        {
            _avisos.Add("Active 'Requiere documento electrónico' antes de emitir 4.4 automáticamente.");
        }
    }

    private string PreviewConsecutivo()
    {
        if (_edicion is null) return "—";
        var su = SucursalSel;
        var codigoFe = _requiereElectronico ? _edicion.CodigoFE : null;
        if (su is null || !su.FEValida || string.IsNullOrWhiteSpace(codigoFe) || codigoFe!.Length != 2
            || _edicion.NumeroTerminal is < 0 or > 99999 || _edicion.Secuencia is < 0 or >= 9999999999)
        {
            return "Complete sucursal (con FE), documento electrónico, terminal y secuencia.";
        }
        return $"{su.NumeroFE} · {_edicion.NumeroTerminal:D5} · {codigoFe} · {(_edicion.Secuencia + 1):D10}";
    }

    private async Task Guardar()
    {
        if (_edicion is null) return;
        RecalcularAvisos();
        if (_avisos.Count > 0) return;

        _edicion.Descripcion = _edicion.Descripcion.Trim();
        _edicion.EsCredito = EsUsoFacturacion && _esCredito;
        _edicion.RequiereDocumentoElectronico = _requiereElectronico;
        _edicion.CodigoFE = _requiereElectronico ? _edicion.CodigoFE : null;

        var respuesta = _nuevo ? await Api.Crear(_edicion) : await Api.Actualizar(_edicion);
        if (await Respuestas.CorrectaAsync(respuesta, "guardar la serie"))
        {
            Dialogos.Exito(_nuevo ? "Serie creada." : "Serie actualizada.");
            await _modal.HideAsync();
            await Recargar();
        }
    }

    private static string GrupoUso(string? uso) => uso switch
    {
        "facturacion" => "Facturación",
        "devolucion" => "Devolución (Nota de crédito)",
        "compra" => "Compra",
        "consignacion" => "Consignación",
        _ => "Otros"
    };
}
