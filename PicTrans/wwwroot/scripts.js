
function downloadFile(base64Content, filename) {
    const link = document.createElement('a');
    link.href = "data:application/octet-stream;base64," + base64Content;
    link.download = filename;

    link.click();
}