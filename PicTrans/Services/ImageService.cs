using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Pbm;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;
using ResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;

namespace PicTrans.Services;
public class ImageService : IImageService
{
    private const long MaxFileSize = 1024 * 1024 * 500; // represents 500MB
    private readonly IPathService _pathService;

    public ImageService(IPathService pathService)
    {
        _pathService = pathService;
    }

    public async Task<string> LoadImageFileAsync(IBrowserFile file)
    {
        using var stream = file.OpenReadStream(MaxFileSize);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        byte[] bytes = memoryStream.ToArray();
        return $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
    }

    public async Task CompressImageAndDownloadAsync(
        IBrowserFile imageFile,
        string selectedPath,
        string fileExtension,
        int maxWidth = 800, 
        int maxHeight = 800)
    {
        using var imageStream = imageFile.OpenReadStream(MaxFileSize);
        using var image = await Image.LoadAsync(imageStream);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(maxWidth, maxHeight),
            Mode = ResizeMode.Max,
        }));

        using var compressedStream = new MemoryStream();
        await GetEncoderAsync(image, compressedStream, fileExtension);
        await DownloadFileAsync(imageFile, compressedStream, selectedPath, fileExtension);
    }

    public async Task ConvertAndDownloadAsync(
        IBrowserFile file,
        string selectedPath,
        string selectedExtension)
    {
        using var stream = file.OpenReadStream(MaxFileSize);
        using var image = await Image.LoadAsync(stream);
        using var convertedStream = new MemoryStream();
        await GetEncoderAsync(image, convertedStream, selectedExtension);

        string filePath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
        using var outputStream = new FileStream(filePath, FileMode.Create);
        convertedStream.Position = 0;
        await convertedStream.CopyToAsync(outputStream);
    }

    private async Task DownloadFileAsync(
        IBrowserFile file,
        MemoryStream convertedImage,
        string selectedPath,
        string selectedExtension)
    {
        string filePath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
        using var outputStream = new FileStream(filePath, FileMode.Create);
        convertedImage.Position = 0;
        await convertedImage.CopyToAsync(outputStream);
    }

    private static async Task GetEncoderAsync(
        Image image,
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

            case ".tga":
                await image.SaveAsTgaAsync(convertedStream, new TgaEncoder());
                break;

            case ".pbm":
                await image.SaveAsPbmAsync(convertedStream, new PbmEncoder());
                break;

            default:
                throw new NotImplementedException($"Unsupported file extension: {selectedExtension}");
        }
    }
}
