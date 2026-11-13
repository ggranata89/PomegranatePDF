using PdfMergerWasm.Models;

namespace PdfMergerWasm.Services.Abstractions;

/// <summary>
/// Contratto per l'unione di più documenti PDF in un unico file (ISP / DIP).
/// </summary>
public interface IPdfMerger
{
    /// <summary>
    /// Unisce i documenti nell'ordine specificato e restituisce i byte del PDF risultante.
    /// </summary>
    byte[] Merge(IReadOnlyList<PdfDocumentItem> documents);
}

