<<<<<<< HEAD
# PomegranatePDF

Applicazione **Blazor WebAssembly (PWA)** per unire, ruotare e riordinare più file PDF direttamente nel browser, senza alcun upload su server esterni. Funziona su qualsiasi sistema operativo moderno (**Windows, macOS, Linux, ChromeOS**).

## Funzionalità principali
- Caricamento di un numero generico di file PDF (in una o più selezioni).
- Riordino dei documenti (sposta su / sposta giù) e rimozione dalla coda.
- Rotazione per singolo documento (`-90°` / `+90°`) con indicatore visivo dell'angolo applicato.
- Unione in un unico PDF con nome file personalizzabile (o generazione automatica con suffisso timestamp se lasciato vuoto).
- Elaborazione **100% locale** lato client tramite WebAssembly e supporto **PWA**.

## Architettura e Best Practice
Il progetto applica i principi **SOLID** e alcuni **Design Pattern**:
- **SRP & ISP:** interfacce piccole e mirate (`IPdfInspector`, `IPdfMerger`, `IPdfPageTransform`, `IFileDownloader`).
- **OCP & Strategy Pattern:** trasformazioni pagina modulari tramite `IPdfPageTransform` (`RotationPageTransform`), orchestrate da `PdfSharpMerger`.
- **DIP:** i componenti Blazor e i servizi applicativi dipendono esclusivamente dalle astrazioni registrate in Dependency Injection (`Program.cs`).
- **Observer Pattern:** `PdfWorkspaceService` gestisce la coda documenti e notifica la UI tramite `StateChanged`.
- **Adapter / JSInterop Isolation:** `BrowserFileDownloader` incapsula il modulo ES isolato `wwwroot/js/pdfHelper.js`.

## Dipendenze e Licenze (Open Source)
| Dipendenza | Scopo | Licenza |
| :--- | :--- | :--- |
| **.NET 9 / Blazor WebAssembly** | Runtime e framework UI client-side | MIT License |
| **PDFsharp** (v6.5.0) | Lettura, rotazione e unione PDF in C# | GNU AGPLv3 |
| **Bootstrap** | Stile e layout responsive | MIT License |

## Licenza
Questo progetto utilizza **PDFsharp v6.5.0** con licenza **GNU AGPLv3** (`PdfSharpLicense.SetAgplLicense()`). Di conseguenza, l'intero codice sorgente di **PomegranatePDF** è rilasciato sotto licenza **GNU AGPLv3**.

## Avvio in locale
Prerequisiti: [.NET 9 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet run
```

Apri quindi il browser all'indirizzo indicato nel terminale.

=======
# PomegranatePDF-
Applicazione per l'unione di più PDF in unico File
>>>>>>> e0007adfd1ce86be44a1d3856880475d0a6c82a2
