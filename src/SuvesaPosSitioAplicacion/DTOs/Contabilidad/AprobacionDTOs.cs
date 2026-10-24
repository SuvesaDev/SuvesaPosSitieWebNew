using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.DTOs.Contabilidad;

// Espejo de DTOs/Contabilidad/AprobacionDTOs.cs del API (F2 aprobaciones, F4 multimoneda).
// TEMPORAL: se sustituye por los generados al regenerar los contratos NSwag.

public sealed class ConfiguracionAprobacionDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("modo")] public string Modo { get; set; } = "Mitigante";
    [JsonPropertyName("roles")] public List<UsuarioRolContableDTO> Roles { get; set; } = new();
}

public sealed class UsuarioRolContableDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("usuario")] public string Usuario { get; set; } = string.Empty;
    [JsonPropertyName("rol")] public string Rol { get; set; } = string.Empty;
}

public sealed class CambiarModoAprobacionDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("modo")] public string Modo { get; set; } = string.Empty;
}

public sealed class AsignarRolContableDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("usuario")] public string Usuario { get; set; } = string.Empty;
    [JsonPropertyName("rol")] public string Rol { get; set; } = string.Empty;
}

public sealed class SolicitarAprobacionDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = string.Empty;
    [JsonPropertyName("idReferencia")] public long? IdReferencia { get; set; }
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = string.Empty;
    [JsonPropertyName("asiento")] public CrearAsientoManualDTO? Asiento { get; set; }
    [JsonPropertyName("periodoAfectado")] public string? PeriodoAfectado { get; set; }
    [JsonPropertyName("causaLegalOFraude")] public bool CausaLegalOFraude { get; set; }
}

public sealed class ResolverSolicitudDTO
{
    [JsonPropertyName("aprobar")] public bool Aprobar { get; set; }
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
    [JsonPropertyName("contrasena")] public string? Contrasena { get; set; }
}

public sealed class SolicitudAprobacionDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = string.Empty;
    [JsonPropertyName("idReferencia")] public long? IdReferencia { get; set; }
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = string.Empty;
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("solicitadoPor")] public string SolicitadoPor { get; set; } = string.Empty;
    [JsonPropertyName("fechaSolicitudUtc")] public DateTime FechaSolicitudUtc { get; set; }
    [JsonPropertyName("resueltoPor")] public string? ResueltoPor { get; set; }
    [JsonPropertyName("fechaResolucionUtc")] public DateTime? FechaResolucionUtc { get; set; }
    [JsonPropertyName("comentarioResolucion")] public string? ComentarioResolucion { get; set; }
    [JsonPropertyName("periodoAfectado")] public string? PeriodoAfectado { get; set; }
    [JsonPropertyName("montoImpacto")] public decimal? MontoImpacto { get; set; }
    [JsonPropertyName("causaLegalOFraude")] public bool CausaLegalOFraude { get; set; }
    [JsonPropertyName("esMaterial")] public bool EsMaterial { get; set; }
    [JsonPropertyName("detalleMaterialidad")] public string? DetalleMaterialidad { get; set; }
    [JsonPropertyName("idAsientoGenerado")] public long? IdAsientoGenerado { get; set; }
    [JsonPropertyName("evidencias")] public int Evidencias { get; set; }
    [JsonPropertyName("asiento")] public CrearAsientoManualDTO? Asiento { get; set; }
}

public sealed class ItemRevisionSemanalDTO
{
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = string.Empty;
    [JsonPropertyName("idAsiento")] public long IdAsiento { get; set; }
    [JsonPropertyName("numero")] public long? Numero { get; set; }
    [JsonPropertyName("fechaContable")] public DateOnly FechaContable { get; set; }
    [JsonPropertyName("usuario")] public string Usuario { get; set; } = string.Empty;
    [JsonPropertyName("concepto")] public string? Concepto { get; set; }
    [JsonPropertyName("monto")] public decimal Monto { get; set; }
}

