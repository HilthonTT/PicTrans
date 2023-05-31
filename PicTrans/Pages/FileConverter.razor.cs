using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Pages;

public partial class FileConverter
{
    private const string DefaultFolder = "Download Folder";
    private const string DefaultExtension = ".txt";
    private string selectedFilePath = DefaultFolder;
    private string selectedFileExtension = DefaultExtension;
    private string alertClass = "";
    private string searchText = "";
    private string errorMessage = "";
    private double progress = 0;
    private double bufferValue = 0;
    private List<IBrowserFile> files = new();
    private List<IBrowserFile> initialFiles = new();
    private List<IBrowserFile> downloadedFiles = new();
    private List<string> fileExtensions = new();
    private List<string> folderPaths = new();
    protected override async Task OnInitializedAsync()
    {
        fileExtensions = defaultDataService.GetFileFormats();
        folderPaths = defaultDataService.GetFolderPaths();
        await LoadStates();
    }

    private async Task LoadStates()
    {
        searchText = await secureStorage.GetAsync(nameof(searchText)) ?? "";
        selectedFilePath = await secureStorage.GetAsync(nameof(selectedFilePath)) ?? DefaultFolder;
    }

    private async Task SaveStates()
    {
        await secureStorage.SetAsync(nameof(selectedFileExtension), selectedFileExtension);
        await secureStorage.SetAsync(nameof(selectedFilePath), selectedFilePath);
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
        foreach (var f in files)
        {
            this.files.Add(f);
            CalculateProgress(this.files.Count);
        }

        initialFiles = this.files;
    }

    private async Task ConvertAndDownloadFiles()
    {
        try
        {
            ClearAlert();
            if (files.Count <= 0)
            {
                OpenAlertWarning("No files loaded");
                return;
            }

            progress = 0;
            bufferValue = 0;
            downloadedFiles.Clear();
            await ConvertAndDownloadFile();
        }
        catch (Exception ex)
        {
            OpenAlertError(ex.Message);
        }
    }

    private async Task ConvertAndDownloadFile()
    {
        for (int i = 0; i < files.Count; i++)
        {
            var f = files[i];
            await fileService.DownloadFileAsync(f, selectedFilePath, selectedFileExtension);
            downloadedFiles.Add(f);
            CalculateProgress(downloadedFiles.Count);
        }
    }

    private void RemoveFile(IBrowserFile inputFile)
    {
        files.Remove(inputFile);
    }

    private string GetButtonClass(string path)
    {
        if (selectedFilePath == path)
        {
            return "text-success";
        }

        return "text-danger";
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

    private void ClearFiles()
    {
        files.Clear();
        initialFiles.Clear();
        progress = 0;
        bufferValue = 0;
    }

    private void CalculateProgress(int count)
    {
        if (files.Count == 0)
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