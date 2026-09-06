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
    Task<ResponseGeneric<CajasCantidad>> CrearCaja(long numCaja);
    Task<ResponseGeneric<bool>> EliminarCaja(long idCaja);
    Task<ResponseGeneric<ICollection<DenominacionMonedum>>> Denominaciones();
    Task<ResponseGeneric<ICollection<User>>> CajerosConCajaAbierta();
    Task<ResponseGeneric<ICollection<AperturaCajaDTO>>> AperturasSinCerrar();
    Task<ResponseGeneric<ICollection<ObtenerAperturaCajaDTO>>> AperturasSinArqueo();
    Task<ResponseGeneric<AperturaCajaDTO>> CrearApertura(AperturaCajaDTO apertura);
    Task<ResponseGeneric<ArqueoCajaDTO>> CrearArqueo(ArqueoCajaDTO arqueo);
    Task<ResponseGeneric<ObtenerDatosCierreCaja>> DatosCierre(long numeroApertura);
    Task<ResponseGeneric<CierreCajaDTO>> CrearCierre(CierreCajaDTO cierre);
    Task<ResponseGeneric<ICollection<EntidadesBancariasDTO>>> Bancos();
    Task<ResponseGeneric<ICollection<EmpresaDTO>>> Empresas();
    Task<ResponseGeneric<ICollection<CuentaBancariaDTO>>> Cuentas(int banco, int empresa);
    Task<ResponseGeneric<ICollection<PreDepositosDTO>>> PreDepositosDeApertura(long apertura);
    Task<ResponseGeneric<ICollection<PreDepositosBuscarDTO>>> BuscarPreDepositos(FiltroBusquedaPreDepositosDTO filtro);
    Task<ResponseGeneric<ICollection<DepositosBuscarDTO>>> BuscarDepositos(FiltroBusquedaDepositosDTO filtro);
    Task<ResponseGeneric<PreDepositosDTO>> CrearPreDeposito(PreDepositosDTO deposito);
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
    Task<ResponseGeneric<ICollection<ArqueoCajaFiltroResultadoDTO>>> BuscarArqueos(ArqueoCajaFiltroDTO filtro);
    Task<ResponseGeneric<ArqueoCajaDTO>> ObtenerArqueo(long id);
    Task<ResponseGeneric<ArqueoCajaDTO>> EditarArqueo(ArqueoCajaDTO arqueo);
    Task<ResponseGeneric<ArqueoCajaDTO>> AnularArqueo(long id);

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
    Task<ResponseGeneric<ArqueoMontoDepositoDTO>> MontoDepositosDeApertura(int numeroApertura);

    /// <summary>Catalogo de tipos de tarjeta/forma de pago para desglosar el arqueo
    /// (una fila por tipo, en vez de un total de tarjetas sin desglosar).</summary>
    Task<ResponseGeneric<ICollection<FormasPagoDTO>>> TiposDeTarjeta();
}
