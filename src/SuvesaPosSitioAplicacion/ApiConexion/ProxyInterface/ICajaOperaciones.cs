using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Caja;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Operaciones de caja: aperturas, arqueos, cierres y depósitos.</summary>
public interface ICajaOperaciones
{
    Task<ResponseGeneric<Usuario>> ValidarClaveInterna(string contrasena);
    Task<ResponseGeneric<UsuarioCajaAbiertaValidadaWebDTO>> ValidarClaveInternaConCajaAbierta(string contrasena);
    Task<ResponseGeneric<ICollection<CajasCantidad>>> CajasDisponibles();

    /// <summary>Todas las cajas del catálogo (mantenimiento en Parámetros).</summary>
    Task<ResponseGeneric<ICollection<CajasCantidad>>> TodasLasCajas();
    /// <summary>Lo mismo que <see cref="TodasLasCajas"/>, con el cajero de la apertura activa de cada caja.</summary>
    Task<ResponseGeneric<ICollection<CajaMantenimientoDTO>>> CajasMantenimiento();
    Task<ResponseGeneric<CajasCantidad>> CrearCaja(long numCaja);
    Task<ResponseGeneric<bool>> EliminarCaja(long idCaja);
    Task<ResponseGeneric<ICollection<DenominacionMonedum>>> Denominaciones();
    Task<ResponseGeneric<ICollection<User>>> CajerosConCajaAbierta();
    Task<ResponseGeneric<ICollection<AperturaCajaDTO>>> AperturasSinCerrar();
    /// <summary>Apertura abierta del cajero dueño de la clave interna. El API no devuelve las de otros usuarios.</summary>
    Task<ResponseGeneric<ICollection<ObtenerAperturaCajaDTO>>> AperturasSinArqueo(string? claveInterna);
    Task<ResponseGeneric<AperturaCajaDTO>> CrearApertura(AperturaCajaDTO apertura);
    Task<ResponseGeneric<ArqueoCajaDTO>> CrearArqueo(ArqueoCajaDTO arqueo, string? claveInterna);
    Task<ResponseGeneric<ObtenerDatosCierreCaja>> DatosCierre(long numeroApertura);
    Task<ResponseGeneric<CierreCajaDTO>> CrearCierre(CierreCajaDTO cierre);
    Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Bancos();
    Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas();
    Task<ResponseGeneric<ICollection<CuentaBancariaDTO>>> Cuentas(int banco, int empresa);
    Task<ResponseGeneric<ICollection<PreDepositosDTO>>> PreDepositosDeApertura(long apertura);
    Task<ResponseGeneric<ICollection<PreDepositosBuscarDTO>>> BuscarPreDepositos(FiltroBusquedaPreDepositosDTO filtro);
    Task<ResponseGeneric<ICollection<DepositosBuscarDTO>>> BuscarDepositos(FiltroBusquedaDepositosDTO filtro);
    /// <summary>Solo el cajero dueño de la apertura puede sacar efectivo de ella: el API valida su clave interna.</summary>
    Task<ResponseGeneric<PreDepositosDTO>> CrearPreDeposito(PreDepositosDTO deposito, string? claveInterna);
    Task<ResponseGeneric<PreDepositosDTO>> EliminarPreDeposito(int id);
    Task<ResponseGeneric<DepositosDTO>> CrearDeposito(DepositosDTO deposito);

    /// <summary>
    /// Tipo de cambio del dólar (venta) para consolidar montos en dólares dentro
    /// de un total en colones — el arqueo lo necesita para no sumar las dos
    /// monedas como si fueran una sola.
    /// </summary>
    Task<ResponseGeneric<Moneda>> TipoCambioDolar();

    // --- Apertura: buscar, abrir para editar, anular ---
    // No hay "editar la apertura completa": el API edita cada linea de
    // denominacion y cada linea de total/tope por separado.
    Task<ResponseGeneric<ICollection<AperturaCajaFiltroResultadoDTO>>> BuscarAperturas(AperturaCajaFiltroDTO filtro);
    Task<ResponseGeneric<AperturaCajaDTO>> ObtenerApertura(int numeroApertura);
    Task<ResponseGeneric<AperturaDenominacionDTO>> EditarDenominacionApertura(AperturaDenominacionDTO linea);
    Task<ResponseGeneric<AperturaTotalTopeDTO>> EditarTotalTopeApertura(AperturaTotalTopeDTO linea);
    Task<ResponseGeneric<AperturaCajaDTO>> AnularApertura(int numeroApertura);

    // --- Arqueo: buscar, abrir para editar, anular ---
    // Sin clave interna el API solo responde a quien tenga el permiso de consulta de arqueos.
    Task<ResponseGeneric<ICollection<ArqueoCajaFiltroResultadoDTO>>> BuscarArqueos(ArqueoCajaFiltroDTO filtro, string? claveInterna = null);
    Task<ResponseGeneric<ArqueoCajaDTO>> ObtenerArqueo(long id, string? claveInterna = null);
    Task<ResponseGeneric<ArqueoCajaDTO>> EditarArqueo(ArqueoCajaDTO arqueo, string? claveInterna);
    Task<ResponseGeneric<ArqueoCajaDTO>> AnularArqueo(long id, string? claveInterna);

    // --- Cierre: buscar y anular. No existe "editar cierre" en el API: un
    // cierre ya hecho solo se anula, no se corrige. ---
    Task<ResponseGeneric<ICollection<FiltroCierreCajaBusquedaDTO>>> BuscarCierres(BuscarCierreCajaDTO filtro);
    Task<ResponseGeneric<CierreCajaDTO>> AnularCierre(long idCierre);

    /// <summary>Consolidado de un cierre ya registrado (a diferencia de <see cref="DatosCierre"/>,
    /// que es el consolidado PREVIO a cerrar, usado para armar el cierre nuevo).</summary>
    Task<ResponseGeneric<ObtenerDatosCierreCaja>> DatosCierreRegistrado(long numeroApertura);

    // --- Arqueo: lo que el sistema registro durante la apertura, para
    // comparar contra lo que declara el cajero (el proposito real de un arqueo). ---

    /// <summary>Documentos (facturas) cobrados durante la apertura, por forma de pago.</summary>
    Task<ResponseGeneric<ICollection<OpcionesDePagoCierreCaja>>> DocumentosDeApertura(long numeroApertura);

    /// <summary>Total de depositos que el sistema tiene registrados para esta apertura.</summary>
    Task<ResponseGeneric<ArqueoMontoDepositoDTO>> MontoDepositosDeApertura(int numeroApertura, string? claveInterna = null);

    /// <summary>Lo que el sistema tiene registrado en la apertura, por moneda, calculado en el API. Solo referencia: no compara.</summary>
    Task<ResponseGeneric<ArqueoReferenciaSistemaWebDTO>> ReferenciaSistemaArqueo(long numeroApertura, string? claveInterna = null);

    /// <summary>Catalogo de tipos de tarjeta/forma de pago para desglosar el arqueo
    /// (una fila por tipo, en vez de un total de tarjetas sin desglosar).</summary>
    Task<ResponseGeneric<ICollection<FormasPagoDTO>>> TiposDeTarjeta();
}
