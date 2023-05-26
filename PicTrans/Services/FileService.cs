using Syncfusion.Pdf;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using Xceed.Words.NET;
using Syncfusion.Pdf.Graphics;
using PointF = Syncfusion.Drawing.PointF;

namespace PicTrans.Services;
public class FileService : IFileService
{
    private const long MaxFileSize = 1024 * 1024 * 500; // represents 500MB;
    private readonly IPathService _pathService;

    public FileService(IPathService pathService)
    {
        _pathService = pathService;
    }

    public async Task DownloadFileAsync(
        IBrowserFile file,
        string selectedPath,
        string selectedExtension)
    {
        byte[] convertedFileContent;

        convertedFileContent = await ConvertAsync(file, selectedExtension);

        string filePath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
        await File.WriteAllBytesAsync(filePath, convertedFileContent);
    }

    private static async Task<byte[]> ConvertToPdfAsync(IBrowserFile inputFile)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        string fileContents = await streamReader.ReadToEndAsync();

        using var memoryStream = new MemoryStream();
        using var document = new PdfDocument();

        var page = document.Pages.Add();

        var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
        var textElement = new PdfTextElement(fileContents, font);

        var layoutResult = textElement.Draw(page, new PointF(0, 0));

        document.Save(memoryStream);

        byte[] pdfBytes = memoryStream.ToArray();

        memoryStream.Position = 0;

        return pdfBytes;
    }

    private static async Task<byte[]> ConvertToWordAsync(IBrowserFile inputFile)
    {
        var fileContent = new byte[inputFile.Size];
        await inputFile.OpenReadStream(MaxFileSize).ReadAsync(fileContent);

        var document = DocX.Create(new MemoryStream());

        string text = Encoding.UTF8.GetString(fileContent);
        document.InsertParagraph(text);

        var memoryStream = new MemoryStream();
        document.SaveAs(memoryStream);
        document.Dispose();

        return memoryStream.ToArray();
    }

    private static async Task<byte[]> ConvertToTxtAsync(IBrowserFile inputFile)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        using var memoryStream = new MemoryStream();
        await streamReader.BaseStream.CopyToAsync(memoryStream);

        using var doc = DocX.Load(memoryStream);

        string textContent = doc.Text;

        return Encoding.UTF8.GetBytes(textContent);
    }

    private static async Task<byte[]> ConvertAsync(
        IBrowserFile file,
        string selectedExtension)
    {
        return selectedExtension switch
        {
            ".pdf" => await ConvertToPdfAsync(file),
            ".docx" => await ConvertToWordAsync(file),
            ".txt" => await ConvertToTxtAsync(file),
            _ => throw new NotImplementedException($"Unsupported file type: {selectedExtension}"),
        };
    }
}
