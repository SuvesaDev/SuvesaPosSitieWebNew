using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Views.Ventas;

public partial class TramiteCobro
{
    private const string Titulo = "Trámite de cobro";

    private string _tab = "nueva";

    // --- Nueva boleta ---
    private string _textoCliente = "";
    private bool _buscando;
    private List<FiltranClienteDTO> _clientesEncontrados = new();
    private FiltranClienteDTO? _cliente;

    private DateTime _fechaEntrega = DateTime.Today;
    private string _entrega = "";
    private string _recibe = "";
    private string _observaciones = "";

    private bool _cargandoFacturas;
    private List<FacturaTramiteCobroWebDTO> _facturas = new();
    private readonly HashSet<long> _seleccion = new();
    private readonly Dictionary<long, DateTime> _fechasPago = new();
    private bool _guardando, _enviando;
    private TramiteCobroWebDTO? _creada;

    private decimal _totalSeleccion => _facturas.Where(f => _seleccion.Contains(f.IdVenta)).Sum(f => f.SaldoActual);

    // --- Consultar ---
    private bool _consultando;
    private List<TramiteCobroWebDTO> _boletas = new();
    private long _consultaConsecutivo;
    private DateTime? _consultaDesde = DateTime.Today.AddMonths(-1);
    private DateTime? _consultaHasta = DateTime.Today;
    private bool _consultaIncluirAnuladas;

    private async Task CambiarAConsultar()
    {
        _tab = "consultar";
        if (_boletas.Count == 0) await Consultar();
    }

    private async Task BuscarConEnter(KeyboardEventArgs e) { if (e.Key == "Enter") await Buscar(); }

    private async Task Buscar()
    {
        if (string.IsNullOrWhiteSpace(_textoCliente)) return;
        _buscando = true;
        try
        {
            _clientesEncontrados = (await Respuestas.DatoAsync(await Clientes.Buscar(_textoCliente.Trim()), "buscar el cliente"))?.Take(8).ToList() ?? new();
        }
        finally { _buscando = false; }
    }

    private async Task ElegirCliente(FiltranClienteDTO c)
    {
        _cliente = c;
        _textoCliente = c.Cedula ?? c.Nombre ?? "";
        _clientesEncontrados.Clear();
        _creada = null;
        _seleccion.Clear();
        _fechasPago.Clear();

        _cargandoFacturas = true;
        try
        {
            _facturas = (await Respuestas.DatoAsync(await Api.Candidatas(c.Identificacion), "consultar las facturas pendientes"))?.ToList() ?? new();
        }
        finally { _cargandoFacturas = false; }
    }

    private void Alternar(FacturaTramiteCobroWebDTO f)
    {
        if (!_seleccion.Add(f.IdVenta)) { _seleccion.Remove(f.IdVenta); return; }
        if (!_fechasPago.ContainsKey(f.IdVenta)) _fechasPago[f.IdVenta] = f.Vence ?? DateTime.Today;
    }

    private DateTime FechaPago(long idVenta) => _fechasPago.TryGetValue(idVenta, out var d) ? d : DateTime.Today;

    private void CambiarFechaPago(long idVenta, ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var d)) _fechasPago[idVenta] = d;
    }

    private async Task Guardar()
    {
        if (_cliente is null || _seleccion.Count == 0 || string.IsNullOrWhiteSpace(_recibe) || _guardando) return;
        if (!await Dialogos.ConfirmarAsync($"¿Registrar la entrega de {_seleccion.Count} factura(s) a {_recibe}?")) return;

        _guardando = true;
        try
        {
            var cmd = new CrearTramiteCobroWebDTO
            {
                IdCliente = _cliente.Identificacion,
                IdSucursal = Sesion.IdSucursal,
                FechaEntrega = _fechaEntrega,
                Entrega = string.IsNullOrWhiteSpace(_entrega) ? Sesion.Usuario : _entrega,
                Recibe = _recibe,
                Observaciones = _observaciones,
                Facturas = _seleccion.Select(id => new LineaTramiteCobroComandoWebDTO { IdVenta = id, FechaPagoComprometida = FechaPago(id) }).ToList(),
            };

            _creada = await Respuestas.DatoAsync(await Api.Crear(cmd), "crear la boleta de trámite de cobro");
            if (_creada is not null)
            {
                Dialogos.Exito($"Boleta N.º {_creada.Consecutivo ?? _creada.Id} registrada.");
                _seleccion.Clear();
                _fechasPago.Clear();
                _recibe = ""; _observaciones = "";
                _facturas = (await Respuestas.DatoAsync(await Api.Candidatas(_cliente.Identificacion), "consultar las facturas pendientes"))?.ToList() ?? new();
            }
        }
        finally { _guardando = false; }
    }

    private async Task Consultar()
    {
        _consultando = true;
        try
        {
            var r = await Api.Listar(
                consecutivo: _consultaConsecutivo > 0 ? _consultaConsecutivo : null,
                incluirAnuladas: _consultaIncluirAnuladas,
                desde: _consultaDesde, hasta: _consultaHasta?.AddDays(1).AddSeconds(-1),
                limite: 300);
            _boletas = (await Respuestas.DatoAsync(r, "consultar las boletas"))?.ToList() ?? new();
        }
        finally { _consultando = false; }
    }

    private async Task Anular(TramiteCobroWebDTO b)
    {
        if (!await Dialogos.ConfirmarPeligroAsync($"¿Anular la boleta N.º {b.Consecutivo ?? b.Id}?")) return;
        var r = await Respuestas.DatoAsync(await Api.Anular(b.Id, null), "anular la boleta");
        if (r is not null)
        {
            Dialogos.Exito("Boleta anulada.");
            var i = _boletas.FindIndex(x => x.Id == b.Id);
            if (i >= 0) _boletas[i] = r;
        }
    }

    private async Task Imprimir(long id)
        => await JS.InvokeVoidAsync("open", $"/documentos/boleta-tramite-cobro/{id}/pdf", "_blank");

    private async Task Enviar(long id)
    {
        _enviando = true;
        try
        {
            var res = await Respuestas.DatoAsync(await Api.EnviarCorreo(id, null), "enviar la boleta por correo");
            if (res is null) return;
            if (res.Enviado) Dialogos.Exito($"Boleta enviada a {res.Destino}.");
            else await Dialogos.ErrorAsync(res.Error ?? "No se pudo enviar el correo.", "Envío fallido");
        }
        finally { _enviando = false; }
    }
}
