using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Views.Ventas;

public partial class CuentasPorCobrar
{
    private const string Titulo = "Abono Cobrar";
    private const string CodigoEfectivo = "EFE";

    private HxModal _modalDesbloqueo = default!;

    // --- Caja / desbloqueo ---
    private bool _desbloqueado, _validando;
    private string _clave = string.Empty;
    private User? _usuario;
    private long _numApertura;
    private List<Moneda> _monedas = new();

    // --- Cliente / preventas ---
    private string _cedula = string.Empty;
    private bool _buscando;
    private long _codCliente;
    private string? _cliente;
    private List<PreventaResumenWebDTO> _preventas = new();
    private readonly HashSet<long> _seleccion = new();

    // --- Pago ---
    private List<FormasPagoDTO> _formasPago = new();
    private readonly Dictionary<string, decimal> _montos = new();
    private decimal _aCobrar, _entregado, _cambio;
    private bool _procesando;

    // --- Resultado ---
    private readonly List<FilaResultado> _resultados = new();

    // --- Modo "Facturas de crédito" (SANEAMIENTO Fase 8) ---
    private string _modo = "preventas";
    private CreditoClienteWebDTO? _credito;
    private List<FacturaCreditoWebDTO> _facturasCredito = new();
    private readonly HashSet<long> _selCredito = new();
    private decimal _aCobrarCredito, _entregadoCredito;

    private bool TodasSeleccionadas => _preventas.Count > 0 && _preventas.All(p => _seleccion.Contains(p.Id));

    private void CambiarModo(string modo)
    {
        _modo = modo;
        _resultados.Clear();
    }

    private void AlternarCredito(long id)
    {
        if (!_selCredito.Add(id)) _selCredito.Remove(id);
        RecalcularCredito();
    }

    private void RecalcularCredito()
    {
        _aCobrarCredito = _facturasCredito.Where(f => _selCredito.Contains(f.IdVenta)).Sum(f => f.SaldoActual);
        _entregadoCredito = _montos.Values.Sum();
    }

    private async Task CobrarCredito()
    {
        if (_procesando || _selCredito.Count == 0 || _entregadoCredito <= 0) return;

        var formas = _formasPago
            .Where(f => f.Codigo is not null && _montos.TryGetValue(f.Codigo, out var m) && m > 0)
            .Select(f => new CobroCreditoFormaPagoWebDTO { CodigoFormaPago = f.Codigo!, MontoRecibido = _montos[f.Codigo!] })
            .ToList();
        if (formas.Count == 0) { await Dialogos.ErrorAsync("Indique el monto de al menos una forma de pago."); return; }

        if (!await Dialogos.ConfirmarAsync(
                $"Cobrar {Formato.Importe(_entregadoCredito)} y aplicarlo a {_selCredito.Count} factura(s) de crédito?", "Cobrar crédito"))
            return;

        _procesando = true;
        try
        {
            var comando = new CobroCreditoComandoWebDTO
            {
                ClaveIdempotencia = Guid.NewGuid().ToString("N"),
                IdCliente = _codCliente,
                IdApertura = _numApertura,
                IdSucursal = Sesion.IdSucursal,
                CedulaCajero = _usuario?.Nombre is { } ? null : null,
                Usuario = Sesion.Usuario ?? _usuario?.Nombre ?? "",
                Facturas = _selCredito.Select(id => new CobroCreditoFacturaWebDTO { IdVenta = id }).ToList(),
                FormasPago = formas,
                PermitirParcial = true,
            };

            var r = await Credito.Cobrar(comando);
            var res = await Respuestas.DatoAsync(r, "registrar el cobro de crédito");
            if (!r.EsCorrecta || res is null) return;

            Dialogos.Exito($"Cobro registrado. Recibo N.º {res.NumeroRecibo}. Aplicado {Formato.Importe(res.TotalAplicado)}" +
                           (res.Vuelto > 0 ? $", vuelto {Formato.Importe(res.Vuelto)}" : "") + ".");

            if (await Dialogos.ConfirmarAsync("¿Imprimir el recibo?", "Impresión"))
                await JS.InvokeVoidAsync("open", $"/documentos/recibo-cobro/{res.IdCobro}/pdf", "_blank");

            _selCredito.Clear();
            foreach (var k in _montos.Keys.ToList()) _montos[k] = 0;
            await Buscar();
        }
        finally { _procesando = false; }
    }

    private sealed class FilaResultado
    {
        public long Id { get; init; }
        public string? NumFactura { get; init; }
        public bool Facturada { get; set; }
        public string? EstadoHacienda { get; set; }
        public string? Slug { get; init; }
        public string? Error { get; set; }
    }

    // ------------------------------------------------------------------ Desbloqueo

    private async Task DesbloquearConEnter(KeyboardEventArgs e) { if (e.Key == "Enter") await Desbloquear(); }

