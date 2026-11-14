using PomegranatePDF.Models;
using PdfSharp.Pdf;

namespace PomegranatePDF.Services.Abstractions;

/// <summary>
/// Strategy Pattern: definisce una trasformazione applicabile a una singola pagina PDF
/// durante il processo di unione (OCP).
/// </summary>
public interface IPdfPageTransform
{
    /// <summary>
    /// Applica la trasformazione alla singola pagina PDF in base ai metadati del documento sorgente.
    /// </summary>
    void Apply(PdfPage page, PdfDocumentItem sourceDocument);
}

