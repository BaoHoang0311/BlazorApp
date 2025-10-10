function triggerFileDownload(fileName, url) {
    if (!fileName || !url) {
        console.error("fileName or url is missing");
        return;
    }

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName || '';
    anchorElement.click();
    anchorElement.remove();
}