public sealed class RevisionSemanalDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("semanaInicio")] public DateOnly SemanaInicio { get; set; }
    [JsonPropertyName("semanaFin")] public DateOnly SemanaFin { get; set; }
    [JsonPropertyName("polizasManuales")] public int PolizasManuales { get; set; }
    [JsonPropertyName("reversos")] public int Reversos { get; set; }
    [JsonPropertyName("ajustes")] public int Ajustes { get; set; }
    [JsonPropertyName("reaperturas")] public int Reaperturas { get; set; }
    [JsonPropertyName("firmada")] public bool Firmada { get; set; }
    [JsonPropertyName("firmadoPor")] public string? FirmadoPor { get; set; }
    [JsonPropertyName("fechaFirmaUtc")] public DateTime? FechaFirmaUtc { get; set; }
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
    [JsonPropertyName("items")] public List<ItemRevisionSemanalDTO> Items { get; set; } = new();
}

public sealed class FirmarRevisionSemanalDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("semanaInicio")] public DateOnly SemanaInicio { get; set; }
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
}

public sealed class TipoCambioContableDTO
{
    [JsonPropertyName("fecha")] public DateOnly Fecha { get; set; }
    [JsonPropertyName("moneda")] public string Moneda { get; set; } = string.Empty;
    [JsonPropertyName("compra")] public decimal Compra { get; set; }
    [JsonPropertyName("venta")] public decimal Venta { get; set; }
    [JsonPropertyName("fuente")] public string Fuente { get; set; } = string.Empty;
}

public sealed class RegistrarTipoCambioDTO
{
    [JsonPropertyName("fecha")] public DateOnly Fecha { get; set; }
    [JsonPropertyName("moneda")] public string Moneda { get; set; } = "USD";
    [JsonPropertyName("compra")] public decimal Compra { get; set; }
    [JsonPropertyName("venta")] public decimal Venta { get; set; }
}

public sealed class EjecutarRevaluacionDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    [JsonPropertyName("mes")] public int Mes { get; set; }
    [JsonPropertyName("moneda")] public string Moneda { get; set; } = "USD";
}

public sealed class LineaRevaluacionDTO
{
    [JsonPropertyName("idCuentaContable")] public long IdCuentaContable { get; set; }
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("saldoMonedaExtranjera")] public decimal SaldoMonedaExtranjera { get; set; }
    [JsonPropertyName("saldoFuncionalActual")] public decimal SaldoFuncionalActual { get; set; }
    [JsonPropertyName("saldoFuncionalRevaluado")] public decimal SaldoFuncionalRevaluado { get; set; }
    [JsonPropertyName("ajuste")] public decimal Ajuste { get; set; }
}

public sealed class RevaluacionDTO
{
    [JsonPropertyName("id")] public long? Id { get; set; }
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    [JsonPropertyName("mes")] public int Mes { get; set; }
    [JsonPropertyName("moneda")] public string Moneda { get; set; } = string.Empty;
    [JsonPropertyName("fechaTasa")] public DateOnly FechaTasa { get; set; }
    [JsonPropertyName("tipoCambioVenta")] public decimal TipoCambioVenta { get; set; }
    [JsonPropertyName("totalGanancia")] public decimal TotalGanancia { get; set; }
    [JsonPropertyName("totalPerdida")] public decimal TotalPerdida { get; set; }
    [JsonPropertyName("idAsiento")] public long? IdAsiento { get; set; }
    [JsonPropertyName("procesoEjecutado")] public bool ProcesoEjecutado { get; set; }
    [JsonPropertyName("lineas")] public List<LineaRevaluacionDTO> Lineas { get; set; } = new();
}

// ---- Ola 3 ----

public sealed class ResultadoSincronizacionCatalogoDTO
{
    [JsonPropertyName("cuentasAgregadas")] public int CuentasAgregadas { get; set; }
    [JsonPropertyName("cuentasRenombradas")] public int CuentasRenombradas { get; set; }
    [JsonPropertyName("plantillasNuevas")] public int PlantillasNuevas { get; set; }
    [JsonPropertyName("versionesBorrador")] public int VersionesBorrador { get; set; }
    [JsonPropertyName("detalles")] public List<string> Detalles { get; set; } = new();
}

