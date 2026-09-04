using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class SeriesFacturacionFiscal
{
    private const string Titulo = "Series de Facturación";

    private HxModal _modal = default!;
    private List<SerieFacturacionFiscalDTO> _items = new();
    private SeriesFacturacionCatalogosFiscalDTO? _cat;
    private SerieFacturacionFiscalDTO? _edicion;
    private bool _nuevo, _cargado;

    private int _filtroEmisor, _filtroSucursal;

    // Los flags Es* del DTO son bool? — se manejan con checkbox bool y se
    // colapsan a bool? al guardar.
    private bool _esConsignacion, _esCredito, _esRecibo, _esPago;
    private long _secuenciaMin;
    private readonly List<string> _avisos = new();

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
        _esConsignacion = _esCredito = _esRecibo = _esPago = false;
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
            EsRecibo = serie.EsRecibo,
            EsPago = serie.EsPago,
            EsConsignacion = serie.EsConsignacion,
            EmisionV44Habilitada = serie.EmisionV44Habilitada,
            CodigoFE = serie.CodigoFE,
            NumeroSucursalFE = serie.NumeroSucursalFE,
            TieneDocumentos = serie.TieneDocumentos
        };
        _esConsignacion = serie.EsConsignacion == true;
        _esCredito = serie.EsCredito == true;
        _esRecibo = serie.EsRecibo == true;
        _esPago = serie.EsPago == true;
        _secuenciaMin = serie.Secuencia;
        RecalcularAvisos();
        await _modal.ShowAsync();
    }

    private void AlCambiarTipo()
    {
        if (TipoSel is null || !TipoSel.CompatibleV44)
        {
            _edicion!.EmisionV44Habilitada = false;
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
        if (_edicion.EmisionV44Habilitada && !(TipoSel?.CompatibleV44 ?? false))
            _avisos.Add("La emisión 4.4 solo aplica a Factura (FE 01), Tiquete (FE 04) o Nota de crédito (FE 05).");
    }

    private string PreviewConsecutivo()
    {
        if (_edicion is null) return "—";
        var su = SucursalSel;
        var codigoFe = TipoSel?.CodigoFE;
        if (su is null || !su.FEValida || string.IsNullOrWhiteSpace(codigoFe) || codigoFe!.Length != 2
            || _edicion.NumeroTerminal is < 0 or > 99999 || _edicion.Secuencia is < 0 or >= 9999999999)
        {
            return "Complete sucursal (con FE), tipo fiscal, terminal y secuencia.";
        }
        return $"{su.NumeroFE} · {_edicion.NumeroTerminal:D5} · {codigoFe} · {(_edicion.Secuencia + 1):D10}";
    }

    private async Task Guardar()
    {
        if (_edicion is null) return;
        RecalcularAvisos();
        if (_avisos.Count > 0) return;

        _edicion.Descripcion = _edicion.Descripcion.Trim();
        // SeriesFacturacion.EsConsignacion no admite NULL en la base (a diferencia
        // de EsCredito/EsRecibo/EsPago, que sí son nullable) — desmarcar la casilla
        // debe mandar false, no null, o el UPDATE falla.
        _edicion.EsConsignacion = _esConsignacion;
        _edicion.EsCredito = _esCredito ? true : null;
        _edicion.EsRecibo = _esRecibo ? true : null;
        _edicion.EsPago = _esPago ? true : null;

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
