using Havit.Blazor.Components.Web;
using Havit.Blazor.Components.Web.Bootstrap;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="IServicioDialogos" />
public sealed class ServicioDialogos : IServicioDialogos
{
    private readonly IHxMessageBoxService _cajas;
    private readonly IHxMessengerService _avisos;

    public ServicioDialogos(IHxMessageBoxService cajas, IHxMessengerService avisos)
    {
        _cajas = cajas;
        _avisos = avisos;
    }

    public async Task<bool> ConfirmarAsync(string mensaje, string? titulo = null, string? textoConfirmar = null)
    {
        // Con texto propio hace falta el boton Custom; sin el, basta Si/No.
        var conTextoPropio = !string.IsNullOrWhiteSpace(textoConfirmar);

        var r = await _cajas.ShowAsync(new MessageBoxRequest
        {
            Title = titulo ?? "Confirmar",
            Text = mensaje,
            Buttons = conTextoPropio ? MessageBoxButtons.CustomCancel : MessageBoxButtons.YesNo,
            CustomButtonText = textoConfirmar,
            PrimaryButton = conTextoPropio ? MessageBoxButtons.Custom : MessageBoxButtons.Yes
        });

        return r == (conTextoPropio ? MessageBoxButtons.Custom : MessageBoxButtons.Yes);
    }

    public async Task<bool> ConfirmarPeligroAsync(string mensaje, string? titulo = null)
    {
        var r = await _cajas.ShowAsync(new MessageBoxRequest
        {
            Title = titulo ?? "Confirmar",
            Text = mensaje,
            Buttons = MessageBoxButtons.CustomCancel,
            CustomButtonText = "Si, continuar",
            // Cancelar es lo primario: destruir datos no debe ser el camino facil.
            PrimaryButton = MessageBoxButtons.Cancel
        });

        return r == MessageBoxButtons.Custom;
    }

    public Task InformarAsync(string mensaje, string? titulo = null)
        => _cajas.ShowAsync(new MessageBoxRequest
        {
            Title = titulo ?? "Aviso",
            Text = mensaje,
            Buttons = MessageBoxButtons.Ok
        });

    public Task ErrorAsync(string mensaje, string? titulo = null)
        => _cajas.ShowAsync(new MessageBoxRequest
        {
            Title = titulo ?? "Error",
            Text = mensaje,
            Buttons = MessageBoxButtons.Ok
        });

    public void Exito(string mensaje, string? titulo = null)
        => _avisos.AddInformation(titulo ?? "Listo", mensaje);

    public void Advertencia(string mensaje, string? titulo = null)
        => _avisos.AddWarning(titulo ?? "Atencion", mensaje);

    public void ErrorBreve(string mensaje, string? titulo = null)
        => _avisos.AddError(titulo ?? "Error", mensaje);
}
