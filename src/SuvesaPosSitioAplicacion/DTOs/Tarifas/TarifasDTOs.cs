namespace SuvesaPosSitioAplicacion.DTOs.Tarifas;

/// <summary>Una tarifa (lista de precios) del catálogo. Las tarifas 1–4 son Precio A–D.</summary>
public sealed class TarifaPrecioDTO
{
    public short IdTarifa { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public int Orden { get; set; }
    /// <summary>Tarifas A–D: se pueden renombrar o desactivar, no quitar.</summary>
    public bool EsSistema { get; set; }
    /// <summary>Clientes activos que la tienen asignada (solo lectura).</summary>
    public int Clientes { get; set; }
    /// <summary>Artículos con precio en esta tarifa (solo lectura).</summary>
    public int Articulos { get; set; }
}

/// <summary>Precio sin impuesto de un artículo en una tarifa nueva (id 5 o mayor), con su utilidad.</summary>
public sealed class PrecioTarifaArticuloDTO
{
    public short IdTarifa { get; set; }
    public string? Nombre { get; set; }
    public decimal Precio { get; set; }
    public decimal? Utilidad { get; set; }
}

public sealed class GuardarPreciosTarifaArticuloRequest
{
    public List<PrecioTarifaArticuloDTO> Precios { get; set; } = new();
}

/// <summary>Precio de un artículo en una tarifa, para que Facturación ponga el de la tarifa del cliente.</summary>
public sealed class PrecioArticuloEnTarifaDTO
{
    public long IdArticulo { get; set; }
    public decimal Precio { get; set; }
}
