using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components.Forms;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.Views.Parametros;

public partial class EmisoresFiscal
{
    private const long TamanoMaximoLogo = 2 * 1024 * 1024;
    private EmisorLogoResumenDTO? _logo;

    private string UrlLogo => _publico is { Id: > 0 } && _logo?.TieneLogo == true
        ? $"/emisores/{_publico.Id}/logo?v={Uri.EscapeDataString(_logo.HashSha256 ?? string.Empty)}"
        : string.Empty;

    private async Task CargarEstadoLogo(int idEmisor)
        => _logo = await Respuestas.DatoAsync(await Api.LogoMetadata(idEmisor), "consultar el logo del emisor")
            ?? new EmisorLogoResumenDTO();

    private async Task CargarLogo(InputFileChangeEventArgs evento)
    {
        if (_publico is null) return;
        var archivo = evento.File;
        if (archivo.Size <= 0 || archivo.Size > TamanoMaximoLogo)
        {
            await Dialogos.ErrorAsync("El logo debe medir como máximo 2 MB.", "Logo del emisor");
            return;
        }

        if (archivo.ContentType is not ("image/png" or "image/jpeg"))
        {
            await Dialogos.ErrorAsync("Seleccione una imagen PNG o JPEG.", "Logo del emisor");
            return;
        }

        try
        {
            await using var flujo = archivo.OpenReadStream(TamanoMaximoLogo);
            using var memoria = new MemoryStream();
            await flujo.CopyToAsync(memoria);
            var logo = await Respuestas.DatoAsync(await Api.GuardarLogo(_publico.Id, new EmisorLogoActualizarDTO
            {
                NombreArchivo = archivo.Name,
                MimeType = archivo.ContentType,
                ContenidoBase64 = Convert.ToBase64String(memoria.ToArray())
            }), "guardar el logo del emisor");
            if (logo is null) return;

            _logo = logo;
            Dialogos.Exito("Logo del emisor actualizado.");
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            await Dialogos.ErrorAsync("No se pudo leer el archivo del logo.", "Logo del emisor");
        }
    }

    private async Task EliminarLogo()
    {
        if (_publico is null || !await Dialogos.ConfirmarPeligroAsync("¿Eliminar el logo oficial de este emisor? Los nuevos documentos se generarán sin logo.", "Logo del emisor")) return;
        if (await Respuestas.CorrectaAsync(await Api.EliminarLogo(_publico.Id), "eliminar el logo del emisor"))
        {
            _logo = new EmisorLogoResumenDTO();
            Dialogos.Exito("Logo del emisor eliminado.");
        }
    }

    // ---- Actividades económicas en el modal de datos públicos (edición) ----
    private string _actCodigo = string.Empty;
    private string _actDescripcion = string.Empty;
    private bool _buscandoActsEmisor;

    private void AgregarActividad()
    {
        if (_publico is null || string.IsNullOrWhiteSpace(_actCodigo))
        {
            return;
        }

        var codigo = _actCodigo.Trim();
        if (_publico.Actividades.Any(a => string.Equals(a.Codigo, codigo, StringComparison.OrdinalIgnoreCase)))
        {
            _actCodigo = _actDescripcion = string.Empty;
            return;
        }

        _publico.Actividades.Add(new ActividadEconomicaEmisorDTO
        {
            Codigo = codigo,
            Descripcion = _actDescripcion.Trim(),
            Activo = true,
            Principal = _publico.Actividades.Count == 0
        });
        _actCodigo = _actDescripcion = string.Empty;
    }

    private void MarcarPrincipal(ActividadEconomicaEmisorDTO actividad)
    {
        if (_publico is null)
        {
            return;
        }

        foreach (var a in _publico.Actividades)
        {
            a.Principal = ReferenceEquals(a, actividad);
        }
    }

