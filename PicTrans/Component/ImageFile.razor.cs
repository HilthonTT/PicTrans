using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace PicTrans.Component;

public partial class ImageFile
{
    [Parameter]
    [EditorRequired]
    public IBrowserFile BrowserFile { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedPath { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<IBrowserFile> OnRemoveImage { get; set; }

    private int maxWidth = 800;
    private int maxHeight = 800;
    private bool isCompressedOption = false;
    private string selectedExtension = ".png";
    private string sourcePath = "";
    private List<string> pictureExtensions = new();
    protected override async Task OnInitializedAsync()
    {
        await LoadThumbnailFile();
        pictureExtensions = defaultDataService.GetPictureFormats();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadThumbnailFile();
    }

    private async Task LoadThumbnailFile()
    {
        sourcePath = await imageService.LoadImageFileAsync(BrowserFile);
    }

    private async Task ConvertAndDownload()
    {
        if (isCompressedOption)
        {
            await imageService.CompressImageAndDownloadAsync(BrowserFile, SelectedPath, selectedExtension, maxWidth, maxHeight);
        }
        else
        {
            await imageService.ConvertAndDownloadAsync(BrowserFile, SelectedPath, selectedExtension);
        }

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

    private async Task RemoveImage()
    {
        await OnRemoveImage.InvokeAsync(BrowserFile);
    }
}