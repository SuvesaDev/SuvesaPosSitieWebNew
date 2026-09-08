using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SuvesaPosSitioAplicacion.DTOs.Bandeja;

namespace SuvesaPosSitioAplicacion.Views.Documentos;

public partial class Bandeja
{
    private const string Titulo = "Bandeja de documentos";
    private static readonly string[] _pestanas = { "Preventas", "Facturas", "Notas de Crédito", "Consignaciones" };

    private int _tab;
    private DateTime _desde = DateTime.Today.AddDays(-7);
    private DateTime _hasta = DateTime.Today;
    private string _texto = string.Empty;
    private string _estadoHacienda = string.Empty;
    private bool _incluirAnulados;
    private int _pagina = 1;
    private const int _tamano = 25;
    private int _total;
    private bool _cargando;

    private List<DocumentoBandeja> _comunes = new();
    private List<DocumentoFiscalBandeja> _fiscales = new();

    private HxOffcanvas _detalle = default!;
    private HxModal _modalFiscal = default!;
    private FacturaBandejaDetalle? _detFactura;
    private NotaCreditoBandejaDetalle? _detNC;
    private DocumentoBandeja? _detComun;
    private string? _claveFiscal;
    private string? _contenidoFiscal;

    private bool EsFiscal => _tab is 1 or 2;

    private IReadOnlyList<DocumentoBandeja> Filas => EsFiscal ? _fiscales : _comunes;

    protected override Task OnInitializedAsync() => Cargar();

    private async Task CambiarTab(int t)
    {
        if (_tab == t) return;
        _tab = t;
        _pagina = 1;
        await Cargar();
    }

    private async Task Buscar()
    {
        _pagina = 1;
        await Cargar();
    }

    private async Task Anterior()
    {
        if (_pagina <= 1) return;
        _pagina--;
        await Cargar();
    }

    private async Task Siguiente()
    {
        _pagina++;
        await Cargar();
    }

    private async Task BuscarConEnter(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await Buscar();
    }

    private async Task Cargar()
    {
        if (_desde.Date > _hasta.Date)
        {
            await Dialogos.ErrorAsync("La fecha inicial no puede ser posterior a la final.");
            return;
        }

        _cargando = true;
        var f = new BandejaDocumentosFiltro
        {
            Desde = _desde,
            Hasta = _hasta,
            Texto = Nulo(_texto),
            EstadoHacienda = EsFiscal ? Nulo(_estadoHacienda) : null,
            IncluirAnulados = _incluirAnulados,
            Pagina = _pagina,
            TamanoPagina = _tamano
        };

        switch (_tab)
        {
            case 0:
                {
                    var r = await Respuestas.DatoAsync(await Api.Preventas(f), "consultar las preventas");
                    _comunes = r?.Registros ?? new();
                    _fiscales = new();
                    _total = r?.TotalRegistros ?? 0;
                    break;
                }
            case 1:
                {
                    var r = await Respuestas.DatoAsync(await Api.Facturas(f), "consultar las facturas");
                    _fiscales = r?.Registros ?? new();
                    _comunes = new();
                    _total = r?.TotalRegistros ?? 0;
                    break;
                }
            case 2:
                {
                    var r = await Respuestas.DatoAsync(await Api.NotasCredito(f), "consultar las notas de crédito");
                    _fiscales = r?.Registros ?? new();
                    _comunes = new();
                    _total = r?.TotalRegistros ?? 0;
                    break;
                }
            case 3:
                {
                    var r = await Respuestas.DatoAsync(await Api.Consignaciones(f), "consultar las consignaciones");
                    _comunes = r?.Registros ?? new();
                    _fiscales = new();
                    _total = r?.TotalRegistros ?? 0;
                    break;
                }
        }

        _cargando = false;
    }

    private async Task VerDetalle(DocumentoBandeja d)
    {
        _detFactura = null;
        _detNC = null;
        _detComun = null;

        if (_tab == 1)
        {
            _detFactura = await Respuestas.DatoAsync(await Api.DetalleFactura(d.Id), "consultar el detalle de la factura");
        }
        else if (_tab == 2)
        {
            _detNC = await Respuestas.DatoAsync(await Api.DetalleNotaCredito(d.Id), "consultar el detalle de la nota de crédito");
        }
        else
        {
            _detComun = d;
        }

        await _detalle.ShowAsync();
    }

    private void Devolucion(DocumentoBandeja d) => DevolucionPorVentaId(d.Id);

    // Llevamos el Id exacto de la venta (no el consecutivo, que se repite entre
    // series/tipos) para que la pantalla de devoluciones cargue el documento correcto.
    private void DevolucionPorVentaId(long idVenta)
        => Navegacion.NavigateTo($"/sales/repayment?ventaId={idVenta}");

    private async Task AccionFiscal(string clave)
    {
        _claveFiscal = clave;
        _contenidoFiscal = null;
        await _modalFiscal.ShowAsync();
    }

    private async Task VerXml()
        => _contenidoFiscal = await Respuestas.DatoAsync(await Fiscal.XmlFirmado(_claveFiscal!), "obtener el XML firmado");

    private async Task VerRespuesta()
        => _contenidoFiscal = await Respuestas.DatoAsync(await Fiscal.RespuestaHacienda(_claveFiscal!), "obtener la respuesta de Hacienda");

    private async Task ConsultarEstadoHacienda()
    {
        if (await Respuestas.CorrectaAsync(await Fiscal.ConsultarEstado(_claveFiscal!), "consultar el estado en Hacienda"))
        {
            Dialogos.Exito("Estado consultado. Cierre y vuelva a buscar para ver el cambio.");
        }
    }

    private async Task ReintentarEmision()
    {
        if (await Respuestas.CorrectaAsync(await Fiscal.Reintentar(_claveFiscal!), "reintentar la emisión fiscal"))
        {
            Dialogos.Exito("Emisión reencolada.");
            await _modalFiscal.HideAsync();
            await Cargar();
        }
    }

    private static DateTime Fecha(ChangeEventArgs e, DateTime actual)
        => DateTime.TryParse(e.Value?.ToString(), out var v) ? v : actual;

    private static string? Nulo(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string Truncar(string s) => s.Length <= 60 ? s : s[..60] + "…";

    private static string ClaseEstado(string estado)
    {
        var e = estado.ToLowerInvariant();
        if (e.Contains("acept") || e.Contains("recib")) return "text-bg-success";
        if (e.Contains("error") || e.Contains("rechaz")) return "text-bg-danger";
        return "text-bg-secondary";
    }
}
