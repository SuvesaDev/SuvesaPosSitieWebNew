using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Seguridad;

// -----------------------------------------------------------------------------
// Contrato del modulo de seguridad V2 (/seguridad/* del API).
//
// TEMPORAL: estos tipos estan escritos a mano porque los contratos NSwag del
// sitio provienen de un API distinto del codigo fuente actual y regenerarlos en
// local rompe proxies ajenos. Cuando se despliegue el API nuevo y se corra
// ./tools/actualizar-contratos.sh, NSwag generara equivalentes en
// DTOs/Generated y estos se borran (los proxies solo cambian el using).
//
// Se deserializan con System.Text.Json por reflexion (igual que el cliente
// NSwag), asi que basta con [JsonPropertyName] en camelCase.
// -----------------------------------------------------------------------------

public sealed class PerfilLoginDTO
{
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("esSuperAdministracion")] public bool EsSuperAdministracion { get; set; }
    [JsonPropertyName("gestionaUsuarios")] public bool GestionaUsuarios { get; set; }
    [JsonPropertyName("costaPets")] public bool CostaPets { get; set; }
    [JsonPropertyName("agenteCostaPets")] public bool AgenteCostaPets { get; set; }
    [JsonPropertyName("aceptaConsignacion")] public bool AceptaConsignacion { get; set; }
}

public sealed class PermisoLoginDTO
{
    [JsonPropertyName("moduloCodigo")] public string? ModuloCodigo { get; set; }
    [JsonPropertyName("moduloNombre")] public string? ModuloNombre { get; set; }
    [JsonPropertyName("funcionCodigo")] public string? FuncionCodigo { get; set; }
    [JsonPropertyName("funcionNombre")] public string? FuncionNombre { get; set; }
    [JsonPropertyName("acciones")] public List<string> Acciones { get; set; } = new();
}

public sealed class AccionCatalogoDTO
{
    [JsonPropertyName("idAccion")] public int? IdAccion { get; set; }
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("orden")] public int Orden { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;
}

public sealed class FuncionCatalogoDTO
{
    [JsonPropertyName("idFuncion")] public int? IdFuncion { get; set; }
    [JsonPropertyName("idModulo")] public int IdModulo { get; set; }
    [JsonPropertyName("idFuncionPadre")] public int? IdFuncionPadre { get; set; }
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("ruta")] public string? Ruta { get; set; }
    [JsonPropertyName("orden")] public int Orden { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;
    [JsonPropertyName("accionesDisponibles")] public List<string> AccionesDisponibles { get; set; } = new();
    [JsonPropertyName("hijas")] public List<FuncionCatalogoDTO> Hijas { get; set; } = new();
}

public sealed class ModuloCatalogoDTO
{
    [JsonPropertyName("idModulo")] public int? IdModulo { get; set; }
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("orden")] public int Orden { get; set; }
    [JsonPropertyName("icono")] public string? Icono { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;
    [JsonPropertyName("funciones")] public List<FuncionCatalogoDTO> Funciones { get; set; } = new();
}

public sealed class RolResumenDTO
{
    [JsonPropertyName("idRol")] public int IdRol { get; set; }
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; }
}

public sealed class RolFuncionPermisoDTO
{
    [JsonPropertyName("moduloCodigo")] public string? ModuloCodigo { get; set; }
    [JsonPropertyName("moduloNombre")] public string? ModuloNombre { get; set; }
    [JsonPropertyName("funcionCodigo")] public string? FuncionCodigo { get; set; }
    [JsonPropertyName("funcionNombre")] public string? FuncionNombre { get; set; }
    [JsonPropertyName("funcionPadreCodigo")] public string? FuncionPadreCodigo { get; set; }
    [JsonPropertyName("accionesDisponibles")] public List<string> AccionesDisponibles { get; set; } = new();
    [JsonPropertyName("accionesConcedidas")] public List<string> AccionesConcedidas { get; set; } = new();
}

public sealed class RolDetalleDTO
{
    [JsonPropertyName("idRol")] public int? IdRol { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;
    [JsonPropertyName("funciones")] public List<RolFuncionPermisoDTO> Funciones { get; set; } = new();
}

public sealed class PermisoFilaDTO
{
    [JsonPropertyName("funcionCodigo")] public string? FuncionCodigo { get; set; }
    [JsonPropertyName("acciones")] public List<string> Acciones { get; set; } = new();
}

public sealed class PerfilSeguridadDTO
{
    [JsonPropertyName("idPerfil")] public int? IdPerfil { get; set; }
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("esSuperAdministracion")] public bool EsSuperAdministracion { get; set; }
    [JsonPropertyName("gestionaUsuarios")] public bool GestionaUsuarios { get; set; }
    [JsonPropertyName("costaPets")] public bool CostaPets { get; set; }
    [JsonPropertyName("agenteCostaPets")] public bool AgenteCostaPets { get; set; }
    [JsonPropertyName("aceptaConsignacion")] public bool AceptaConsignacion { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;
}

/// <summary>Alta de usuario contra <c>/seguridad/usuarios</c>.</summary>
public sealed class UsuarioAltaDTO
{
    [JsonPropertyName("idUsuario")] public string? IdUsuario { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("claveEntrada")] public string? ClaveEntrada { get; set; }
    [JsonPropertyName("claveInterna")] public string? ClaveInterna { get; set; }
    [JsonPropertyName("idPerfil")] public int IdPerfil { get; set; }
    [JsonPropertyName("iniciales")] public string? Iniciales { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;
    [JsonPropertyName("idRol")] public int? IdRol { get; set; }
}

/// <summary>Cuerpo de <c>/seguridad/usuarios/{id}/perfil</c> y <c>/rol</c>: <c>{ "id": 3 }</c>.</summary>
public sealed class CambioIdDTO
{
    [JsonPropertyName("id")] public int? Id { get; set; }
}

/// <summary>Detalle de usuario que devuelve <c>usuario/ObtenerUnUsuario</c> (contrato nuevo, con <c>idPerfil</c>).</summary>
public sealed class UsuarioDetalleDTO
{
    [JsonPropertyName("id")] public long? Id { get; set; }
    [JsonPropertyName("idUsuario")] public string? IdUsuario { get; set; }
    [JsonPropertyName("nombre")] public string? Nombre { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; }
    [JsonPropertyName("idPerfil")] public int IdPerfil { get; set; }
    [JsonPropertyName("idRol")] public int? IdRol { get; set; }
    [JsonPropertyName("iniciales")] public string? Iniciales { get; set; }
}
