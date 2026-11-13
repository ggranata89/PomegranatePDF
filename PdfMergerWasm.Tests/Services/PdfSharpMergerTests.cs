using PdfMergerWasm.Models;
using PdfMergerWasm.Services;
using PdfMergerWasm.Services.Abstractions;
using PdfMergerWasm.Services.Transforms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfMergerWasm.Tests.Services;

public sealed class PdfSharpMergerTests
{
    [Fact]
    public void Constructor_WhenTransformsAreNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PdfSharpMerger(null!));
    }

    [Fact]
    public void Merge_WhenDocumentsAreNull_ThrowsArgumentNullException()
    {
        var merger = new PdfSharpMerger([]);
        Assert.Throws<ArgumentNullException>(() => merger.Merge(null!));
    }

    [Fact]
    public void Merge_WhenDocumentsAreEmpty_ThrowsInvalidOperationException()
    {
        var merger = new PdfSharpMerger([]);
        Assert.Throws<InvalidOperationException>(() => merger.Merge([]));
    }

    [Fact]
    public void Merge_CombinesPagesInOrderAndAppliesTransforms()
    {
        var firstDoc = new PdfDocumentItem
        {
            FileName = "first.pdf",
            Content = TestPdfFactory.CreatePdf(pageCount: 2, pageWidthPt: 400),
            PageCount = 2
        };
        firstDoc.RotateRight();

        var secondDoc = new PdfDocumentItem
        {
            FileName = "second.pdf",
            Content = TestPdfFactory.CreatePdf(pageCount: 1, pageWidthPt: 500, initialRotate: 90),
            PageCount = 1
        };
        secondDoc.RotateRight();

        var merger = new PdfSharpMerger([new RotationPageTransform()]);

        var mergedBytes = merger.Merge([firstDoc, secondDoc]);

        using var stream = new MemoryStream(mergedBytes, writable: false);
        using var mergedPdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        Assert.Equal(3, mergedPdf.PageCount);
        Assert.Equal(400, mergedPdf.Pages[0].Width.Point, precision: 1);
        Assert.Equal(90, mergedPdf.Pages[0].Rotate);
        Assert.Equal(401, mergedPdf.Pages[1].Width.Point, precision: 1);
        Assert.Equal(90, mergedPdf.Pages[1].Rotate);
        Assert.Equal(500, mergedPdf.Pages[2].Width.Point, precision: 1);
        Assert.Equal(180, mergedPdf.Pages[2].Rotate);
    }

    [Fact]
    public void Merge_InvokesAllRegisteredTransformsForEachPage()
    {
        var doc = new PdfDocumentItem
        {
            FileName = "doc.pdf",
            Content = TestPdfFactory.CreatePdf(pageCount: 2),
            PageCount = 2
        };
        var spyTransform = new SpyPageTransform();
        var merger = new PdfSharpMerger([spyTransform]);

        _ = merger.Merge([doc]);

        Assert.Equal(2, spyTransform.CallCount);
    }

    private sealed class SpyPageTransform : IPdfPageTransform
    {
        public int CallCount { get; private set; }

        public void Apply(PdfPage page, PdfDocumentItem sourceDocument) => CallCount++;
    }
}

