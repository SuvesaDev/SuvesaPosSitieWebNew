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
