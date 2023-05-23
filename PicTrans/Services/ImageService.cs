using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

namespace PicTrans.Services;
public class ImageService : IImageService
{
    public async Task GetEncoderAsync(SixLabors.ImageSharp.Image image,
                                  MemoryStream convertedStream,
                                  string selectedExtension)
    {
        switch (selectedExtension)
        {
            case ".png":
                await image.SaveAsPngAsync(convertedStream, new PngEncoder());
                break;

            case ".jpeg":
                await image.SaveAsJpegAsync(convertedStream, new JpegEncoder());
                break;

            case ".jpg":
                await image.SaveAsJpegAsync(convertedStream, new JpegEncoder());
                break;

            case ".gif":
                await image.SaveAsGifAsync(convertedStream, new GifEncoder());
                break;

            case ".bmp":
                await image.SaveAsBmpAsync(convertedStream, new BmpEncoder());
                break;

            case ".tiff":
                await image.SaveAsTiffAsync(convertedStream, new TiffEncoder());
                break;

            case ".webp":
                await image.SaveAsWebpAsync(convertedStream, new WebpEncoder());
                break;

            default:
                throw new NotImplementedException();
        }
    }

    public List<string> GetPictureFormats()
    {
        List<string> pictureExtensions = new()
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".tiff",
            ".webp",
        };

        return pictureExtensions;
    }

    public List<string> GetFolderPaths()
    {
        return new List<string>()
        {
            "Download Folder",
            "Picture Folder",
            "Document Folder",
            "Video Folder",
            "Desktop",
        };
    }

    public string GetDefaultDownloadPath(IBrowserFile file, string selectedExtension)
    {
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = Path.GetFileNameWithoutExtension(file.Name) + selectedExtension;
        return Path.Combine(downloadsFolder, "Downloads", fileName);
    }

    public string GetPicturesDownloadPath(IBrowserFile file, string selectedExtension)
    {
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        string fileName = Path.GetFileNameWithoutExtension(file.Name) + selectedExtension;
        return Path.Combine(downloadsFolder, fileName);
    }
}
