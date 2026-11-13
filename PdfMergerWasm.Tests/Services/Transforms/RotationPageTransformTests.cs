using PdfMergerWasm.Models;
using PdfMergerWasm.Services.Transforms;
using PdfSharp.Pdf;

namespace PdfMergerWasm.Tests.Services.Transforms;

public sealed class RotationPageTransformTests
{
    private readonly RotationPageTransform _sut = new();

    [Fact]
    public void Apply_WhenPageIsNull_ThrowsArgumentNullException()
    {
        var item = CreateItem();
        Assert.Throws<ArgumentNullException>(() => _sut.Apply(null!, item));
    }

    [Fact]
    public void Apply_WhenSourceDocumentIsNull_ThrowsArgumentNullException()
    {
        using var document = new PdfDocument();
        var page = document.AddPage();

        Assert.Throws<ArgumentNullException>(() => _sut.Apply(page, null!));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(90, 0, 90)]
    [InlineData(90, 1, 180)]
    [InlineData(270, 2, 90)]
    public void Apply_AddsConfiguredRotationAndWrapsAt360(int initialRotate, int rotateRightSteps, int expectedRotate)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        page.Rotate = initialRotate;

        var item = CreateItem();
        for (var i = 0; i < rotateRightSteps; i++)
        {
            item.RotateRight();
        }

        _sut.Apply(page, item);

        Assert.Equal(expectedRotate, page.Rotate);
    }

    private static PdfDocumentItem CreateItem() => new()
    {
        FileName = "doc.pdf",
        Content = [1, 2, 3],
        PageCount = 1
    };
}

