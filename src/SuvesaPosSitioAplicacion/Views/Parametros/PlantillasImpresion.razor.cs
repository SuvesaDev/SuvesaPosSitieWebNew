using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Impresion;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class PlantillasImpresion
{
    private const string Titulo = "Plantillas de impresión";

    private HxModal _modal = default!;
    private List<EmisorFiscalDTO> _emisores = new();
    private List<SerieFacturacionFiscalDTO> _series = new();
    private List<PlantillaImpresionResumenDTO> _items = new();
    private EmisorLogoResumenDTO? _logoEmisor;

    private int _idEmisor;
    private int _tipo;

    private PlantillaImpresionDTO? _edicion;
    private ConfiguracionPlantillaModelo? _cfg;
    private CatalogoPlantillaImpresionDTO? _cat;
    private byte _anchoRollo = 80;
    private string _lineasEncabezado = "";
    private string _lineasPie = "";
    private string? _previewData;
    private bool _guardando;

    private string TituloModal => _edicion?.Id > 0 ? "Editar plantilla" : "Nueva plantilla";

    protected override async Task OnInitializedAsync()
    {
        _emisores = (await Respuestas.DatoAsync(await EmisoresApi.Obtener(), "consultar los emisores"))?.ToList() ?? new();
    }

    private async Task Cargar()
    {
        _items = new();
        if (_idEmisor <= 0) return;

        var slug = _tipo > 0 ? TiposImpresionUi.Slug(_tipo) : null;
        _items = (await Respuestas.DatoAsync(await Api.Listar(_idEmisor, slug), "consultar las plantillas"))?.ToList() ?? new();
    }

    private async Task Nueva()
    {
        if (_idEmisor <= 0 || _tipo <= 0) return;

        var slug = TiposImpresionUi.Slug(_tipo);
        _cat = await Respuestas.DatoAsync(await Api.Catalogo(slug), "consultar el catálogo de la plantilla");
        if (_cat is null) return;

        _edicion = new PlantillaImpresionDTO
        {
            Id = 0,
            IdEmisor = _idEmisor,
            TipoDocumento = _tipo,
            Nombre = TiposImpresionUi.Nombre(_tipo),
            Formato = 1,
            Activa = true,
            ConfiguracionJson = _cat.ConfiguracionPorDefectoA4Json,
        };
        _cfg = ConfiguracionPlantillaModelo.Desde(_cat.ConfiguracionPorDefectoA4Json);
        _anchoRollo = 80;
        _previewData = null;
        SincronizarTextareas(desdeModelo: true);
        await CargarSeries();
        await CargarEstadoLogoEmisor();
        await _modal.ShowAsync();
    }

    private async Task Editar(int id)
    {
        var dto = await Respuestas.DatoAsync(await Api.Obtener(id), "consultar la plantilla");
        if (dto is null) return;

        _edicion = dto;
        _cat = await Respuestas.DatoAsync(await Api.Catalogo(TiposImpresionUi.Slug(dto.TipoDocumento)), "consultar el catálogo de la plantilla");
        _cfg = ConfiguracionPlantillaModelo.Desde(dto.ConfiguracionJson);
        _anchoRollo = dto.AnchoRolloMm ?? 80;
        _previewData = null;
        SincronizarTextareas(desdeModelo: true);
        await CargarSeries();
        await CargarEstadoLogoEmisor();
        await _modal.ShowAsync();
    }

    private async Task CargarEstadoLogoEmisor()
    {
        if (_edicion?.IdEmisor is not > 0)
        {
            _logoEmisor = new EmisorLogoResumenDTO();
            return;
        }

        _logoEmisor = await Respuestas.DatoAsync(await EmisoresApi.LogoMetadata(_edicion.IdEmisor), "consultar el logo del emisor")
            ?? new EmisorLogoResumenDTO();
    }

    private async Task CargarSeries()
    {
        _series = new();
        if (_cat?.UsaSerie != true || _idEmisor <= 0) return;
        var todas = await Respuestas.DatoAsync(await SeriesApi.Obtener(), "consultar las series");
        _series = todas?.Where(s => s.IdEmisor == _idEmisor).ToList() ?? new();
    }

    private void AplicarPreset()
    {
        if (_cfg is null || _edicion is null) return;

        var termico = _edicion.Formato == 2;
        _cfg.Layout.EncabezadoDosColumnas = !termico && _cfg.Layout.Preset == "corporativo-a4";
        _cfg.Layout.TotalesDestacados = true;
        _cfg.Qr.Mostrar = !termico;
        _cfg.Qr.Payload = "https://costapets.com/";
        _cfg.MontoEnLetras.Mostrar = !termico;

        if (_cfg.Layout.Preset == "minimal-a4")
        {
            _cfg.Tema.Nombre = "minimal";
            _cfg.Tema.ColorPrimario = "#202020";
            _cfg.Tema.ColorTotal = "#202020";
            _cfg.Tema.ColorSecundario = "#F3F3F3";
            return;
        }

        _cfg.Tema.Nombre = termico ? "termico" : "corporativo";
        _cfg.Tema.ColorPrimario = termico ? "#202020" : "#1072A9";
        _cfg.Tema.ColorTotal = termico ? "#202020" : "#0D5B88";
        _cfg.Tema.ColorSecundario = termico ? "#F3F3F3" : "#EEF5F8";
    }

    private void SincronizarTextareas(bool desdeModelo)
    {
        if (_cfg is null) return;
        if (desdeModelo)
        {
            _lineasEncabezado = string.Join("\n", _cfg.Encabezado.LineasTexto);
            _lineasPie = string.Join("\n", _cfg.Pie.LineasTexto);
        }
        else
        {
            _cfg.Encabezado.LineasTexto = Trocear(_lineasEncabezado);
            _cfg.Pie.LineasTexto = Trocear(_lineasPie);
        }
    }

    private static List<string> Trocear(string s) => (s ?? "")
        .Replace("\r", "")
        .Split('\n', StringSplitOptions.TrimEntries)
        .Where(x => x.Length > 0)
        .ToList();

    private async Task Previsualizar()
    {
        if (_edicion is null || _cfg is null || _edicion.Id <= 0) return;
        SincronizarTextareas(desdeModelo: false);

        var r = await Api.Previsualizar(_edicion.Id, _cfg.AJson(), _edicion.Formato);
        if (r is { EsCorrecta: true, Responses: { Length: > 0 } bytes })
            _previewData = "data:application/pdf;base64," + Convert.ToBase64String(bytes);
        else
            await Dialogos.ErrorAsync(r.Excepcion ?? "No se pudo previsualizar.");
    }

    private async Task Guardar()
    {
        if (_edicion is null || _cfg is null) return;
        _guardando = true;
        try
        {
            SincronizarTextareas(desdeModelo: false);
            _edicion.ConfiguracionJson = _cfg.AJson();
            _edicion.AnchoRolloMm = _edicion.Formato == 2 ? _anchoRollo : null;
            if (!TiposImpresionUi.UsaSerie(_edicion.TipoDocumento)) _edicion.IdSerie = null;

            var r = _edicion.Id > 0 ? await Api.Actualizar(_edicion) : await Api.Crear(_edicion);
            var nuevoId = await Respuestas.DatoAsync(r, "guardar la plantilla");
            if (!r.EsCorrecta) return;

            Dialogos.Exito("Plantilla guardada.");
            if (_edicion.Id <= 0 && nuevoId > 0) _edicion.Id = nuevoId;
            await Cargar();
        }
        finally { _guardando = false; }
    }

    private async Task Predeterminada(int id)
    {
        if (await Respuestas.CorrectaAsync(await Api.MarcarPredeterminada(id), "marcar la plantilla como predeterminada"))
            await Cargar();
    }

    private async Task Desactivar(int id)
    {
        if (!await Dialogos.ConfirmarPeligroAsync("¿Desactivar esta plantilla?", "Plantillas")) return;
        if (await Respuestas.CorrectaAsync(await Api.Desactivar(id), "desactivar la plantilla"))
            await Cargar();
    }
}
