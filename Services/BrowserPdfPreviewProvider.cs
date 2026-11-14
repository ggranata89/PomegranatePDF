using Microsoft.JSInterop;
using PomegranatePDF.Services.Abstractions;

namespace PomegranatePDF.Services;

/// <summary>
/// Implementazione di <see cref="IPdfPreviewProvider"/> tramite modulo JSInterop isolato (Adapter / DIP).
/// </summary>
public sealed class BrowserPdfPreviewProvider : IPdfPreviewProvider, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    /// <summary>
    /// Inizializza il provider caricando in modo lazy il modulo JavaScript dedicato.
    /// </summary>
    public BrowserPdfPreviewProvider(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/pdfHelper.js").AsTask());
    }

    /// <summary>
    /// Crea un Blob URL per l'anteprima del PDF nel browser.
    /// </summary>
    public async ValueTask<string> CreatePreviewUrlAsync(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("createPdfObjectUrl", pdfBytes);
    }

    /// <summary>
    /// Revoca un Blob URL non più necessario.
    /// </summary>
    public async ValueTask RevokePreviewUrlAsync(string? previewUrl)
    {
        if (string.IsNullOrWhiteSpace(previewUrl))
        {
            return;
        }

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("revokePdfObjectUrl", previewUrl);
    }

    /// <summary>
    /// Rilascia il riferimento al modulo JavaScript se è stato caricato.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_moduleTask.IsValueCreated)
        {
            return;
        }

        var module = await _moduleTask.Value;
        await module.DisposeAsync();
    }
}

