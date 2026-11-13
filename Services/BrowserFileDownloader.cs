using Microsoft.JSInterop;
using PdfMergerWasm.Services.Abstractions;

namespace PdfMergerWasm.Services;

/// <summary>
/// Implementazione di <see cref="IFileDownloader"/> tramite modulo JSInterop isolato (Adapter / DIP).
/// </summary>
public sealed class BrowserFileDownloader : IFileDownloader, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    /// <summary>
    /// Inizializza il downloader caricando in modo lazy il modulo JavaScript dedicato.
    /// </summary>
    public BrowserFileDownloader(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/pdfHelper.js").AsTask());
    }

    /// <summary>
    /// Avvia il download del file sul browser dell'utente.
    /// </summary>
    public async ValueTask DownloadAsync(string fileName, byte[] content, string contentType = "application/pdf")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("downloadFileFromBytes", fileName, contentType, content);
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

