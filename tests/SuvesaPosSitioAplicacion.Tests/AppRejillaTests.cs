using SuvesaPosSitioAplicacion.DTOs.Reportes;
using SuvesaPosSitioAplicacion.Views.Shared.Componentes;

namespace SuvesaPosSitioAplicacion.Tests;

public sealed class AppRejillaTests
{
    [Fact]
    public void EstadoUsuario_IniciaConUnValorValido()
    {
        var rejilla = new AppRejilla<FilaReporteOperacionWebDTO>();

        Assert.NotNull(rejilla.EstadoUsuario);
        Assert.Equal(0, rejilla.EstadoUsuario.PageIndex);
        Assert.Empty(rejilla.EstadoUsuario.Sorting);
    }
}
