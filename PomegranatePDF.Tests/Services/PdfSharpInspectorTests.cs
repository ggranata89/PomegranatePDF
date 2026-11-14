using PomegranatePDF.Services;

namespace PomegranatePDF.Tests.Services;

public sealed class PdfSharpInspectorTests
{
    private readonly PdfSharpInspector _sut = new();

    [Fact]
    public void GetPageCount_WhenBytesAreNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.GetPageCount(null!));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void GetPageCount_ReturnsExactNumberOfPages(int expectedPages)
    {
        var pdfBytes = TestPdfFactory.CreatePdf(expectedPages);

        var actualPages = _sut.GetPageCount(pdfBytes);

        Assert.Equal(expectedPages, actualPages);
    }
}

