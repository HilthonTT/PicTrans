using Microsoft.AspNetCore.Components.Forms;
using PicTrans.Converters;

namespace PicTrans.Services;
public class FileService : IFileService
{
    private readonly IPathService _pathService;
    private readonly IHtmlConverter _htmlConverter;
    private readonly IPdfConverter _pdfConverter;
    private readonly IWordConverter _wordConverter;
    private readonly ITxtConverter _txtConverter;

    public FileService(IPathService pathService,
                       IHtmlConverter htmlConverter,
                       IPdfConverter pdfConverter,
                       IWordConverter wordConverter,
                       ITxtConverter txtConverter)
    {
        _pathService = pathService;
        _htmlConverter = htmlConverter;
        _pdfConverter = pdfConverter;
        _wordConverter = wordConverter;
        _txtConverter = txtConverter;
    }

    public async Task DownloadFileAsync(
        IBrowserFile file,
        string selectedPath,
        string selectedExtension)
    {
        byte[] convertedFileContent;

        convertedFileContent = await ConvertAsync(file, selectedPath, selectedExtension);

        if (convertedFileContent is not null)
        {
            string filePath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
            await File.WriteAllBytesAsync(filePath, convertedFileContent);
        }
    }

    private async Task<byte[]> ConvertAsync(IBrowserFile inputFile, string selectedPath, string selectedExtension)
    {
        string outputPath = _pathService.GetFilePath(inputFile, selectedPath, selectedExtension);

        switch (selectedExtension)
        {
            case ".pdf":
                await _pdfConverter.ConvertToPdfAsync(inputFile, outputPath);
                return default;
            case ".docx" or ".doc":
                return await _wordConverter.ConvertToWordAsync(inputFile);
            case ".txt":
                return await _txtConverter.ConvertToTxtAsync(inputFile);
            case ".html" or ".htm":
                return await _htmlConverter.ConvertToHtmlAsync(inputFile);
            default: 
                throw new NotImplementedException("Unsupported file format.");
        }
    }
}