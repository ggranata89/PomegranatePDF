using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PdfMergerWasm.Services;
using PdfMergerWasm.Services.Abstractions;

namespace PdfMergerWasm.Pages;

/// <summary>
/// Code-behind della pagina principale: coordina l'input utente con i servizi applicativi (SRP / Observer).
/// </summary>
public partial class Home : IDisposable
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024;
    private const int MaxFilesPerSelection = 50;

    [Inject]
    private PdfWorkspaceService Workspace { get; set; } = default!;

    [Inject]
    private IPdfMerger PdfMerger { get; set; } = default!;

    [Inject]
    private IFileDownloader FileDownloader { get; set; } = default!;

    private string OutputFileName { get; set; } = string.Empty;
    private string? ErrorMessage { get; set; }
    private string? StatusMessage { get; set; }
    private bool IsBusy { get; set; }

    /// <summary>
    /// Sottoscrive gli aggiornamenti dello stato del workspace all'inizializzazione del componente.
    /// </summary>
    protected override void OnInitialized()
    {
        Workspace.StateChanged += OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Rimuove la sottoscrizione allo stato del workspace per evitare memory leak.
    /// </summary>
    public void Dispose()
    {
        Workspace.StateChanged -= OnWorkspaceStateChanged;
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
    /// Unisce i PDF nell'ordine corrente, applica le rotazioni e avvia il download del risultato.
    /// </summary>
    private async Task MergeAndDownloadAsync()
    {
        ResetFeedback();
        IsBusy = true;

        try
        {
            var mergedBytes = PdfMerger.Merge(Workspace.Documents);
            var safeFileName = ResolveOutputFileName(OutputFileName);
            await FileDownloader.DownloadAsync(safeFileName, mergedBytes);
            StatusMessage = $"PDF '{safeFileName}' generato ({Workspace.TotalPages} pagine totali).";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore durante l'unione dei PDF: {ex.Message}";
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

