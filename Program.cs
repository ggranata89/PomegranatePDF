using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PdfMergerWasm;
using PdfMergerWasm.Services;
using PdfMergerWasm.Services.Abstractions;
using PdfMergerWasm.Services.Transforms;
using PdfSharp;

// Dichiarazione esplicita della licenza AGPLv3 richiesta da PDFsharp 6.5+
PdfSharpLicense.SetAgplLicense();

// Configurazione dell'host Blazor WebAssembly eseguito interamente nel browser
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient predefinito per eventuali richieste verso l'origine dell'app
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Ispezione e validazione dei PDF caricati (SRP / ISP)
builder.Services.AddScoped<IPdfInspector, PdfSharpInspector>();

// Strategie di trasformazione delle pagine durante il merge (OCP / Strategy Pattern)
builder.Services.AddScoped<IPdfPageTransform, RotationPageTransform>();

// Motore di unione PDF che orchestra le trasformazioni registrate
builder.Services.AddScoped<IPdfMerger, PdfSharpMerger>();

// Adapter per il download del file generato tramite JSInterop isolato (DIP)
builder.Services.AddScoped<IFileDownloader, BrowserFileDownloader>();

// Gestore centralizzato dello stato della coda documenti con notifica ai subscriber (Observer Pattern)
builder.Services.AddScoped<PdfWorkspaceService>();

await builder.Build().RunAsync();
