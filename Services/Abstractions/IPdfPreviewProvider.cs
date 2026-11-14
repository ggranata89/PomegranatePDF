namespace PomegranatePDF.Services.Abstractions;

/// <summary>
/// Astrazione per la creazione e la revoca di URL temporanei di anteprima PDF nel browser (ISP / DIP).
/// </summary>
public interface IPdfPreviewProvider
{
    /// <summary>
    /// Crea un URL temporaneo utilizzabile da un elemento HTML (es. iframe) per mostrare l'anteprima del PDF.
    /// </summary>
    ValueTask<string> CreatePreviewUrlAsync(byte[] pdfBytes);

    /// <summary>
    /// Rilascia un URL temporaneo creato in precedenza per liberare memoria nel browser.
    /// </summary>
    ValueTask RevokePreviewUrlAsync(string? previewUrl);
}

