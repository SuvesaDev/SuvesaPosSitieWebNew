namespace SuvesaPosSitioAplicacion.DTOs.Comercial;

// Espejo a mano de api/comercial/clasificacion (DTOs/ClasificacionComercialDTOs.cs del API).

// ---------------- Clientes ----------------

public sealed class SucursalClasificacionDTO
{
    public long IdClienteSucursal { get; set; }
    public string Sucursal { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public int? IdCanalPropio { get; set; }
    public int? IdCanalEfectivo { get; set; }
}

public sealed class ClienteClasificacionDTO
{
    public long IdCliente { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int? IdCanal { get; set; }
    public List<SucursalClasificacionDTO> Sucursales { get; set; } = new();
    public bool Pendiente { get; set; }
}

public sealed class FiltroClasificacionClientesDTO
{
    public string? Texto { get; set; }
    public bool SoloPendientes { get; set; }
    public int? IdCanal { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 50;
}

public sealed class PaginaClientesClasificacionDTO
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public List<ClienteClasificacionDTO> Items { get; set; } = new();
}

public sealed class AsignarCanalItemDTO
{
    public long IdCliente { get; set; }
    public long? IdClienteSucursal { get; set; }
}

public sealed class AsignarCanalRequest
{
    public List<AsignarCanalItemDTO> Items { get; set; } = new();
    public int? IdCanal { get; set; }
}

public sealed class ResultadoAsignacionDTO
{
    public int Actualizados { get; set; }
}

// ---------------- Artículos ----------------

public sealed class EspecieArticuloDTO
{
    public int IdEspecie { get; set; }
    public bool EsPrincipal { get; set; }
}

public sealed class ArticuloClasificacionDTO
{
    public long IdInventario { get; set; }
    public string CodArticulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? CodMarca { get; set; }
    public string? Marca { get; set; }
    public List<EspecieArticuloDTO> Especies { get; set; } = new();
    public List<int> Funciones { get; set; } = new();
    public decimal? MargenMinimoPct { get; set; }
}

public sealed class FiltroClasificacionArticulosDTO
{
    public string? Texto { get; set; }
    public int? CodMarca { get; set; }
    public int? IdFamilia { get; set; }
    public int? IdCategoria { get; set; }
    public bool SinEspecie { get; set; }
    public bool SinFuncion { get; set; }
    public bool SinMargen { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 50;
}

public sealed class PaginaArticulosClasificacionDTO
{
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public List<ArticuloClasificacionDTO> Items { get; set; } = new();
}

public sealed class OpcionFiltroDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class OpcionesFiltroArticulosDTO
{
    public List<OpcionFiltroDTO> Marcas { get; set; } = new();
    public List<OpcionFiltroDTO> Familias { get; set; } = new();
    public List<OpcionFiltroDTO> Categorias { get; set; } = new();
}

public sealed class GuardarClasificacionArticuloRequest
{
    public List<EspecieArticuloDTO> Especies { get; set; } = new();
    public List<int> Funciones { get; set; } = new();
    public decimal? MargenMinimoPct { get; set; }
}

public sealed class AsignarClasificacionMasivaRequest
{
    public List<long>? IdsInventario { get; set; }
    public FiltroClasificacionArticulosDTO? Filtro { get; set; }
    public List<int> AgregarEspecies { get; set; } = new();
    public List<int> AgregarFunciones { get; set; } = new();
    public decimal? MargenMinimoPct { get; set; }
}