public sealed class UsoCuentaContableDTO
{
    [JsonPropertyName("uso")] public string Uso { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("codigoPorOmision")] public string CodigoPorOmision { get; set; } = string.Empty;
    [JsonPropertyName("idCuentaContable")] public long? IdCuentaContable { get; set; }
    [JsonPropertyName("codigoCuenta")] public string? CodigoCuenta { get; set; }
    [JsonPropertyName("origen")] public string Origen { get; set; } = string.Empty;
}

public sealed class AsignarUsoCuentaDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("uso")] public string Uso { get; set; } = string.Empty;
    [JsonPropertyName("idCuentaContable")] public long IdCuentaContable { get; set; }
}

public sealed class RegistrarLiquidacionProcesadorDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("idSucursal")] public int? IdSucursal { get; set; }
    [JsonPropertyName("procesador")] public string Procesador { get; set; } = string.Empty;
    [JsonPropertyName("numeroLiquidacion")] public string NumeroLiquidacion { get; set; } = string.Empty;
    [JsonPropertyName("fecha")] public DateOnly Fecha { get; set; }
    [JsonPropertyName("bruto")] public decimal Bruto { get; set; }
    [JsonPropertyName("comision")] public decimal Comision { get; set; }
    [JsonPropertyName("ivaComision")] public decimal IvaComision { get; set; }
    [JsonPropertyName("retencionRenta")] public decimal RetencionRenta { get; set; }
    [JsonPropertyName("neto")] public decimal Neto { get; set; }
}

public sealed class LiquidacionProcesadorDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("procesador")] public string Procesador { get; set; } = string.Empty;
    [JsonPropertyName("numeroLiquidacion")] public string NumeroLiquidacion { get; set; } = string.Empty;
    [JsonPropertyName("fecha")] public DateOnly Fecha { get; set; }
    [JsonPropertyName("bruto")] public decimal Bruto { get; set; }
    [JsonPropertyName("comision")] public decimal Comision { get; set; }
    [JsonPropertyName("ivaComision")] public decimal IvaComision { get; set; }
    [JsonPropertyName("retencionRenta")] public decimal RetencionRenta { get; set; }
    [JsonPropertyName("neto")] public decimal Neto { get; set; }
    [JsonPropertyName("estadoContable")] public string EstadoContable { get; set; } = string.Empty;
    [JsonPropertyName("ultimoError")] public string? UltimoError { get; set; }
}

public sealed class ResumenProcesadoresDTO
{
    [JsonPropertyName("saldoPorLiquidar")] public decimal SaldoPorLiquidar { get; set; }
    [JsonPropertyName("liquidaciones")] public List<LiquidacionProcesadorDTO> Liquidaciones { get; set; } = new();
}

public sealed class EjecutarCorteEntregaDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    [JsonPropertyName("mes")] public int Mes { get; set; }
}

public sealed class DocumentoCorteEntregaDTO
{
    [JsonPropertyName("idVenta")] public long IdVenta { get; set; }
    [JsonPropertyName("numeroDocumento")] public string? NumeroDocumento { get; set; }
    [JsonPropertyName("ingreso")] public decimal Ingreso { get; set; }
    [JsonPropertyName("costo")] public decimal Costo { get; set; }
}

public sealed class CorteEntregaDTO
{
    [JsonPropertyName("id")] public long? Id { get; set; }
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    [JsonPropertyName("mes")] public int Mes { get; set; }
    [JsonPropertyName("fechaCorte")] public DateOnly FechaCorte { get; set; }
    [JsonPropertyName("cantidadDocumentos")] public int CantidadDocumentos { get; set; }
    [JsonPropertyName("ingresoGravado")] public decimal IngresoGravado { get; set; }
    [JsonPropertyName("ingresoExento")] public decimal IngresoExento { get; set; }
    [JsonPropertyName("ingresoDiferido")] public decimal IngresoDiferido { get; set; }
    [JsonPropertyName("costoDiferido")] public decimal CostoDiferido { get; set; }
    [JsonPropertyName("utilidadDelMes")] public decimal UtilidadDelMes { get; set; }
    [JsonPropertyName("umbral")] public decimal Umbral { get; set; }
    [JsonPropertyName("material")] public bool Material { get; set; }
    [JsonPropertyName("ejecutado")] public bool Ejecutado { get; set; }
    [JsonPropertyName("idAsiento")] public long? IdAsiento { get; set; }
    [JsonPropertyName("fechaReverso")] public DateOnly FechaReverso { get; set; }
    [JsonPropertyName("idAsientoReverso")] public long? IdAsientoReverso { get; set; }
    [JsonPropertyName("documentos")] public List<DocumentoCorteEntregaDTO> Documentos { get; set; } = new();
    [JsonPropertyName("avisos")] public List<string> Avisos { get; set; } = new();
}

