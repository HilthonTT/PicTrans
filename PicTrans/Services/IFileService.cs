using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Services;
public interface IFileService
{
    Task DownloadFileAsync(IBrowserFile file, string selectedPath, string selectedExtension);
}