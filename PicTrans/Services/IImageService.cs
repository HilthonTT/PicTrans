using Microsoft.AspNetCore.Components.Forms;
using PicTrans.Models;
using Image = SixLabors.ImageSharp.Image;

namespace PicTrans.Services;
public interface IImageService
{
    Task CompressImageAndDownloadAsync(IBrowserFile imageFile, CompressFileModel model);
    Task ConvertImageAsync(IBrowserFile file, string selectedExtension, List<MemoryStream> convertedImages = null);
    Task DownloadFileAsync(IBrowserFile file, MemoryStream convertedImage, string selectedPath, string selectedExtension);
    Task GetEncoderAsync(Image image, MemoryStream convertedStream, string selectedExtension);
    string GetFilePath(IBrowserFile file, string selectedPath, string selectedExtension);
    Task<string> LoadImageFileAsync(IBrowserFile file);
}