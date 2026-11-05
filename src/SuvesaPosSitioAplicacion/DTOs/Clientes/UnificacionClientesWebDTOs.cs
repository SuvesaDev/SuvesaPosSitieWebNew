namespace SuvesaPosSitioAplicacion.DTOs.Clientes;

/// <summary>Una tabla que guarda algo del cliente y cuántas filas tiene (espejo del API).</summary>
public sealed class UnificacionFilaWebDTO
{
    public string Tabla { get; set; } = string.Empty;
    /// <summary>Historial transaccional; lo demás es la ficha del cliente.</summary>
    public bool EsHistorial { get; set; }
    public int Filas { get; set; }
}

public sealed class VistaUnificacionWebDTO
{
    public long IdOrigen { get; set; }
    public string? NombreOrigen { get; set; }
    public long IdDestino { get; set; }
    public string? NombreDestino { get; set; }
    public List<UnificacionFilaWebDTO> Filas { get; set; } = new();
    public int TotalFilas { get; set; }
    public long? IdBitacora { get; set; }
}

public sealed class EstadoEliminacionClienteWebDTO
{
    public long IdCliente { get; set; }
    public bool PuedeEliminar { get; set; }
    public bool Eliminado { get; set; }
    public string? Mensaje { get; set; }
    public List<UnificacionFilaWebDTO> Historial { get; set; } = new();
}

public sealed class UnificarClientesWebComando
{
    public long IdOrigen { get; set; }
    public long IdDestino { get; set; }
}
