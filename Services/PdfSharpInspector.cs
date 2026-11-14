using PomegranatePDF.Services.Abstractions;
using PdfSharp.Pdf.IO;

namespace PomegranatePDF.Services;

/// <summary>
/// Implementazione di <see cref="IPdfInspector"/> basata su PDFsharp (SRP).
/// </summary>
public sealed class PdfSharpInspector : IPdfInspector
{
    /// <summary>
    /// Apre il PDF in modalità importazione e restituisce il numero totale di pagine.
    /// </summary>
    public int GetPageCount(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        using var stream = new MemoryStream(pdfBytes, writable: false);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        return document.PageCount;
    }
}

