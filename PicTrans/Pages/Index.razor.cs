using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using PicTrans.Component;

namespace PicTrans.Pages;

public partial class Index
{
    private const string DefaultFolder = "Download Folder";
    private const string DefaultExtension = ".png";
    private IDialogReference dialogReference;
    private string searchText = "";
    private string selectedExtension = DefaultExtension;
    private string selectedPath = DefaultFolder;
    private string errorMessage = "";
    private string alertClass = "";
    private int loadedFilesCount = 0;
    private int maxWidth = 800;
    private int maxHeight = 800;
    private double progress = 0;
    private double bufferValue = 0;
    private bool isCompressedOption = false;
    private List<IBrowserFile> files = new();
    private List<IBrowserFile> initialFiles = new();
    private List<IBrowserFile> downloadedFiles = new();
    private List<string> pictureExtensions = new();
    private List<string> folderPaths = new();
    protected override async Task OnInitializedAsync()
    {
        pictureExtensions = defaultDataService.GetPictureFormats();
        folderPaths = defaultDataService.GetFolderPaths();
        await LoadStates();
    }

    private async Task LoadStates()
    {
        selectedExtension = await secureStorage.GetAsync(nameof(selectedExtension)) ?? DefaultExtension;
        searchText = await secureStorage.GetAsync(nameof(searchText)) ?? "";
        selectedPath = await secureStorage.GetAsync(nameof(selectedPath)) ?? DefaultFolder;
    }

    private async Task SaveStates()
    {
        await secureStorage.SetAsync(nameof(selectedExtension), selectedExtension);
        await secureStorage.SetAsync(nameof(selectedPath), selectedPath);
        if (string.IsNullOrWhiteSpace(searchText))
        {
            secureStorage.Remove(nameof(searchText));
        }
        else
        {
            await secureStorage.SetAsync(nameof(searchText), searchText);
        }
    }

    private async Task FilterImages()
    {
        var output = initialFiles;
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(f => f.Name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        files = output;
        await SaveStates();
    }

    private async Task<IEnumerable<string>> SearchImage(string value)
    {
        searchText = value;
        var output = initialFiles;
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(f => f.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        await SaveStates();
        return output.Select(f => f.Name);
    }

    private void UploadFiles(IReadOnlyList<IBrowserFile> files)
    {
        initialFiles.Clear();
        this.files.Clear();
        foreach (var f in files)
        {
            this.files.Add(f);
            loadedFilesCount = this.files.Count;
            CalculateProgress(loadedFilesCount);
        }

        initialFiles = this.files;
    }

    private async Task ConvertAndDownloadImage()
    {
        try
        {
            ClearAlert();
            if (files.Count <= 0)
            {
                OpenAlertWarning("No files to download.");
                return;
            }

            progress = 0;
            bufferValue = 0;
            downloadedFiles.Clear();
            if (isCompressedOption)
            {
                await ConvertAndDownloadWithCompressing();
            }
            else
            {
                await ConvertAndDownloadWithoutCompressing();
            }

            OpenDialog($"Successfully downloaded images to {selectedPath}");
            await SaveStates();
        }
        catch (Exception ex)
        {
            OpenAlertError(ex.Message);
        }
    }

    private async Task ConvertAndDownloadWithoutCompressing()
    {
        for (int i = 0; i < files.Count; i++)
        {
            var f = files[i];
            await imageService.ConvertAndDownloadAsync(f, selectedPath, selectedExtension);
            downloadedFiles.Add(f);
            CalculateProgress(downloadedFiles.Count);
        }
    }

    private async Task ConvertAndDownloadWithCompressing()
    {
        for (int i = 0; i < files.Count; i++)
        {
            var f = files[i];
            await imageService.CompressImageAndDownloadAsync(f, selectedPath, selectedExtension, maxWidth, maxHeight);
            downloadedFiles.Add(f);
            CalculateProgress(downloadedFiles.Count);
        }
    }

    private void RemoveImageFile(IBrowserFile imageFile)
    {
        files.Remove(imageFile);
    }

    private string GetButtonClass(string path)
    {
        if (selectedPath == path)
        {
            return "text-success";
        }

        return "text-danger";
    }

    private void GetAlertSeverity(string alert)
    {
        string output = alert switch
        {
            "Normal" => "Severity.Normal",
            "Info" => "Severity.Info",
            "Success" => "Severity.Success",
            "Warning" => "Severity.Warning",
            "Error" => "Severity.Error",
            _ => "Severity.Info",
        };
        alertClass = alert;
    }

    private string GetFilesCount()
    {
        string output = files?.Count switch
        {
            0 => "0 files",
            1 => "1 file",
            _ => $"{files?.Count} files",
        };
        return output;
    }

    private void ClearImages()
    {
        files.Clear();
        initialFiles.Clear();
        downloadedFiles.Clear();
        loadedFilesCount = 0;
        progress = 0;
        bufferValue = 0;
    }

    private void CalculateProgress(int count)
    {
        if (files.Count <= 0)
        {
            progress = 0;
            bufferValue = 0;
        }
        else
        {
            progress = (double)count / files.Count * 100;
            bufferValue = progress + 5;
        }

        StateHasChanged();
    }

    private void OpenDialog(string title)
    {
        var topCenter = new DialogOptions()
        {
            Position = DialogPosition.TopCenter,
            ClassBackground = "backdrop-blur"
        };
        dialogReference = dialogService.Show<Dialog>(title, topCenter);
    }

    private void ClearAlert()
    {
        GetAlertSeverity("");
        errorMessage = "";
    }

    private void OpenAlertWarning(string message)
    {
        GetAlertSeverity("Warning");
        errorMessage = message;
    }

    private void OpenAlertError(string message)
    {
        GetAlertSeverity("Error");
        errorMessage = message;
    }
}