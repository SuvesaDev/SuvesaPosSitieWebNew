using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Ventas;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.Views.Ventas;

public partial class NotaCreditoExterna
{
    private const string Titulo = "Nota de crédito externa";
    private const int Tamano = 20;
    /// <summary>Una factura con muchas líneas pesa poco; esto solo evita subir por error un archivo que no es un XML.</summary>
    private const long MaximoArchivo = 5 * 1024 * 1024;

    private List<EmisorFiscalDTO> _emisores = new();
    private int _idEmisor;

    private string? _xmlFactura, _xmlAcuse, _nombreFactura, _nombreAcuse;
    private bool _validando, _creando;
    private ValidacionNotaCreditoExternaWebDTO? _validacion;
    private BorradorNotaCreditoExterna _borrador = new();
    /// <summary>Una por nota que se está armando: reintentar el mismo envío no la duplica.</summary>
    private string _claveIdempotencia = Guid.NewGuid().ToString("N");

    private PaginaNotasCreditoExternasWebDTO _pagina = new();
    private string _estadoFiltro = "";
    private string? _textoFiltro;
    private int _paginaNum = 1;

    private bool PuedeCrear => Sesion.Puede(Titulo, AccionPantalla.Crear);
    private bool PuedeValidar => !_validando && _idEmisor > 0 && Sesion.TieneSucursal && !string.IsNullOrWhiteSpace(_xmlFactura) && !string.IsNullOrWhiteSpace(_xmlAcuse);
    private int TotalPaginas => Math.Max(1, (int)Math.Ceiling(_pagina.Total / (double)Tamano));

    private VentaCandidataNotaCreditoExternaWebDTO? VentaElegida
        => _borrador.IdVenta is { } id ? _validacion?.VentasCandidatas.FirstOrDefault(v => v.IdVenta == id) : null;
    private bool SoloFiscalForzado => ReglasNotaCreditoExterna.SoloFiscalForzado(VentaElegida);
    private decimal MontoEfectivo => _validacion is null ? 0m : ReglasNotaCreditoExterna.MontoEfectivo(_validacion, _borrador);
    private string? ErrorActual => ReglasNotaCreditoExterna.Error(_validacion, _borrador);

    /// <summary>Ventas candidatas del cliente elegido (o todas mientras no haya cliente).</summary>
    private List<VentaCandidataNotaCreditoExternaWebDTO> VentasDelCliente
        => (_validacion?.VentasCandidatas ?? new()).Where(v => _borrador.IdCliente <= 0 || v.IdCliente == _borrador.IdCliente).ToList();

    protected override async Task OnInitializedAsync()
    {
        _emisores = (await Respuestas.DatoAsync(await EmisoresApi.Obtener(), "consultar los emisores"))?.ToList() ?? new();
        _idEmisor = _emisores.FirstOrDefault()?.Id ?? 0;
        await CargarLista();
    }

    // ------------------------------------------------------------------ Archivos y validación

    private async Task SeleccionarFactura(InputFileChangeEventArgs e) => (_xmlFactura, _nombreFactura) = (await LeerArchivo(e.File), e.File.Name);

    private async Task SeleccionarAcuse(InputFileChangeEventArgs e) => (_xmlAcuse, _nombreAcuse) = (await LeerArchivo(e.File), e.File.Name);

