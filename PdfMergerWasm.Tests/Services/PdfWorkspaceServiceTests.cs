using PdfMergerWasm.Services;
using PdfMergerWasm.Services.Abstractions;

namespace PdfMergerWasm.Tests.Services;

public sealed class PdfWorkspaceServiceTests
{
    [Fact]
    public void Constructor_WhenInspectorIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PdfWorkspaceService(null!));
    }

    [Fact]
    public void AddDocument_AddsDocumentUpdatesTotalPagesAndNotifies()
    {
        var sut = CreateWorkspace(pageCountPerCall: 3);
        var notifications = 0;
        sut.StateChanged += () => notifications++;

        sut.AddDocument("a.pdf", [1, 2, 3]);
        sut.AddDocument("b.pdf", [4, 5]);

        Assert.Equal(2, sut.Documents.Count);
        Assert.Equal("a.pdf", sut.Documents[0].FileName);
        Assert.Equal("b.pdf", sut.Documents[1].FileName);
        Assert.Equal(6, sut.TotalPages);
        Assert.Equal(2, notifications);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddDocument_WhenFileNameIsWhitespace_ThrowsArgumentException(string fileName)
    {
        var sut = CreateWorkspace();
        Assert.Throws<ArgumentException>(() => sut.AddDocument(fileName, [1]));
    }

    [Fact]
    public void AddDocument_WhenContentIsNull_ThrowsArgumentNullException()
    {
        var sut = CreateWorkspace();
        Assert.Throws<ArgumentNullException>(() => sut.AddDocument("a.pdf", null!));
    }

    [Fact]
    public void RemoveDocument_RemovesExistingAndIgnoresUnknownId()
    {
        var sut = CreateWorkspace();
        sut.AddDocument("a.pdf", [1]);
        sut.AddDocument("b.pdf", [2]);
        var firstId = sut.Documents[0].Id;

        var notifications = 0;
        sut.StateChanged += () => notifications++;

        sut.RemoveDocument(Guid.NewGuid());
        Assert.Equal(2, sut.Documents.Count);
        Assert.Equal(0, notifications);

        sut.RemoveDocument(firstId);
        Assert.Single(sut.Documents);
        Assert.Equal("b.pdf", sut.Documents[0].FileName);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void MoveUpAndMoveDown_ReorderDocumentsAndRespectBoundaries()
    {
        var sut = CreateWorkspace();
        sut.AddDocument("first.pdf", [1]);
        sut.AddDocument("second.pdf", [2]);
        var firstId = sut.Documents[0].Id;
        var secondId = sut.Documents[1].Id;

        var notifications = 0;
        sut.StateChanged += () => notifications++;

        // Boundary no-ops
        sut.MoveUp(firstId);
        sut.MoveDown(secondId);
        Assert.Equal(0, notifications);

        // Move second up
        sut.MoveUp(secondId);
        Assert.Equal(["second.pdf", "first.pdf"], sut.Documents.Select(d => d.FileName));
        Assert.Equal(1, notifications);

        // Move second back down
        sut.MoveDown(secondId);
        Assert.Equal(["first.pdf", "second.pdf"], sut.Documents.Select(d => d.FileName));
        Assert.Equal(2, notifications);
    }

    [Fact]
    public void RotateRightAndRotateLeft_UpdateDocumentRotationAndNotify()
    {
        var sut = CreateWorkspace();
        sut.AddDocument("doc.pdf", [1]);
        var id = sut.Documents[0].Id;

        var notifications = 0;
        sut.StateChanged += () => notifications++;

        sut.RotateRight(id);
        Assert.Equal(90, sut.Documents[0].RotationDegrees);

        sut.RotateLeft(id);
        Assert.Equal(0, sut.Documents[0].RotationDegrees);

        sut.RotateRight(Guid.NewGuid());
        sut.RotateLeft(Guid.NewGuid());

        Assert.Equal(2, notifications);
    }

    [Fact]
    public void Clear_EmptiesDocumentsAndNotifiesOnlyWhenNonEmpty()
    {
        var sut = CreateWorkspace();
        var notifications = 0;
        sut.StateChanged += () => notifications++;

        sut.Clear();
        Assert.Equal(0, notifications);

        sut.AddDocument("doc.pdf", [1]);
        sut.Clear();

        Assert.Empty(sut.Documents);
        Assert.Equal(0, sut.TotalPages);
        Assert.Equal(2, notifications);
    }

    private static PdfWorkspaceService CreateWorkspace(int pageCountPerCall = 1) =>
        new(new StubPdfInspector(pageCountPerCall));

    private sealed class StubPdfInspector(int pageCount) : IPdfInspector
    {
        public int GetPageCount(byte[] pdfBytes) => pageCount;
    }
}

