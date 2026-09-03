using Havit.Blazor.Components.Web.Bootstrap;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class TiposFacturaFiscal
{
    private const string Titulo = "Tipos de Factura";

    private HxModal _modal = default!;
    private List<TipoFacturaFiscalDTO> _tipos = new();
    private List<CodigoFEDisponibleFiscalDTO>? _codigosFE;
    private TipoFacturaFiscalDTO? _edicion;
    private bool _esNuevo, _guardando, _cargado, _esElectronico;
    private int _filtroUso;
    private readonly List<string> _avisos = new();

    private string TituloModal => _esNuevo ? "Nuevo tipo de documento" : "Editar tipo de documento";

    private List<TipoFacturaFiscalDTO> Filtrados => _tipos
        .Where(t => _filtroUso == 0 || (int)t.Uso == _filtroUso)
        .OrderBy(t => t.Uso).ThenBy(t => t.Descripcion)
        .ToList();

    protected override async Task OnInitializedAsync()
    {
        _codigosFE = (await Respuestas.DatoAsync(await Api.CodigosFEDisponibles(), "consultar los códigos FE"))?.ToList() ?? new();
        await CargarTipos();
    }

    private async Task CargarTipos()
    {
        if (_cargado) return;
        _tipos = (await Respuestas.DatoAsync(await Api.Obtener(), "consultar los tipos de documento"))?.ToList() ?? new();
        _cargado = true;
    }

    private async Task Recargar()
    {
        _cargado = false;
        _codigosFE = (await Respuestas.DatoAsync(await Api.CodigosFEDisponibles(), "consultar los códigos FE"))?.ToList() ?? _codigosFE;
        await CargarTipos();
    }

    private async Task Agregar()
    {
        if (_codigosFE is null) return;
        _esNuevo = true;
        _edicion = new TipoFacturaFiscalDTO { Uso = UsoTipoDocumento.Facturacion, Contado = true, Activo = true };
        _esElectronico = false;
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
            Credito = tipo.Credito,
            Contado = tipo.Contado,
            Activo = tipo.Activo,
            CodigoFE = tipo.CodigoFE
        };
        _esElectronico = !string.IsNullOrWhiteSpace(tipo.CodigoFE);
        RecalcularAvisos();
        await _modal.ShowAsync();
    }

    private void AlCambiarUso()
    {
        if (_edicion is null) return;
        if (_edicion.Uso != UsoTipoDocumento.Facturacion)
        {
            _edicion.Contado = false;
            _edicion.Credito = false;
        }
        if (_edicion.Uso == UsoTipoDocumento.Compra)
        {
            _esElectronico = false;
            _edicion.CodigoFE = null;
        }
        AlToggleElectronico();
    }

    private void AlToggleElectronico()
    {
        if (_edicion is null) return;
        if (!_esElectronico)
        {
            _edicion.CodigoFE = null;
        }
        else if (string.IsNullOrWhiteSpace(_edicion.CodigoFE))
        {
            _edicion.CodigoFE = CodigosFEParaUso().FirstOrDefault(c => c.EnUsoPorId is null || c.EnUsoPorId == _edicion.Id)?.Codigo;
        }
        RecalcularAvisos();
    }

    private List<CodigoFEDisponibleFiscalDTO> CodigosFEParaUso()
    {
        if (_edicion is null || _codigosFE is null) return new();
        var permitidos = _edicion.Uso switch
        {
            UsoTipoDocumento.Facturacion => new[] { "01", "04" },
            UsoTipoDocumento.Devolucion => new[] { "03" },
            UsoTipoDocumento.Consignacion => new[] { "01" },
            _ => Array.Empty<string>()
        };
        return _codigosFE.Where(c => permitidos.Contains(c.Codigo)).ToList();
    }

    private void RecalcularAvisos()
    {
        _avisos.Clear();
        if (_edicion is null) return;

        if (_edicion.Codigo <= 0) _avisos.Add("Indique el código interno.");
        if (string.IsNullOrWhiteSpace(_edicion.Descripcion)) _avisos.Add("Indique la descripción.");
        if (_edicion.Uso == UsoTipoDocumento.Facturacion && !_edicion.Contado && !_edicion.Credito)
            _avisos.Add("Un tipo de facturación debe marcar contado y/o crédito.");
        if (_esElectronico && string.IsNullOrWhiteSpace(_edicion.CodigoFE))
            _avisos.Add("Elija el código FE del documento electrónico.");
        if (_esElectronico && _edicion.CodigoFE is { } fe && !CodigosFEParaUso().Any(c => c.Codigo == fe))
            _avisos.Add("Ese código FE no aplica al uso seleccionado.");
    }

    private async Task Guardar()
    {
        if (_edicion is null || _guardando) return;
        _edicion.Descripcion = _edicion.Descripcion?.Trim();
        if (!_esElectronico) _edicion.CodigoFE = null;
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
        UsoTipoDocumento.Facturacion => "Aparece en Facturación (según contado / crédito y el cliente).",
        UsoTipoDocumento.Devolucion => "Aparece solo en Devoluciones de venta.",
        UsoTipoDocumento.Compra => "Aparece en Compras.",
        UsoTipoDocumento.Consignacion => "Para las series de consignación.",
        _ => string.Empty
    };

    private static string Condicion(TipoFacturaFiscalDTO t)
    {
        if (t.Uso != UsoTipoDocumento.Facturacion) return "—";
        var partes = new List<string>();
        if (t.Contado) partes.Add("Contado");
        if (t.Credito) partes.Add("Crédito");
        return partes.Count == 0 ? "—" : string.Join(" / ", partes);
    }
}
