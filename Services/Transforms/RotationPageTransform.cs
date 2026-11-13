using PdfMergerWasm.Models;
using PdfMergerWasm.Services.Abstractions;
using PdfSharp.Pdf;

namespace PdfMergerWasm.Services.Transforms;

/// <summary>
/// Strategia concreta che applica la rotazione configurata sul documento sorgente a ciascuna pagina.
/// </summary>
public sealed class RotationPageTransform : IPdfPageTransform
{
    /// <summary>
    /// Applica la rotazione configurata sul documento sorgente alla pagina di destinazione.
    /// </summary>
    public void Apply(PdfPage page, PdfDocumentItem sourceDocument)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(sourceDocument);

        if (sourceDocument.RotationDegrees == 0)
        {
            return;
        }

        page.Rotate = (page.Rotate + sourceDocument.RotationDegrees) % 360;
    }
}