    private async Task BuscarActividadesEmisorHacienda()
    {
        if (_publico is null || string.IsNullOrWhiteSpace(_publico.Identificacion))
        {
            Dialogos.ErrorBreve("Indique la identificación del emisor antes de consultar Hacienda.");
            return;
        }

        _buscandoActsEmisor = true;
        var r = await Respuestas.DatoAsync(await Empresas.ActividadesHacienda(_publico.Identificacion), "consultar las actividades en Hacienda");
        _buscandoActsEmisor = false;

        if (r is null)
        {
            return;
        }

        foreach (var a in r)
        {
            if (_publico.Actividades.Any(x => string.Equals(x.Codigo, a.Codigo, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            _publico.Actividades.Add(new ActividadEconomicaEmisorDTO
            {
                Codigo = a.Codigo,
                Descripcion = a.Descripcion,
                Activo = a.Estado,
                Principal = a.Tipo
            });
        }
    }

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
            if (_publico is { Id: > 0 })
            {
                await CargarEstadoLogo(_publico.Id);
            }
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
        if (_publico is { Id: > 0 })
        {
            await CargarEstadoLogo(_publico.Id);
        }
    }

    // ---- Ubicación del emisor en "Datos públicos" ----------------------------------------------------
    // El emisor guarda solo el id del distrito; de él salen provincia, cantón y distrito del XML. Se elige en
    // cascada (como en el alta) para no teclear un número que no corresponda al domicilio fiscal.
    private int _pubProvincia, _pubCanton;
    private List<CantonDTO> _pubCantones = new();
    private List<DistritoDTO> _pubDistritos = new();
    private string? _textoUbicacionHacienda;
    private List<ProvinciaFiscalDTO> _geoProvincias = new();
    private List<CantonFiscalDTO> _geoCantones = new();
    private List<DistritoFiscalDTO> _geoDistritos = new();
    private bool _geoCargada;

    /// <summary>Catálogo completo con padres y códigos de Hacienda; sirve para ubicar el distrito guardado y mostrar su código.</summary>
    private async Task<bool> AsegurarGeografiaAsync()
    {
        if (_geoCargada) return true;
        var provincias = await GeografiaFiscal.Provincias();
        var cantones = await GeografiaFiscal.Cantones(null);
        var distritos = await GeografiaFiscal.Distritos(null);
        if (!provincias.EsCorrecta || !cantones.EsCorrecta || !distritos.EsCorrecta
            || provincias.Responses is null || cantones.Responses is null || distritos.Responses is null)
            return false;
        _geoProvincias = provincias.Responses.ToList();
        _geoCantones = cantones.Responses.ToList();
        _geoDistritos = distritos.Responses.ToList();
        return _geoCargada = true;
    }

    /// <summary>Al abrir la edición, deja elegidos provincia y cantón del distrito guardado.</summary>
    private async Task CargarUbicacionPublicaAsync()
    {
        _pubProvincia = _pubCanton = 0;
        _pubCantones = new();
        _pubDistritos = new();
        _textoUbicacionHacienda = null;
        if (_publico is null || _publico.Distrito <= 0) return;

        if (!await AsegurarGeografiaAsync())
        {
            // El distrito guardado se conserva; solo no se puede mostrar a qué lugar corresponde.
            Dialogos.Advertencia("No se pudo consultar el catálogo de ubicaciones. El distrito actual se conserva; si lo cambia, vuelva a abrir esta ventana.");
            return;
        }

        var distrito = _geoDistritos.FirstOrDefault(d => d.IdDistrito == _publico.Distrito);
        var canton = distrito is null ? null : _geoCantones.FirstOrDefault(c => c.IdCanton == distrito.IdCanton);
        if (distrito is null || canton is null)
        {
            _publico.Distrito = 0;   // un id que ya no existe en el catálogo: hay que elegirlo de nuevo
            return;
        }

        _pubProvincia = canton.IdProvincia;
        _pubCanton = canton.IdCanton;
        _pubCantones = (await Respuestas.DatoAsync(await Empresas.Cantones(_pubProvincia), "consultar los cantones"))?.ToList() ?? new List<CantonDTO>();
        _pubDistritos = (await Respuestas.DatoAsync(await Empresas.Distritos(_pubCanton), "consultar los distritos"))?.ToList() ?? new List<DistritoDTO>();
        ActualizarTextoUbicacion();
    }

    private async Task AlCambiarProvinciaPublica()
    {
        _pubCanton = 0;
        if (_publico is not null) _publico.Distrito = 0;
        _pubDistritos = new();
        _pubCantones = _pubProvincia > 0
            ? (await Respuestas.DatoAsync(await Empresas.Cantones(_pubProvincia), "consultar los cantones"))?.ToList() ?? new List<CantonDTO>()
            : new List<CantonDTO>();
        ActualizarTextoUbicacion();
    }

    private async Task AlCambiarCantonPublico()
    {
        if (_publico is not null) _publico.Distrito = 0;
        _pubDistritos = _pubCanton > 0
            ? (await Respuestas.DatoAsync(await Empresas.Distritos(_pubCanton), "consultar los distritos"))?.ToList() ?? new List<DistritoDTO>()
            : new List<DistritoDTO>();
        ActualizarTextoUbicacion();
    }

    private Task AlCambiarDistritoPublico()
    {
        ActualizarTextoUbicacion();
        return Task.CompletedTask;
    }

    /// <summary>Lo que viajará a Hacienda en el XML: códigos de provincia, cantón y distrito, con sus nombres.</summary>
    private void ActualizarTextoUbicacion()
    {
        _textoUbicacionHacienda = null;
        if (!_geoCargada || _publico is null || _publico.Distrito <= 0) return;
        var distrito = _geoDistritos.FirstOrDefault(d => d.IdDistrito == _publico.Distrito);
        var canton = distrito is null ? null : _geoCantones.FirstOrDefault(c => c.IdCanton == distrito.IdCanton);
        var provincia = canton is null ? null : _geoProvincias.FirstOrDefault(p => p.IdProvincia == canton.IdProvincia);
        if (distrito is null || canton is null || provincia is null) return;
        _textoUbicacionHacienda = $"Se enviará a Hacienda como {provincia.CodigoFE}-{canton.CodigoFE}-{distrito.CodigoFE} "
            + $"({provincia.Descripcion} · {canton.Descripcion} · {distrito.Descripcion}).";
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
