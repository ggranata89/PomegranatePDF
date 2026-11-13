using PdfSharp.Pdf;

namespace PdfMergerWasm.Tests;

internal static class TestPdfFactory
{
    /// <summary>
    /// Crea un PDF valido in memoria con il numero di pagine e la larghezza/rotazione iniziale specificate.
    /// </summary>
    public static byte[] CreatePdf(int pageCount = 1, double pageWidthPt = 595, int initialRotate = 0)
    {
        using var document = new PdfDocument();
        for (var i = 0; i < pageCount; i++)
        {
            var page = document.AddPage();
            page.Width = PdfSharp.Drawing.XUnit.FromPoint(pageWidthPt + i);
            page.Height = PdfSharp.Drawing.XUnit.FromPoint(842);
            page.Rotate = initialRotate;
        }

        using var stream = new MemoryStream();
        document.Save(stream, closeStream: false);
        return stream.ToArray();
    }
}