    private async Task<string?> LeerArchivo(IBrowserFile archivo)
    {
        Invalidar();
        try
        {
            await using var flujo = archivo.OpenReadStream(MaximoArchivo);
            using var lector = new StreamReader(flujo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await lector.ReadToEndAsync();
        }
        catch (IOException)
        {
            await Dialogos.ErrorAsync($"No se pudo leer «{archivo.Name}»: el archivo es demasiado grande o no se puede abrir.", "Archivo no válido");
            return null;
        }
    }

    /// <summary>Cambiar archivos o emisor deja sin efecto lo validado: hay que validar de nuevo.</summary>
    private void Invalidar()
    {
        _validacion = null;
        _borrador = new BorradorNotaCreditoExterna();
    }

    private async Task Validar()
    {
        if (!PuedeValidar) return;
        _validando = true;
        try
        {
            var r = await Respuestas.DatoAsync(await Api.Validar(new ValidarNotaCreditoExternaWebComando
            {
                IdEmisor = _idEmisor, IdSucursal = Sesion.IdSucursal, XmlFactura = _xmlFactura, XmlAcuse = _xmlAcuse,
            }), "validar los archivos del comprobante");
            if (r is null) return;

            _validacion = r;
            _borrador = new BorradorNotaCreditoExterna
            {
                IdCliente = ReglasNotaCreditoExterna.ClienteSugerido(r),
                Monto = r.Disponible,
            };
            _claveIdempotencia = Guid.NewGuid().ToString("N");
        }
        finally { _validando = false; }
    }

    // ------------------------------------------------------------------ Elecciones

    private void SeleccionarCliente(long idCliente)
    {
        _borrador.IdCliente = idCliente;
        // Una venta de otro cliente ya no es válida.
        if (VentaElegida is { } v && v.IdCliente != idCliente) _borrador.IdVenta = null;
    }

    private void SeleccionarVenta(long? idVenta)
    {
        _borrador.IdVenta = idVenta;
        if (idVenta is { } id && _validacion?.VentasCandidatas.FirstOrDefault(v => v.IdVenta == id) is { } venta)
            _borrador.IdCliente = venta.IdCliente;
        if (!SoloFiscalForzado) return;
        _borrador.SoloFiscal = true;
    }

    private void TodoDisponible() => _borrador.Monto = _validacion?.Disponible;

    // ------------------------------------------------------------------ Crear

    private async Task Crear()
    {
        if (_validacion is null || _creando || ReglasNotaCreditoExterna.Error(_validacion, _borrador) is not null) return;

        var resumen = $"Se emitirá una nota de crédito por {MontoEfectivo:N2} contra el comprobante {_validacion.Consecutivo}.\n\n" +
                      ReglasNotaCreditoExterna.EfectoFinanciero(_validacion, _borrador) + "\n\nNo se puede deshacer desde aquí.";
        if (!await Dialogos.ConfirmarAsync(resumen, "Crear nota de crédito externa", "Crear nota")) return;

        _creando = true;
        try
        {
            var soloFiscal = _borrador.SoloFiscal || SoloFiscalForzado;
            var creada = await Respuestas.DatoAsync(await Api.Crear(new CrearNotaCreditoExternaWebComando
            {
                ClaveIdempotencia = _claveIdempotencia,
                IdEmisor = _idEmisor,
                IdSucursal = Sesion.IdSucursal,
                XmlFactura = _xmlFactura,
                XmlAcuse = _xmlAcuse,
                IdCliente = _borrador.IdCliente,
                IdVenta = _borrador.IdVenta,
                Monto = _borrador.Monto is { } m && m < _validacion.Disponible ? m : null,
                Razon = _borrador.Razon?.Trim(),
                SoloFiscal = soloFiscal,
                MotivoSoloFiscal = _borrador.SoloFiscal ? _borrador.MotivoSoloFiscal?.Trim() : null,
            }), "crear la nota de crédito externa");
            if (creada is null) return;

            Dialogos.Exito($"Nota de crédito externa n.º {creada.Id} creada. Queda pendiente de numeración y envío a Hacienda.");
            Limpiar();
            await CargarLista();
        }
        finally { _creando = false; }
    }

    private void Limpiar()
    {
        _xmlFactura = _xmlAcuse = _nombreFactura = _nombreAcuse = null;
        _validacion = null;
        _borrador = new BorradorNotaCreditoExterna();
        _claveIdempotencia = Guid.NewGuid().ToString("N");
    }

    // ------------------------------------------------------------------ Lista

    private async Task CargarLista()
    {
        var r = await Respuestas.DatoAsync(await Api.Listar(
            string.IsNullOrWhiteSpace(_estadoFiltro) ? null : _estadoFiltro, string.IsNullOrWhiteSpace(_textoFiltro) ? null : _textoFiltro, _paginaNum, Tamano),
            "consultar las notas de crédito externas");
        if (r is not null) _pagina = r;
    }

    private async Task BuscarLista()
    {
        _paginaNum = 1;
        await CargarLista();
    }

    private async Task Anterior() { if (_paginaNum > 1) { _paginaNum--; await CargarLista(); } }
    private async Task Siguiente() { if (_paginaNum < TotalPaginas) { _paginaNum++; await CargarLista(); } }

    // ------------------------------------------------------------------ Presentación

    /// <summary>Número del comprobante referenciado: los diez últimos dígitos del consecutivo, que van en la posición 31 a 41 de la clave.</summary>
    private static string ComprobanteReferenciado(NotaCreditoExternaWebDTO n)
        => n.ClaveOrigen.Length == 50 && long.TryParse(n.ClaveOrigen[31..41], out var numero) ? $"n.º {numero}" : "—";

    private static string Recortar(string? texto, int max)
        => string.IsNullOrEmpty(texto) ? string.Empty : texto.Length <= max ? texto : texto[..max] + "…";

    private static string Color(string estado) => estado switch
    {
        "Aceptado" => "success",
        "Rechazado" or "ErrorTecnico" => "danger",
        "RecibidoPorHacienda" => "info",
        _ => "warning",
    };

    private static string Humano(string estado) => estado switch
    {
        "" => "Pendiente",
        "PendienteGeneracion" or "Generando" or "PendienteFirma" or "PendienteEnvio" or "ReintentoPendiente" => "Pendiente",
        "RecibidoPorHacienda" => "Recibido por Hacienda",
        "ErrorTecnico" => "Error técnico",
        _ => estado,
    };
}
