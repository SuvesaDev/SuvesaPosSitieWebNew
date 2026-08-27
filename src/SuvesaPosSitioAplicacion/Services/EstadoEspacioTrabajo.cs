using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="IEstadoEspacioTrabajo" />
public sealed class EstadoEspacioTrabajo : IEstadoEspacioTrabajo
{
    private const string TituloVenta = "Venta";

    private readonly IAlmacenEspacioTrabajo _almacen;
    private readonly ILogger<EstadoEspacioTrabajo> _log;

    private readonly List<PestanaTrabajo> _pestanas = new();
    private int _ultimaVenta;
    private bool _restaurado;

    public EstadoEspacioTrabajo(IAlmacenEspacioTrabajo almacen, ILogger<EstadoEspacioTrabajo> log)
    {
        _almacen = almacen;
        _log = log;
    }

    public IReadOnlyList<PestanaTrabajo> Pestanas => _pestanas;

    public PestanaTrabajo? Actual { get; private set; }

    public event Action? Cambio;

    public PestanaTrabajo Abrir(string titulo, string ruta)
    {
        var esVenta = titulo.Equals(TituloVenta, StringComparison.OrdinalIgnoreCase);

        if (!esVenta)
        {
            // Una sola pestana por pantalla: si ya esta abierta, se trae al frente.
            var abierta = _pestanas.FirstOrDefault(
                p => p.Ruta.Equals(ruta, StringComparison.OrdinalIgnoreCase));

            if (abierta is not null)
            {
                Actual = abierta;
                Notificar();
                return abierta;
            }
        }

        var numero = esVenta ? ++_ultimaVenta : 0;

        var pestana = new PestanaTrabajo
        {
            Id = Guid.NewGuid().ToString("N"),
            Titulo = esVenta ? $"{titulo} # {numero}" : titulo,
            Ruta = esVenta ? $"{ruta}/{numero}" : ruta,
            EsVenta = esVenta,
            Numero = numero
        };

        _pestanas.Add(pestana);
        Actual = pestana;
        Notificar();
        return pestana;
    }

    public void Seleccionar(string id)
    {
        var pestana = _pestanas.FirstOrDefault(p => p.Id == id);
        if (pestana is null || pestana.Id == Actual?.Id)
        {
            return;
        }

        Actual = pestana;
        Notificar();
    }

    public void Cerrar(string id)
    {
        var indice = _pestanas.FindIndex(p => p.Id == id);
        if (indice < 0)
        {
            return;
        }

        var eraActual = _pestanas[indice].Id == Actual?.Id;
        _pestanas.RemoveAt(indice);

        if (eraActual)
        {
            // Pasa a la anterior; si se cerro la primera, a la que quedo primera.
            var siguiente = indice == 0 ? 0 : indice - 1;
            Actual = _pestanas.Count > 0 ? _pestanas[siguiente] : null;
        }

        Notificar();
    }

    public void CerrarTodas()
    {
        _pestanas.Clear();
        Actual = null;
        _ultimaVenta = 0;
        Notificar();
    }

    public async Task RestaurarAsync()
    {
        if (_restaurado)
        {
            return;
        }

        _restaurado = true;

        try
        {
            var guardado = await _almacen.LeerAsync();

            if (guardado is null)
            {
                return;
            }

            _pestanas.Clear();
            _pestanas.AddRange(guardado.Pestanas);
            _ultimaVenta = guardado.UltimaVenta;
            Actual = _pestanas.FirstOrDefault(p => p.Id == guardado.IdActual);

            Cambio?.Invoke();
        }
        catch (Exception ex)
        {
            // Un estado corrupto no debe impedir trabajar: se empieza en blanco.
            _log.LogWarning(ex, "No se pudo restaurar el espacio de trabajo");
        }
    }

    private void Notificar()
    {
        Cambio?.Invoke();
        _ = GuardarAsync();
    }

    private async Task GuardarAsync()
    {
        try
        {
            await _almacen.GuardarAsync(new EstadoEspacioGuardado(
                _pestanas.ToList(),
                Actual?.Id,
                _ultimaVenta));
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "No se pudo guardar el espacio de trabajo");
        }
    }

}
