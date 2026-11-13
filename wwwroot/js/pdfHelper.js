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

/**
 * Crea un Blob URL temporaneo per visualizzare un PDF in anteprima (iframe/embed).
 * @param {Uint8Array} bytes - Contenuto binario del PDF.
 * @returns {string} Object URL creato dal browser.
 */
export function createPdfObjectUrl(bytes) {
    const blob = new Blob([bytes], { type: "application/pdf" });
    return URL.createObjectURL(blob);
}

/**
 * Rilascia un Blob URL precedentemente creato con createPdfObjectUrl.
 * @param {string} url - Object URL da revocare.
 */
export function revokePdfObjectUrl(url) {
    if (url) {
        URL.revokeObjectURL(url);
    }
}