public sealed class LineaGastoDTO
{
    [JsonPropertyName("idCuentaContable")] public long IdCuentaContable { get; set; }
    [JsonPropertyName("monto")] public decimal Monto { get; set; }
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("idValorDimension")] public long? IdValorDimension { get; set; }
}

public sealed class RegistrarGastoDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("idSucursal")] public int IdSucursal { get; set; }
    [JsonPropertyName("idProveedor")] public long IdProveedor { get; set; }
    [JsonPropertyName("nombreProveedor")] public string NombreProveedor { get; set; } = string.Empty;
    [JsonPropertyName("fechaDocumento")] public DateOnly FechaDocumento { get; set; }
    [JsonPropertyName("numeroDocumento")] public string NumeroDocumento { get; set; } = string.Empty;
    [JsonPropertyName("concepto")] public string Concepto { get; set; } = string.Empty;
    [JsonPropertyName("iva")] public decimal Iva { get; set; }
    [JsonPropertyName("retencion")] public decimal Retencion { get; set; }
    [JsonPropertyName("referenciaRetencion")] public string? ReferenciaRetencion { get; set; }
    [JsonPropertyName("fechaVencimiento")] public DateOnly? FechaVencimiento { get; set; }
    [JsonPropertyName("lineas")] public List<LineaGastoDTO> Lineas { get; set; } = new();
}

public sealed class GastoDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("idProveedor")] public long IdProveedor { get; set; }
    [JsonPropertyName("nombreProveedor")] public string NombreProveedor { get; set; } = string.Empty;
    [JsonPropertyName("fechaDocumento")] public DateOnly FechaDocumento { get; set; }
    [JsonPropertyName("numeroDocumento")] public string NumeroDocumento { get; set; } = string.Empty;
    [JsonPropertyName("concepto")] public string Concepto { get; set; } = string.Empty;
    [JsonPropertyName("subtotal")] public decimal Subtotal { get; set; }
    [JsonPropertyName("iva")] public decimal Iva { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("retencion")] public decimal Retencion { get; set; }
    [JsonPropertyName("referenciaRetencion")] public string? ReferenciaRetencion { get; set; }
    [JsonPropertyName("fechaVencimiento")] public DateOnly? FechaVencimiento { get; set; }
    [JsonPropertyName("fechaPagoProgramada")] public DateOnly? FechaPagoProgramada { get; set; }
    [JsonPropertyName("fechaPago")] public DateOnly? FechaPago { get; set; }
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("idAsiento")] public long? IdAsiento { get; set; }
    [JsonPropertyName("motivoAnulacion")] public string? MotivoAnulacion { get; set; }
    [JsonPropertyName("lineas")] public List<LineaGastoDTO> Lineas { get; set; } = new();
}

public sealed class FechaGastoDTO
{
    [JsonPropertyName("fecha")] public DateOnly Fecha { get; set; }
}

public sealed class AnularGastoDTO
{
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = string.Empty;
}

public sealed class EstadoTablaRetencionesDTO
{
    [JsonPropertyName("cargada")] public bool Cargada { get; set; }
    [JsonPropertyName("filas")] public int Filas { get; set; }
    [JsonPropertyName("mensaje")] public string Mensaje { get; set; } = string.Empty;
}

public sealed class DescartarEventoDTO
{
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = string.Empty;
}

public sealed class EstatusContableDocumentoDTO
{
    [JsonPropertyName("tipoEvento")] public string TipoEvento { get; set; } = string.Empty;
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("ultimoError")] public string? UltimoError { get; set; }
    [JsonPropertyName("idAsiento")] public long? IdAsiento { get; set; }
    [JsonPropertyName("numeroPoliza")] public long? NumeroPoliza { get; set; }
    [JsonPropertyName("fechaContable")] public DateOnly FechaContable { get; set; }
}