    private async Task Desbloquear()
    {
        if (string.IsNullOrWhiteSpace(_clave)) { await Dialogos.ErrorAsync("Ingrese la clave interna."); return; }
        _validando = true;
        var r = await Caja.ValidarClaveInterna(_clave);
        _validando = false;
        if (!r.EsCorrecta || r.Responses is null)
        {
            _clave = string.Empty;
            await Dialogos.ErrorAsync(r.Excepcion ?? "Contraseña incorrecta.");
            return;
        }

        var usuario = r.Responses;
        _clave = string.Empty;

        var cajeros = await Respuestas.DatoAsync(await Caja.CajerosConCajaAbierta(), "consultar la caja abierta");
        var cajero = cajeros?.FirstOrDefault(c => string.Equals(c.Nombre, usuario.Nombre, StringComparison.OrdinalIgnoreCase));
        if (cajero is null)
        {
            await Dialogos.ErrorAsync("Este usuario no tiene ninguna caja abierta.", "No se pudo desbloquear");
            return;
        }

        _usuario = cajero;
        _numApertura = cajero.IdApertura;
        _monedas = (await Respuestas.DatoAsync(await Compras.Monedas(), "consultar las monedas"))?.ToList() ?? new();

        _desbloqueado = true;
        await _modalDesbloqueo.HideAsync();
    }

    // ------------------------------------------------------------------ Buscar

    private async Task BuscarConEnter(KeyboardEventArgs e) { if (e.Key == "Enter") await Buscar(); }

    private async Task Buscar()
    {
        if (string.IsNullOrWhiteSpace(_cedula)) { await Dialogos.ErrorAsync("Indique la cédula del cliente."); return; }
        _buscando = true;
        _preventas = new();
        _seleccion.Clear();
        _resultados.Clear();
        _cliente = null;
        _credito = null;
        _facturasCredito = new();
        _selCredito.Clear();

        var codigo = await Respuestas.DatoAsync(await Api.CodigoClientePorCedula(_cedula.Trim()), "buscar el código del cliente");
        if (codigo == 0)
        {
            _buscando = false;
            await Dialogos.ErrorAsync($"No existe ningún cliente con la cédula {_cedula}.");
            return;
        }
        _codCliente = codigo;

        if (_modo == "preventas")
        {
            var lista = await Respuestas.DatoAsync(await Preventas.PreventasPendientes(_codCliente), "consultar las preventas pendientes");
            _preventas = (lista ?? new List<PreventaResumenWebDTO>()).ToList();
            _cliente = _preventas.FirstOrDefault()?.Cliente ?? $"Cliente {_codCliente}";
        }
        else
        {
            _credito = await Respuestas.DatoAsync(await Credito.Credito(_codCliente), "consultar el crédito del cliente");
            var facturas = await Respuestas.DatoAsync(await Credito.Facturas(_codCliente), "consultar las facturas de crédito");
            _facturasCredito = (facturas ?? new List<FacturaCreditoWebDTO>()).ToList();
            _cliente = _credito?.Nombre ?? $"Cliente {_codCliente}";
        }

        _formasPago = (await Respuestas.DatoAsync(await Api.FormasPago(_codCliente), "consultar las formas de pago"))?.ToList() ?? new();
        _montos.Clear();
        foreach (var f in _formasPago) if (f.Codigo is not null) _montos[f.Codigo] = 0;

        _buscando = false;
        Recalcular();
        RecalcularCredito();
    }

    // ------------------------------------------------------------------ Selección

    private void Alternar(long id)
    {
        if (!_seleccion.Add(id)) _seleccion.Remove(id);
        Recalcular();
    }

    private void AlternarTodas()
    {
        if (TodasSeleccionadas) _seleccion.Clear();
        else foreach (var p in _preventas) _seleccion.Add(p.Id);
        Recalcular();
    }

    private void Recalcular()
    {
        _aCobrar = CalculoDocumento.Redondear(_preventas.Where(p => _seleccion.Contains(p.Id)).Sum(p => (decimal)p.Total));
        _entregado = _montos.Values.Sum();
        var efectivo = _montos.TryGetValue(CodigoEfectivo, out var e) ? e : 0m;
        _cambio = efectivo > 0 && _entregado > _aCobrar ? _entregado - _aCobrar : 0m;
    }

    // ------------------------------------------------------------------ Cobrar

