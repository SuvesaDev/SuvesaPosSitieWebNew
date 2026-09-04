using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SuvesaPosSitioAplicacion.DTOs.Cobros;

namespace SuvesaPosSitioAplicacion.Views.Ventas.Componentes;

public partial class PanelRecibosFallidas
{
    /// <summary>null = pestañas propias; "recibos" / "fallidas" = una sola sección (integrado en Cobrar).</summary>
    [Parameter] public string? Vista { get; set; }

    private HxModal _modalDetalle = default!;

    private string _tab = "recibos";
    private bool _cargando;
    private bool _procesando;

    private string VistaEfectiva => Vista ?? _tab;

    // Filtro de recibos
    private DateTime? _desde = DateTime.Today.AddDays(-7);
    private DateTime? _hasta = DateTime.Today;
    private long _numeroRecibo;
    private long _numApertura;
    private int _estado;

    private List<ReciboCobroResumenWebDTO> _recibos = new();
    private List<OperacionFallidaWebDTO> _fallidas = new();
    private ReciboCobroResumenWebDTO? _detalle;

    protected override async Task OnInitializedAsync()
    {
        if (VistaEfectiva == "recibos") await CargarRecibos();
        // Las fallidas se cargan siempre: alimentan el badge del contador.
        await CargarFallidas();
    }

    private async Task Cambiar(string tab)
    {
        _tab = tab;
        if (tab == "recibos" && _recibos.Count == 0) await CargarRecibos();
        if (tab == "fallidas") await CargarFallidas();
    }

    private async Task CargarRecibos()
    {
        _cargando = true;
        try
        {
            var r = await Api.Recibos(
                desde: _desde,
                hasta: _hasta?.AddDays(1).AddSeconds(-1),
                numApertura: _numApertura > 0 ? _numApertura : null,
                estado: _estado > 0 ? _estado : null,
                numeroRecibo: _numeroRecibo > 0 ? _numeroRecibo : null,
                limite: 300);
            _recibos = (await Respuestas.DatoAsync(r, "consultar los recibos emitidos"))?.ToList() ?? new();
        }
        finally { _cargando = false; }
    }

    private async Task CargarFallidas()
    {
        _cargando = true;
        try
        {
            var r = await Api.OperacionesFallidas(200);
            _fallidas = (await Respuestas.DatoAsync(r, "consultar las operaciones fallidas"))?.ToList() ?? new();
        }
        finally { _cargando = false; }
    }

    private async Task VerDetalle(ReciboCobroResumenWebDTO r)
    {
        _detalle = r;
        await _modalDetalle.ShowAsync();
    }

    private async Task ImprimirRecibo(long idCobro)
        => await JS.InvokeVoidAsync("open", $"/documentos/recibo-cobro/{idCobro}/pdf", "_blank");

    // ---------------------------------------------------------- Acciones D10

    private async Task Reenviar(OperacionFallidaWebDTO f)
    {
        if (f.OrigenId is not { } idVenta) return;
        if (!await Dialogos.ConfirmarAsync(
                "Se corrigen los datos en el origen y se vuelve a firmar y enviar el comprobante a Hacienda. ¿Reenviar ahora?",
                "Reenviar comprobante"))
            return;

        _procesando = true;
        try
        {
            var esTiquete = f.TipoComprobante.Contains("Tiquete", StringComparison.OrdinalIgnoreCase);
            var r = esTiquete ? await Emision.EmitirTiquete(idVenta) : await Emision.EmitirFactura(idVenta);
            var res = await Respuestas.DatoAsync(r, "reenviar el comprobante");
            if (res is null) return;

            if (res.EsValido)
                Dialogos.Exito($"Comprobante reenviado. Estado: {res.Estado ?? "enviado"}.");
            else
                await Dialogos.ErrorAsync(
                    res.Errores.Count > 0 ? string.Join(" · ", res.Errores) : "Hacienda rechazó el reenvío.",
                    "No se pudo reenviar");

            await CargarFallidas();
        }
        finally { _procesando = false; }
    }

    private async Task AnularYRecrear(OperacionFallidaWebDTO f)
    {
        if (f.OrigenId is not { } idVenta) return;
        if (!await Dialogos.ConfirmarPeligroAsync(
                "Se genera una nota de crédito interna (SIN ruta fiscal): se reingresa inventario y se ajusta la cuenta por cobrar. " +
                "El cobro NO se borra. Luego deberá recrear la factura desde Facturación. ¿Continuar?",
                "NC interna y recrear"))
            return;

        _procesando = true;
        try
        {
            var r = await VentaOrquestada.DevolucionInterna(new DevolucionInternaComandoWebDTO
            {
                ClaveIdempotencia = Guid.NewGuid().ToString("N"),
                IdVentaOrigen = idVenta,
                Motivo = $"Comprobante rechazado ({f.Clave}) — NC interna para recrear la factura (D10).",
                AnularOrigen = true,
            });
            var res = await Respuestas.DatoAsync(r, "generar la nota de crédito interna");
            if (res is null) return;

            Dialogos.Exito(
                $"NC interna #{res.IdDevolucionInterna} por {res.Total:N2}." +
                (res.GeneroCreditoAFavor ? " Se generó crédito a favor del cliente." : "") +
                " Recree la factura desde Facturación.");
            await CargarFallidas();
        }
        finally { _procesando = false; }
    }
}
