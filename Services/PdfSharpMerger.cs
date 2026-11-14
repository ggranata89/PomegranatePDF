using PomegranatePDF.Models;
using PomegranatePDF.Services.Abstractions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PomegranatePDF.Services;

/// <summary>
/// Implementazione di <see cref="IPdfMerger"/> basata su PDFsharp.
/// Applica tutte le strategie <see cref="IPdfPageTransform"/> registrate in DI (OCP / Strategy Pattern).
/// </summary>
public sealed class PdfSharpMerger : IPdfMerger
{
    private readonly IReadOnlyList<IPdfPageTransform> _pageTransforms;

    /// <summary>
    /// Inizializza il servizio di unione con le strategie di trasformazione pagina registrate.
    /// </summary>
    public PdfSharpMerger(IEnumerable<IPdfPageTransform> pageTransforms)
    {
        ArgumentNullException.ThrowIfNull(pageTransforms);
        _pageTransforms = pageTransforms.ToArray();
    }

    /// <summary>
    /// Unisce i documenti PDF nell'ordine fornito applicando le trasformazioni configurate.
    /// </summary>
    public byte[] Merge(IReadOnlyList<PdfDocumentItem> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        if (documents.Count == 0)
        {
            throw new InvalidOperationException("Seleziona almeno un file PDF da unire.");
        }

        using var outputDocument = new PdfDocument();

        foreach (var item in documents)
        {
            AppendDocument(outputDocument, item);
        }

        return SerializeDocument(outputDocument);
    }

    /// <summary>
    /// Apre un documento PDF sorgente e ne importa tutte le pagine nel documento di output.
    /// </summary>
    private void AppendDocument(PdfDocument outputDocument, PdfDocumentItem item)
    {
        using var inputStream = new MemoryStream(item.Content, writable: false);
        using var inputDocument = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);

        foreach (var sourcePage in inputDocument.Pages)
        {
            ImportAndTransformPage(outputDocument, sourcePage, item);
        }
    }

    /// <summary>
    /// Copia la singola pagina nel documento finale e vi applica tutte le trasformazioni.
    /// </summary>
    private void ImportAndTransformPage(PdfDocument outputDocument, PdfPage sourcePage, PdfDocumentItem item)
    {
        var addedPage = outputDocument.AddPage(sourcePage);
        ApplyTransforms(addedPage, item);
    }

    /// <summary>
    /// Esegue in sequenza le strategie di trasformazione registrate sulla pagina indicata.
    /// </summary>
    private void ApplyTransforms(PdfPage page, PdfDocumentItem item)
    {
        foreach (var transform in _pageTransforms)
        {
            transform.Apply(page, item);
        }
    }

    /// <summary>
    /// Serializza il documento PDF finale in un array di byte in memoria.
    /// </summary>
    private static byte[] SerializeDocument(PdfDocument document)
    {
        using var outputStream = new MemoryStream();
        document.Save(outputStream, closeStream: false);
        return outputStream.ToArray();
    }
}

