using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace PicTrans.Component;

public partial class File
{
    [Parameter]
    [EditorRequired]
    public IBrowserFile BrowserFile { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedPath { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<IBrowserFile> OnRemoveFile { get; set; }

    private string selectedExtension = ".txt";
    private List<string> fileExtensions = new();
    protected override void OnInitialized()
    {
        fileExtensions = defaultDataService.GetFileFormats();
    }

    private async Task ConvertAndDownloadFile()
    {
        await fileService.DownloadFileAsync(BrowserFile, SelectedPath, selectedExtension);
        OpenDialog($"Successfully downloaded {BrowserFile.Name}");
    }

    private void OpenDialog(string title)
    {
        var topCenter = new DialogOptions()
        {
            Position = DialogPosition.TopCenter,
            ClassBackground = "backdrop-blur"
        };
        dialogService.Show<Dialog>(title, topCenter);
    }

    private async Task RemoveFile()
    {
        await OnRemoveFile.InvokeAsync(BrowserFile);
    }
}