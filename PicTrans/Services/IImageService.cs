using Microsoft.AspNetCore.Components.Forms;
using Image = SixLabors.ImageSharp.Image;

namespace PicTrans.Services;
public interface IImageService
{
    Task CompressImageAndDownloadAsync(
        IBrowserFile imageFile,
        string selectedPath,
        string fileExtension,
        int maxWidth = 800,
        int maxHeight = 800);
    Task ConvertImageAsync(IBrowserFile file, string selectedExtension, List<MemoryStream> convertedImages = null);
    Task DownloadFileAsync(IBrowserFile file, MemoryStream convertedImage, string selectedPath, string selectedExtension);
    string GetFilePath(IBrowserFile file, string selectedPath, string selectedExtension);
    Task<string> LoadImageFileAsync(IBrowserFile file);
}