using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Services;
public interface IImageService
{
    Task ConvertImageAsync(IBrowserFile file, string selectedExtension, List<MemoryStream> convertedImages = null);
    Task DownloadFileAsync(IBrowserFile file, MemoryStream convertedImage, string selectedPath, string selectedExtension);
    Task GetEncoderAsync(SixLabors.ImageSharp.Image image, MemoryStream convertedStream, string selectedExtension);
    string GetFilePath(IBrowserFile file, string selectedPath, string selectedExtension);
    Task<string> LoadImageFileAsync(IBrowserFile file);
}