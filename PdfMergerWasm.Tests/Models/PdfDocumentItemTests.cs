using PdfMergerWasm.Models;

namespace PdfMergerWasm.Tests.Models;

public sealed class PdfDocumentItemTests
{
    [Fact]
    public void NewItem_HasUniqueIdAndZeroRotation()
    {
        var first = CreateItem(100);
        var second = CreateItem(100);

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(0, first.RotationDegrees);
    }

    [Fact]
    public void RotateRight_IncrementsBy90AndWrapsAt360()
    {
        var item = CreateItem(100);

        item.RotateRight();
        Assert.Equal(90, item.RotationDegrees);

        item.RotateRight();
        Assert.Equal(180, item.RotationDegrees);

        item.RotateRight();
        Assert.Equal(270, item.RotationDegrees);

        item.RotateRight();
        Assert.Equal(0, item.RotationDegrees);
    }

    [Fact]
    public void RotateLeft_DecrementsBy90AndWrapsAt360()
    {
        var item = CreateItem(100);

        item.RotateLeft();
        Assert.Equal(270, item.RotationDegrees);

        item.RotateLeft();
        Assert.Equal(180, item.RotationDegrees);

        item.RotateLeft();
        Assert.Equal(90, item.RotationDegrees);

        item.RotateLeft();
        Assert.Equal(0, item.RotationDegrees);
    }

    [Theory]
    [InlineData(512, "512 B")]
    [InlineData(2048, "2.0 KB")]
    [InlineData(2 * 1024 * 1024, "2.00 MB")]
    public void FormattedSize_FormatsBytesKilobytesAndMegabytes(int sizeInBytes, string expectedSuffixFormat)
    {
        var item = CreateItem(sizeInBytes);

        // Normalize decimal separator for locale-independent verification
        var normalized = item.FormattedSize.Replace(',', '.');
        Assert.Equal(expectedSuffixFormat, normalized);
    }

    private static PdfDocumentItem CreateItem(int byteLength) => new()
    {
        FileName = "sample.pdf",
        Content = new byte[byteLength],
        PageCount = 1
    };
}

