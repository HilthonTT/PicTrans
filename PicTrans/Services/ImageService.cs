using Microsoft.AspNetCore.Components.Forms;
using PicTrans.Models;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;
using ResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;

namespace PicTrans.Services;
public class ImageService : IImageService
{
    private const long MaxFileSize = 1024 * 1024 * 500; // represents 500MB

    public async Task<string> LoadImageFileAsync(IBrowserFile file)
    {
        using var stream = file.OpenReadStream(MaxFileSize);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] bytes = memoryStream.ToArray();
        return $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
    }

    public async Task CompressImageAndDownloadAsync(IBrowserFile imageFile, CompressFileModel model)
    {
        using var imageStream = imageFile.OpenReadStream(MaxFileSize);
        using var image = await Image.LoadAsync(imageStream);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(image.Width, image.Height),
            Mode = ResizeMode.Max,
        }));

        using var compressedStream = new MemoryStream();
        await GetEncoderAsync(image, compressedStream, model.FileExtension);
        await DownloadFileAsync(imageFile, compressedStream, model.SelectedPath, model.FileExtension);
    }

    public async Task DownloadFileAsync(IBrowserFile file,
                                        MemoryStream convertedImage,
                                        string selectedPath,
                                        string selectedExtension)
    {
        string fileName = Path.GetFileNameWithoutExtension(file.Name);
        string filePath = GetFilePath(file, selectedPath, selectedExtension);
        using var outputStream = new FileStream(filePath, FileMode.Create);
        convertedImage.Position = 0;
        await convertedImage.CopyToAsync(outputStream);
    }

    public async Task ConvertImageAsync(IBrowserFile file, string selectedExtension, List<MemoryStream> convertedImages = null)
    {
        using var stream = file.OpenReadStream(MaxFileSize);
        using var image = await Image.LoadAsync(stream);
        using var convertedStream = new MemoryStream();
        await GetEncoderAsync(image, convertedStream, selectedExtension);
        if (convertedImages is not null)
        {
            convertedStream.Position = 0;

            convertedImages.Add(new MemoryStream(convertedStream.ToArray()));
        }
    }

    public string GetFilePath(IBrowserFile file, string selectedPath, string selectedExtension)
    {
        return selectedPath switch
        {
            "Download Folder" => GetDefaultDownloadPath(file, selectedExtension),
            _ => GetSelectedPath(file, selectedExtension, selectedPath),
        };
    }

    public async Task GetEncoderAsync(Image image,
                                  MemoryStream convertedStream,
                                  string selectedExtension)
    {
        switch (selectedExtension)
        {
            case ".png":
                await image.SaveAsPngAsync(convertedStream, new PngEncoder());
                break;

            case ".jpeg":
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

    private static string GetDefaultDownloadPath(IBrowserFile file, string selectedExtension)
    {
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = Path.GetFileNameWithoutExtension(file.Name) + selectedExtension;
        return Path.Combine(downloadsFolder, "Downloads", fileName);
    }

    private static string GetSelectedPath(IBrowserFile file, string selectedExtension, string path)
    {
        string downloadsFolder = Environment.GetFolderPath(GetFolder(path));
        string fileName = Path.GetFileNameWithoutExtension(file.Name) + selectedExtension;
        return Path.Combine(downloadsFolder, fileName);
    }

    private static Environment.SpecialFolder GetFolder(string path)
    {
        return path switch
        {
            "Picture Folder" => Environment.SpecialFolder.MyPictures,
            "Document Folder" => Environment.SpecialFolder.MyDocuments,
            "Video Folder" => Environment.SpecialFolder.MyVideos,
            "Desktop" => Environment.SpecialFolder.Desktop,
            _ => throw new NotImplementedException(),
        };
    }
}
