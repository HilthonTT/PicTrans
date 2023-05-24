using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Services;
public interface IImageService
{
    string GetDefaultDownloadPath(IBrowserFile file, string selectedExtension);
    Task GetEncoderAsync(SixLabors.ImageSharp.Image image, MemoryStream convertedStream, string selectedExtension);
    List<string> GetFolderPaths();
    List<string> GetPictureFormats();
    string GetSelectedPath(IBrowserFile file, string selectedExtension, string path);
}