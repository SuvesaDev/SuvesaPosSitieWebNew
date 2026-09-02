using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components.Forms;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class EmisoresFiscal
{
    // ---- Alta del emisor: antes era la pantalla /parameters/company ("Empresas").
    // Ahora vive aca como un modal con pestanas para no tener dos entradas de menu
    // que confunden. El API de alta sigue siendo el mismo (IEmpresas.Crear).

    private HxModal _modalCrear = default!;
    private int _pestana;
    private bool _catalogosCargados;

    private List<TipoIdentificacionDTO> _tiposIdentificacion = new();
    private List<ProvinciaDTO> _provincias = new();
    private List<CantonDTO> _cantones = new();
    private List<DistritoDTO> _distritos = new();
    private List<EntidadesBancariasDTO> _bancos = new();
    private List<Moneda> _monedas = new();

    private int _tipoIdentificacion;
    private string _identificacion = string.Empty;
    private string _nombre = string.Empty;
    private string _correo = string.Empty;
    private string _telefono = string.Empty;
    private string _sucursalNueva = string.Empty;

    private int _idProvincia;
    private int _idCanton;
    private int _idDistrito;
    private string _otrasSenas = string.Empty;

    private string _usuarioHacienda = string.Empty;
    private string _claveHacienda = string.Empty;
    private string? _certificadoBase64;
    private string _nombreCertificado = string.Empty;
    private string _contrasenaCertificado = string.Empty;
    private string _numeroResolucion = string.Empty;
    private DateTime? _fechaResolucion;
    private DateTime? _venceCertificado;

    private bool _buscandoActividades;
    private List<ActividadesEmpresaDTO> _actividades = new();

    private int _idBancoNuevo;
    private int _idMonedaNueva;
    private string _numeroCuentaNueva = string.Empty;
    private List<CuentaBancariaDTO> _cuentas = new();

    private bool _guardando;

    private async Task Crear()
    {
        _pestana = 0;
        _tipoIdentificacion = 0;
        _identificacion = string.Empty;
        _nombre = string.Empty;
        _correo = string.Empty;
        _telefono = string.Empty;
        _sucursalNueva = string.Empty;
        _idProvincia = 0;
        _idCanton = 0;
        _idDistrito = 0;
        _cantones = new();
        _distritos = new();
        _otrasSenas = string.Empty;
        _usuarioHacienda = string.Empty;
        _claveHacienda = string.Empty;
        _certificadoBase64 = null;
        _nombreCertificado = string.Empty;
        _contrasenaCertificado = string.Empty;
        _numeroResolucion = string.Empty;
        _fechaResolucion = null;
        _venceCertificado = null;
        _actividades = new();
        _cuentas = new();
        _idBancoNuevo = 0;
        _idMonedaNueva = 0;
        _numeroCuentaNueva = string.Empty;
        _guardando = false;

        await CargarCatalogos();
        await _modalCrear.ShowAsync();
    }

    private async Task CargarCatalogos()
    {
        if (_catalogosCargados)
        {
            return;
        }

        _tiposIdentificacion = (await Respuestas.DatoAsync(await Empresas.TiposIdentificacion(), "consultar los tipos de documento"))
                               ?.ToList() ?? new List<TipoIdentificacionDTO>();
        _provincias = (await Respuestas.DatoAsync(await Empresas.Provincias(), "consultar las provincias"))
                      ?.ToList() ?? new List<ProvinciaDTO>();
        _bancos = (await Respuestas.DatoAsync(await Empresas.Bancos(), "consultar los bancos"))
                  ?.ToList() ?? new List<EntidadesBancariasDTO>();
        _monedas = (await Respuestas.DatoAsync(await Empresas.Monedas(), "consultar las monedas"))
                   ?.ToList() ?? new List<Moneda>();

        _catalogosCargados = true;
    }

    private async Task AlCambiarProvincia()
    {
        _idCanton = 0;
        _idDistrito = 0;
        _distritos = new List<DistritoDTO>();
        _cantones = _idProvincia > 0
            ? (await Respuestas.DatoAsync(await Empresas.Cantones(_idProvincia), "consultar los cantones"))
              ?.ToList() ?? new List<CantonDTO>()
            : new List<CantonDTO>();
    }

    private async Task AlCambiarCanton()
    {
        _idDistrito = 0;
        _distritos = _idCanton > 0
            ? (await Respuestas.DatoAsync(await Empresas.Distritos(_idCanton), "consultar los distritos"))
              ?.ToList() ?? new List<DistritoDTO>()
            : new List<DistritoDTO>();
    }

    private async Task CargarCertificadoCrear(InputFileChangeEventArgs evento)
    {
        var archivo = evento.File;
        _nombreCertificado = archivo.Name;

        try
        {
            await using var flujo = archivo.OpenReadStream(TamanoMaximoCertificado);
            using var memoria = new MemoryStream();
            await flujo.CopyToAsync(memoria);
            _certificadoBase64 = Convert.ToBase64String(memoria.ToArray());
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            _nombreCertificado = string.Empty;
            _certificadoBase64 = null;
            await Dialogos.ErrorAsync("No se pudo leer el archivo del certificado.", "Certificado");
        }
    }

    private async Task BuscarActividadesHacienda()
    {
        if (string.IsNullOrWhiteSpace(_identificacion))
        {
            Dialogos.ErrorBreve("Indique la identificacion antes de consultar Hacienda.");
            return;
        }

        _buscandoActividades = true;
        var r = await Respuestas.DatoAsync(await Empresas.ActividadesHacienda(_identificacion), "consultar las actividades en Hacienda");
        _buscandoActividades = false;

        if (r is null)
        {
            return;
        }

        foreach (var a in r)
        {
            if (_actividades.Any(existente => existente.Codigo == a.Codigo))
            {
                continue;
            }

            _actividades.Add(a);
        }
    }

    private void AgregarCuenta()
    {
        if (_idBancoNuevo <= 0 || _idMonedaNueva <= 0 || string.IsNullOrWhiteSpace(_numeroCuentaNueva))
        {
            Dialogos.ErrorBreve("Seleccione el banco, la moneda e indique el numero de cuenta.");
            return;
        }

        var banco = _bancos.FirstOrDefault(b => b.Id == _idBancoNuevo);
        var moneda = _monedas.FirstOrDefault(m => m.CodMoneda == _idMonedaNueva);

        _cuentas.Add(new CuentaBancariaDTO
        {
            IdBanco = _idBancoNuevo,
            Banco = banco?.Banco,
            IdMoneda = _idMonedaNueva,
            Moneda = moneda?.MonedaNombre,
            Numero = _numeroCuentaNueva
        });

        _numeroCuentaNueva = string.Empty;
        _idBancoNuevo = 0;
        _idMonedaNueva = 0;
    }

    private async Task GuardarCrear()
    {
        if (_guardando)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_identificacion) || string.IsNullOrWhiteSpace(_nombre))
        {
            _pestana = 0;
            await Dialogos.ErrorAsync("Indique la identificacion y el nombre del emisor.");
            return;
        }

        _guardando = true;

        var dto = new EmpresaDTO
        {
            TipoIdentificacion = _tipoIdentificacion,
            Identificacion = _identificacion,
            Nombre = _nombre,
            Correo = _correo,
            Telefono = _telefono,
            Sucursal = _sucursalNueva,
            Distrito = _idDistrito,
            OtrasSeñas = _otrasSenas,
            Usuario = _usuarioHacienda,
            Clave = _claveHacienda,
            Certificado = _certificadoBase64,
            NumeroResolucion = _numeroResolucion,
            FechaResolucion = _fechaResolucion?.ToString("yyyy-MM-dd"),
            VenceCertificado = _venceCertificado,
            ContrasenaCertificado = _contrasenaCertificado,
            Actividades = _actividades.Select(a => new ActividadesEmisorDTO
            {
                Codigo = a.Codigo,
                Descripcion = a.Descripcion,
                Activo = a.Estado,
                Principal = a.Tipo,
                IdEmisor = 0
            }).ToList(),
            CuentaBancarias = _cuentas.ToList()
        };

        var ok = await Respuestas.CorrectaAsync(await Empresas.Crear(dto), "crear el emisor");

        _guardando = false;

        if (ok)
        {
            Dialogos.Exito($"Emisor {_nombre} creado.");
            await _modalCrear.HideAsync();
            await Recargar();
        }
    }
}
