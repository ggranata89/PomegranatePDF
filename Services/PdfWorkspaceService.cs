using PomegranatePDF.Models;
using PomegranatePDF.Services.Abstractions;

namespace PomegranatePDF.Services;

/// <summary>
/// Gestisce lo stato della coda di documenti PDF e notifica i subscriber (Observer Pattern / SRP).
/// </summary>
public sealed class PdfWorkspaceService
{
    private readonly List<PdfDocumentItem> _documents = [];
    private readonly IPdfInspector _inspector;

    /// <summary>
    /// Evento notificato ogni volta che la lista dei documenti o il loro stato cambia.
    /// </summary>
    public event Action? StateChanged;

    /// <summary>
    /// Restituisce la vista in sola lettura dei documenti attualmente in coda.
    /// </summary>
    public IReadOnlyList<PdfDocumentItem> Documents => _documents;

    /// <summary>
    /// Restituisce il numero totale di pagine risultante dall'unione corrente.
    /// </summary>
    public int TotalPages => _documents.Sum(d => d.PageCount);

    /// <summary>
    /// Inizializza il workspace con il servizio di ispezione PDF.
    /// </summary>
    public PdfWorkspaceService(IPdfInspector inspector)
    {
        ArgumentNullException.ThrowIfNull(inspector);
        _inspector = inspector;
    }

    /// <summary>
    /// Valida un PDF tramite <see cref="IPdfInspector"/> e lo aggiunge in coda.
    /// </summary>
    public void AddDocument(string fileName, byte[] content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var pageCount = _inspector.GetPageCount(content);
        _documents.Add(new PdfDocumentItem
        {
            FileName = fileName,
            Content = content,
            PageCount = pageCount
        });

        NotifyStateChanged();
    }

    /// <summary>
    /// Rimuove il documento con l'identificativo specificato dalla coda.
    /// </summary>
    public void RemoveDocument(Guid id)
    {
        var index = FindIndexById(id);
        if (index < 0)
        {
            return;
        }

        _documents.RemoveAt(index);
        NotifyStateChanged();
    }

    /// <summary>
    /// Sposta di una posizione verso l'alto il documento specificato.
    /// </summary>
    public void MoveUp(Guid id)
    {
        var index = FindIndexById(id);
        if (index <= 0)
        {
            return;
        }

        SwapDocuments(index, index - 1);
        NotifyStateChanged();
    }

    /// <summary>
    /// Sposta di una posizione verso il basso il documento specificato.
    /// </summary>
    public void MoveDown(Guid id)
    {
        var index = FindIndexById(id);
        if (index < 0 || index >= _documents.Count - 1)
        {
            return;
        }

        SwapDocuments(index, index + 1);
        NotifyStateChanged();
    }

    /// <summary>
    /// Ruota di 90 gradi in senso orario il documento specificato.
    /// </summary>
    public void RotateRight(Guid id)
    {
        var item = FindById(id);
        if (item is null)
        {
            return;
        }

        item.RotateRight();
        NotifyStateChanged();
    }

    /// <summary>
    /// Ruota di 90 gradi in senso antiorario il documento specificato.
    /// </summary>
    public void RotateLeft(Guid id)
    {
        var item = FindById(id);
        if (item is null)
        {
            return;
        }

        item.RotateLeft();
        NotifyStateChanged();
    }

    /// <summary>
    /// Svuota l'intera coda dei documenti caricati.
    /// </summary>
    public void Clear()
    {
        if (_documents.Count == 0)
        {
            return;
        }

        _documents.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Cerca l'indice di un documento nella coda tramite il suo identificativo.
    /// </summary>
    private int FindIndexById(Guid id) => _documents.FindIndex(d => d.Id == id);

    /// <summary>
    /// Restituisce il documento corrispondente all'identificativo oppure <c>null</c>.
    /// </summary>
    private PdfDocumentItem? FindById(Guid id) => _documents.FirstOrDefault(d => d.Id == id);

    /// <summary>
    /// Scambia la posizione di due documenti nella coda.
    /// </summary>
    private void SwapDocuments(int firstIndex, int secondIndex)
    {
        (_documents[firstIndex], _documents[secondIndex]) = (_documents[secondIndex], _documents[firstIndex]);
    }

    /// <summary>
    /// Notifica tutti i componenti in ascolto che lo stato del workspace è cambiato.
    /// </summary>
    private void NotifyStateChanged() => StateChanged?.Invoke();
}

