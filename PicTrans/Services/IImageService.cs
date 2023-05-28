using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Services;
public interface IImageService
{
    Task CompressImageAndDownloadAsync(IBrowserFile imageFile, string selectedPath, string fileExtension, int maxWidth = 800, int maxHeight = 800);
    Task ConvertAndDownloadAsync(IBrowserFile file, string selectedPath, string selectedExtension);
    Task<string> LoadImageFileAsync(IBrowserFile file);
}