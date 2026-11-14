using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PomegranatePDF.Models;
using PomegranatePDF.Services;
using PomegranatePDF.Services.Abstractions;

namespace PomegranatePDF.Pages;

/// <summary>
/// Code-behind della pagina principale: guida l'utente attraverso un wizard a 3 step
/// con anteprima dei singoli file caricati e del PDF finale unito.
/// </summary>
public partial class Home : IAsyncDisposable
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024;
    private const int MaxFilesPerSelection = 50;

    private enum WizardStep
    {
        Upload = 1,
        Arrange = 2,
        PreviewAndDownload = 3
    }

    [Inject]
    private PdfWorkspaceService Workspace { get; set; } = default!;

    [Inject]
    private IPdfMerger PdfMerger { get; set; } = default!;

    [Inject]
    private IFileDownloader FileDownloader { get; set; } = default!;

    [Inject]
    private IPdfPreviewProvider PreviewProvider { get; set; } = default!;

    private WizardStep CurrentStep { get; set; } = WizardStep.Upload;
    private string OutputFileName { get; set; } = string.Empty;
    private string? ErrorMessage { get; set; }
    private string? StatusMessage { get; set; }
    private bool IsBusy { get; set; }

    // Stato anteprima singolo documento caricato
    private Guid? PreviewedDocumentId { get; set; }
    private string? DocumentPreviewUrl { get; set; }
    private string? PreviewedDocumentName { get; set; }

    // Stato anteprima e byte del PDF finale unito (Step 3)
    private byte[]? MergedOutputBytes { get; set; }
    private string? OutputPreviewUrl { get; set; }

    /// <summary>
    /// Sottoscrive gli aggiornamenti dello stato del workspace all'inizializzazione del componente.
    /// </summary>
    protected override void OnInitialized()
    {
        Workspace.StateChanged += OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Rilascia la sottoscrizione al workspace e gli eventuali Blob URL di anteprima attivi.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Workspace.StateChanged -= OnWorkspaceStateChanged;
        await ClearDocumentPreviewAsync();
        await ClearOutputPreviewAsync();
    }

    /// <summary>
    /// Gestisce la selezione di uno o più file PDF dall'input HTML.
    /// </summary>
    private async Task HandleFilesSelectedAsync(InputFileChangeEventArgs e)
    {
        ResetFeedback();
        IsBusy = true;

        try
        {
            var files = e.GetMultipleFiles(MaxFilesPerSelection);
            foreach (var file in files)
            {
                await LoadSingleFileAsync(file);
            }

            if (files.Count > 0 && ErrorMessage is null)
            {
                StatusMessage = $"Caricati {files.Count} file PDF.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Apre o chiude l'anteprima inline di un documento caricato.
    /// </summary>
    private async Task ToggleDocumentPreviewAsync(PdfDocumentItem document)
    {
        if (PreviewedDocumentId == document.Id)
        {
            await ClearDocumentPreviewAsync();
            return;
        }

        await ClearDocumentPreviewAsync();
        DocumentPreviewUrl = await PreviewProvider.CreatePreviewUrlAsync(document.Content);
        PreviewedDocumentId = document.Id;
        PreviewedDocumentName = document.FileName;
    }

    /// <summary>
    /// Rimuove un documento dalla coda e chiude la sua eventuale anteprima attiva.
    /// </summary>
    private async Task RemoveDocumentAsync(Guid documentId)
    {
        if (PreviewedDocumentId == documentId)
        {
            await ClearDocumentPreviewAsync();
        }

        Workspace.RemoveDocument(documentId);

        if (Workspace.Documents.Count == 0)
        {
            await ClearOutputPreviewAsync();
            CurrentStep = WizardStep.Upload;
        }
    }

    /// <summary>
    /// Svuota l'intera lista documenti e riporta il wizard al primo step.
    /// </summary>
    private async Task ClearWorkspaceAsync()
    {
        ResetFeedback();
        await ClearDocumentPreviewAsync();
        await ClearOutputPreviewAsync();
        Workspace.Clear();
        OutputFileName = string.Empty;
        CurrentStep = WizardStep.Upload;
    }

    /// <summary>
    /// Passa allo step indicato, generando l'anteprima finale quando si entra nello step 3.
    /// </summary>
    private async Task GoToStepAsync(WizardStep targetStep)
    {
        if (IsBusy || targetStep == CurrentStep)
        {
            return;
        }

        if (targetStep != WizardStep.Upload && Workspace.Documents.Count == 0)
        {
            return;
        }

        ResetFeedback();

        if (targetStep == WizardStep.PreviewAndDownload)
        {
            await GenerateOutputPreviewAsync();
            return;
        }

        await ClearOutputPreviewAsync();
        CurrentStep = targetStep;
    }

    /// <summary>
    /// Genera il PDF unito in memoria e crea il relativo URL per l'anteprima nello step 3.
    /// </summary>
    private async Task GenerateOutputPreviewAsync()
    {
        IsBusy = true;

        try
        {
            await ClearDocumentPreviewAsync();
            await ClearOutputPreviewAsync();

            MergedOutputBytes = PdfMerger.Merge(Workspace.Documents);
            OutputPreviewUrl = await PreviewProvider.CreatePreviewUrlAsync(MergedOutputBytes);
            CurrentStep = WizardStep.PreviewAndDownload;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore durante la generazione dell'anteprima finale: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Avvia il download del PDF già unito e visualizzato in anteprima nello step 3.
    /// </summary>
    private async Task DownloadMergedPdfAsync()
    {
        if (MergedOutputBytes is null)
        {
            return;
        }

        ResetFeedback();
        IsBusy = true;

        try
        {
            var safeFileName = ResolveOutputFileName(OutputFileName);
            await FileDownloader.DownloadAsync(safeFileName, MergedOutputBytes);
            StatusMessage = $"PDF '{safeFileName}' scaricato ({Workspace.TotalPages} pagine totali).";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore durante il download del PDF: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Legge un singolo file caricato in memoria e lo registra nel workspace.
    /// </summary>
    private async Task LoadSingleFileAsync(IBrowserFile file)
    {
        try
        {
            var bytes = await ReadAllBytesAsync(file);
            Workspace.AddDocument(file.Name, bytes);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossibile caricare '{file.Name}': {ex.Message}";
        }
    }

    /// <summary>
    /// Legge lo stream del file browser rispettando il limite massimo di dimensione.
    /// </summary>
    private static async Task<byte[]> ReadAllBytesAsync(IBrowserFile file)
    {
        await using var stream = file.OpenReadStream(MaxFileSizeBytes);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Revoca il Blob URL dell'anteprima documento corrente, se presente.
    /// </summary>
    private async Task ClearDocumentPreviewAsync()
    {
        if (DocumentPreviewUrl is not null)
        {
            await PreviewProvider.RevokePreviewUrlAsync(DocumentPreviewUrl);
            DocumentPreviewUrl = null;
        }

        PreviewedDocumentId = null;
        PreviewedDocumentName = null;
    }

    /// <summary>
    /// Revoca il Blob URL dell'anteprima finale e libera i byte generati in precedenza.
    /// </summary>
    private async Task ClearOutputPreviewAsync()
    {
        if (OutputPreviewUrl is not null)
        {
            await PreviewProvider.RevokePreviewUrlAsync(OutputPreviewUrl);
            OutputPreviewUrl = null;
        }

        MergedOutputBytes = null;
    }

    /// <summary>
    /// Formatta la dimensione in byte in formato leggibile.
    /// </summary>
    private static string FormatByteSize(int byteCount) => byteCount switch
    {
        < 1024 => $"{byteCount} B",
        < 1024 * 1024 => $"{byteCount / 1024.0:F1} KB",
        _ => $"{byteCount / (1024.0 * 1024.0):F2} MB"
    };

    /// <summary>
    /// Usa il nome scelto dall'utente (aggiungendo .pdf se mancante) oppure genera un nome univoco con timestamp se vuoto.
    /// </summary>
    private static string ResolveOutputFileName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return $"documento-unito_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        }

        var trimmed = rawName.Trim();
        return trimmed.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"{trimmed}.pdf";
    }

    /// <summary>
    /// Azzera eventuali messaggi di stato o errore precedenti.
    /// </summary>
    private void ResetFeedback()
    {
        ErrorMessage = null;
        StatusMessage = null;
    }

    /// <summary>
    /// Callback invocata dal workspace quando la lista documenti cambia (Observer Pattern).
    /// </summary>
    private void OnWorkspaceStateChanged() => InvokeAsync(StateHasChanged);
}
