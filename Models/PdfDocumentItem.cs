namespace PomegranatePDF.Models;

/// <summary>
/// Rappresenta un singolo file PDF caricato dall'utente, con il suo stato di rotazione e metadati.
/// </summary>
public sealed class PdfDocumentItem
{
    public Guid Id { get; } = Guid.NewGuid();

    public required string FileName { get; init; }

    public required byte[] Content { get; init; }

    public int PageCount { get; init; }

    /// <summary>
    /// Rotazione aggiuntiva da applicare a tutte le pagine del documento (0, 90, 180, 270).
    /// </summary>
    public int RotationDegrees { get; private set; }

    public string FormattedSize => Content.Length switch
    {
        < 1024 => $"{Content.Length} B",
        < 1024 * 1024 => $"{Content.Length / 1024.0:F1} KB",
        _ => $"{Content.Length / (1024.0 * 1024.0):F2} MB"
    };

    /// <summary>
    /// Ruota il documento di 90 gradi in senso orario.
    /// </summary>
    public void RotateRight() => RotationDegrees = (RotationDegrees + 90) % 360;

    /// <summary>
    /// Ruota il documento di 90 gradi in senso antiorario.
    /// </summary>
    public void RotateLeft() => RotationDegrees = (RotationDegrees + 270) % 360;
}