    private async Task CobrarYFacturar()
    {
        if (_procesando || _aCobrar <= 0 || _entregado < _aCobrar) return;

        var seleccionadas = _preventas.Where(p => _seleccion.Contains(p.Id)).OrderBy(p => p.Fecha).ToList();
        if (seleccionadas.Count == 0) return;

        if (!await Dialogos.ConfirmarAsync(
                $"Se cobrarán {seleccionadas.Count} preventa(s) por {Formato.Importe(_aCobrar)} y se facturarán. ¿Continuar?",
                "Cobrar y facturar"))
            return;

        _procesando = true;
        _resultados.Clear();

        // Reparto en cascada: cada forma de pago cubre las preventas por orden
        // de fecha hasta agotarse; el vuelto del efectivo se asigna a la última.
        var restantePorForma = _formasPago
            .Where(f => f.Codigo is not null)
            .ToDictionary(f => f.Codigo!, f => _montos.TryGetValue(f.Codigo!, out var m) ? m : 0m);

        var dolar = _monedas.FirstOrDefault(m => (m.MonedaNombre ?? string.Empty).Contains("DOL", StringComparison.OrdinalIgnoreCase));

        for (var i = 0; i < seleccionadas.Count; i++)
        {
            var p = seleccionadas[i];
            var esUltima = i == seleccionadas.Count - 1;
            var totalDoc = CalculoDocumento.Redondear((decimal)p.Total);
            var fila = new FilaResultado { Id = p.Id, NumFactura = p.NumFactura, Slug = p.SlugImpresion };
            _resultados.Add(fila);

            var cobros = new List<CobroDocumentosDTO>();
            var cubierto = 0m;
            foreach (var f in _formasPago.Where(x => x.Codigo is not null))
            {
                var disponible = restantePorForma[f.Codigo!];
                if (disponible <= 0) continue;

                var falta = totalDoc - cubierto;
                var aplica = esUltima ? disponible : Math.Min(disponible, falta);
                if (aplica <= 0) continue;

                restantePorForma[f.Codigo!] -= aplica;
                cubierto += aplica;

                cobros.Add(new CobroDocumentosDTO
                {
                    Documento = double.TryParse(p.NumFactura, out var doc) ? doc : p.Id,
                    TipoDocumento = p.TipoFacturaDescripcion,
                    MontoPago = (double)aplica,
                    FormaPago = f.Codigo,
                    Usuario = Sesion.Usuario,
                    Nombre = _usuario?.Nombre,
                    CodMoneda = p.CodMoneda == 0 ? 1 : p.CodMoneda,
                    Nombremoneda = p.Moneda,
                    TipoCambio = p.CodMoneda == 2 ? (double)Formato.AImporte(dolar?.ValorCompra ?? 0) : 0,
                    Fecha = DateTime.Today.ToString("yyyy-MM-dd"),
                    Numapertura = _numApertura,
                    Vuelto = esUltima && f.Codigo == CodigoEfectivo ? (double)_cambio : 0,
                    IdDocumento = p.Id,
                    IdSucursal = Sesion.IdSucursal,
                    CodCliente = _codCliente,
                });

                if (!esUltima && cubierto >= totalDoc) break;
            }

            if (cobros.Count == 0)
            {
                fila.Error = "Sin monto asignado.";
                continue;
            }

            if (!await Respuestas.CorrectaAsync(await Api.Cobrar(cobros), "registrar el cobro"))
            {
                fila.Error = "No se registró el cobro.";
                continue;
            }

            var fac = await Api.FacturarPreventa(p.Id);
            if (!fac.EsCorrecta)
            {
                fila.Error = fac.Excepcion ?? "No se pudo facturar.";
                continue;
            }
            fila.Facturada = true;

            // Emisión a Hacienda: síncrona si la serie está habilitada para V4.4.
            if (p.EmisionV44Habilitada && p.CodigoFe is "01" or "04")
            {
                var emi = p.CodigoFe == "04"
                    ? await Preventas.EmitirTiquete(p.Id)
                    : await Preventas.EmitirFactura(p.Id);
                fila.EstadoHacienda = emi.EsCorrecta && emi.Responses is not null
                    ? (emi.Responses.EsValido ? (emi.Responses.Estado ?? "Enviado") : $"Error: {string.Join(" ", emi.Responses.Errores)}")
                    : (emi.Excepcion ?? "No se pudo emitir");
            }
            else
            {
                fila.EstadoHacienda = "Automático (worker)";
            }
        }

        _procesando = false;

        var ok = _resultados.Count(r => r.Facturada);
        if (ok > 0)
        {
            Dialogos.Exito($"{ok} preventa(s) cobrada(s) y facturada(s).");

            var imprimibles = _resultados.Where(r => r.Facturada && r.Slug is not null).ToList();
            if (imprimibles.Count > 0 && await Dialogos.ConfirmarAsync("¿Desea imprimir los documentos ahora?", "Impresión"))
            {
                foreach (var r in imprimibles)
                    await JS.InvokeVoidAsync("open", $"/documentos/{r.Slug}/{r.Id}/pdf", "_blank");
            }
        }

        await Buscar(); // refresca la lista (las cobradas ya no son preventa)
    }

    private void Limpiar()
    {
        _cedula = string.Empty;
        _codCliente = 0;
        _cliente = null;
        _preventas = new();
        _seleccion.Clear();
        _formasPago = new();
        _montos.Clear();
        _resultados.Clear();
        _aCobrar = _entregado = _cambio = 0;
    }
}
