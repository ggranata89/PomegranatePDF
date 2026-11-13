/**
 * Crea un Blob dai byte ricevuti da Blazor e avvia il download nel browser.
 * @param {string} fileName - Nome del file da scaricare.
 * @param {string} contentType - MIME type del file (es. application/pdf).
 * @param {Uint8Array} bytes - Contenuto binario del file.
 */
export function downloadFileFromBytes(fileName, contentType, bytes) {
    const blob = new Blob([bytes], { type: contentType });
    const url = URL.createObjectURL(blob);

    try {
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        anchor.style.display = "none";
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
    } finally {
        URL.revokeObjectURL(url);
    }
}

