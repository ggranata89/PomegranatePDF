namespace PdfMergerWasm.Services.Abstractions;

/// <summary>
/// Astrazione per il download di un file generato sul dispositivo dell'utente (DIP / ISP).
/// Disaccoppia la UI e la logica applicativa dall'implementazione JSInterop del browser.
/// </summary>
public interface IFileDownloader
{
    /// <summary>
    /// Avvia il download del file sul browser dell'utente.
    /// </summary>
    ValueTask DownloadAsync(string fileName, byte[] content, string contentType = "application/pdf");
}

