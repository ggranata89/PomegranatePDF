namespace PdfMergerWasm.Services.Abstractions;

/// <summary>
/// Contratto per l'ispezione e la lettura dei metadati di un documento PDF (ISP / DIP).
/// </summary>
public interface IPdfInspector
{
    /// <summary>
    /// Restituisce il numero di pagine presenti nel PDF fornito.
    /// </summary>
    int GetPageCount(byte[] pdfBytes);
}

