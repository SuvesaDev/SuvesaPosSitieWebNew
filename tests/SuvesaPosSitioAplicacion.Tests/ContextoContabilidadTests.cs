using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// ContextoContabilidad (W1): resuelve el flag por emisor activo y lo cachea — a
/// diferencia de IContextoSesion.HabilitaImportaciones (claim fijo al login), aquí el
/// emisor activo puede cambiar dentro de la sesión.
/// </summary>
public class ContextoContabilidadTests
{
    private sealed class ContabilidadFalsa : IContabilidad
    {
        public int LlamadasEstadoActivacion { get; private set; }
        public bool Efectivo { get; set; }

        public Task<ResponseGeneric<EstadoActivacionContabilidadDTO>> EstadoActivacion(long idEmpresa, int? idEmisor)
        {
            LlamadasEstadoActivacion++;
            return Task.FromResult(new ResponseGeneric<EstadoActivacionContabilidadDTO>(
                new EstadoActivacionContabilidadDTO { IdEmpresa = idEmpresa, IdEmisor = idEmisor, Efectivo = Efectivo }));
        }

        public Task<ResponseGeneric<bool>> ActivarEmpresa(long idEmpresa) => Task.FromResult(new ResponseGeneric<bool>(true));
        public Task<ResponseGeneric<bool>> DesactivarEmpresa(long idEmpresa) => Task.FromResult(new ResponseGeneric<bool>(true));
        public Task<ResponseGeneric<long>> ActivarEmisor(ActivarEmisorDTO comando) => Task.FromResult(new ResponseGeneric<long>(1L));
        public Task<ResponseGeneric<bool>> DesactivarEmisor(int idEmisor) => Task.FromResult(new ResponseGeneric<bool>(true));
        public Task<ResponseGeneric<bool>> ConfirmarClave(string contrasena) => Task.FromResult(new ResponseGeneric<bool>(true));

        public Task<ResponseGeneric<ICollection<CuentaContableDTO>>> ListarCuentas(long idLibroContable) => throw new NotImplementedException();
        public Task<ResponseGeneric<CuentaContableDTO>> CrearCuenta(CrearCuentaContableDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<CuentaContableDTO>> EditarCuenta(long idCuenta, EditarCuentaContableDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<DimensionContableDTO>>> ListarDimensiones(long idLibroContable) => throw new NotImplementedException();
        public Task<ResponseGeneric<long>> CrearDimension(CrearDimensionContableDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<ValorDimensionContableDTO>>> ListarValoresDimension(long idDimension) => throw new NotImplementedException();
        public Task<ResponseGeneric<long>> CrearValorDimension(CrearValorDimensionDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<PlantillaContableDTO>>> ListarPlantillas(long idLibroContable) => throw new NotImplementedException();
        public Task<ResponseGeneric<long>> CrearPlantilla(CrearPlantillaContableDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<VersionPlantillaDTO>>> ListarVersiones(long idPlantilla) => throw new NotImplementedException();
        public Task<ResponseGeneric<VersionPlantillaDetalleDTO>> ObtenerVersion(long idVersion) => throw new NotImplementedException();
        public Task<ResponseGeneric<VersionPlantillaDTO>> CrearVersion(long idPlantilla, CrearVersionPlantillaDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<VersionPlantillaDTO>> ActivarVersion(long idVersion) => throw new NotImplementedException();
        public Task<ResponseGeneric<ResultadoSimulacionDTO>> Simular(long idVersion, string payloadJson) => throw new NotImplementedException();

        public Task<ResponseGeneric<ICollection<EventoContableDTO>>> ListarEventos(long? idLibroContable, string? estado, string? tipoEvento, string? origenModulo, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina) => throw new NotImplementedException();
        public Task<ResponseGeneric<bool>> ReintentarEvento(long idEvento) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<AsientoContableDTO>>> ListarAsientos(long? idLibroContable, int pagina, int tamanoPagina, long? idCliente = null, long? idProveedor = null, string? origenModulo = null) => throw new NotImplementedException();
        public Task<ResponseGeneric<AsientoContableDTO>> CrearAsientoManual(CrearAsientoManualDTO comando) => throw new NotImplementedException();
        public Task<ResponseGeneric<AsientoContableDTO>> ReversarAsiento(long idAsiento, string? contrasena) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionRepolinizacionDTO>> SimularRepolinizacion(FiltroRepolinizacionDTO filtro) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionRepolinizacionDTO>> ObtenerRepolinizacion(long idEjecucion) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionRepolinizacionDTO>> AprobarRepolinizacion(long idEjecucion, string contrasena) => throw new NotImplementedException();

        public Task<ResponseGeneric<ICollection<PeriodoContableDTO>>> ListarPeriodos(long idLibroContable) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<AuditoriaContableDTO>>> ListarAuditoria(long? idLibroContable, string? entidad, string? usuario, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina) => throw new NotImplementedException();
        public Task<ResponseGeneric<ICollection<AsientoContableDTO>>> ObtenerLibroDiario(long idLibroContable, DateOnly desde, DateOnly hasta) => throw new NotImplementedException();
        public Task<ResponseGeneric<LibroMayorDTO>> ObtenerLibroMayor(long idCuentaContable, DateOnly desde, DateOnly hasta) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarCxC(int idEmisor) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarCxP(int idEmisor) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarInventario(long idLibroContable) => throw new NotImplementedException();
        public Task<ResponseGeneric<EjecucionConciliacionDTO>> ObtenerConciliacion(long idEjecucion) => throw new NotImplementedException();
        public Task<ResponseGeneric<bool>> ResolverDiferencia(long idDiferencia, string motivo, string? contrasena) => throw new NotImplementedException();
    }

    [Fact]
    public async Task ResolverAsync_ReflejaElEfectivoDelEstadoDeActivacion()
    {
        var proxy = new ContabilidadFalsa { Efectivo = true };
        var contexto = new ContextoContabilidad(proxy);

        await contexto.ResolverAsync(1, 100);

        Assert.True(contexto.HabilitadaEmisorActual);
    }

    [Fact]
    public async Task ResolverAsync_CachaPorEmisorYNoRepiteLaLlamada()
    {
        var proxy = new ContabilidadFalsa { Efectivo = true };
        var contexto = new ContextoContabilidad(proxy);

        await contexto.ResolverAsync(1, 100);
        await contexto.ResolverAsync(1, 100);
        await contexto.ResolverAsync(1, 100);

        Assert.Equal(1, proxy.LlamadasEstadoActivacion);
    }

    [Fact]
    public async Task ResolverAsync_ResuelveDeNuevoParaUnEmisorDistinto()
    {
        var proxy = new ContabilidadFalsa { Efectivo = true };
        var contexto = new ContextoContabilidad(proxy);

        await contexto.ResolverAsync(1, 100);
        await contexto.ResolverAsync(1, 200);

        Assert.Equal(2, proxy.LlamadasEstadoActivacion);
    }

    [Fact]
    public void HabilitadaEmisorActual_EsFalsaPorDefectoAntesDeResolver()
        => Assert.False(new ContextoContabilidad(new ContabilidadFalsa()).HabilitadaEmisorActual);
}
