using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using Xceed.Words.NET;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
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
        using var document = new Syncfusion.Pdf.PdfDocument();
        var page = document.Pages.Add();

        if (IsWordDocument(inputFile))
        {
            using var inputStream = new MemoryStream();
            await inputFile.OpenReadStream(MaxFileSize).CopyToAsync(inputStream);
            inputStream.Position = 0;

            using var wordDocument = DocX.Load(inputStream);

            string wordText = wordDocument.Text;

            var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            var textElement = new PdfTextElement(wordText, font);
            var layoutResult = textElement.Draw(page, new PointF(0, 0));
        }
        else
        {
            var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
            var textElement = new PdfTextElement(fileContents, font);
            var layoutResult = textElement.Draw(page, new PointF(0, 0));
        }

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
        if (IsWordDocument(inputFile))
        {
            using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
            using var memoryStream = new MemoryStream();
            await streamReader.BaseStream.CopyToAsync(memoryStream);

            using var doc = DocX.Load(memoryStream);
            string textContent = doc.Text;

            return Encoding.UTF8.GetBytes(textContent);
        }
        else if (IsPdfDocument(inputFile))
        {
            using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
            using var memoryStream = new MemoryStream();
            await streamReader.BaseStream.CopyToAsync(memoryStream);

            var textContent = new StringBuilder();
            memoryStream.Position = 0;
            var pdfReader = new PdfReader(memoryStream);
            var pdfDocument = new PdfDocument(pdfReader);
            var strategy = new LocationTextExtractionStrategy();

            for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
            {
                var currentPage = pdfDocument.GetPage(page);
                string text = PdfTextExtractor.GetTextFromPage(currentPage, strategy);
                textContent.Append(text);
            }

            return Encoding.UTF8.GetBytes(textContent.ToString());
        }
        else
        {
            throw new NotSupportedException("Unsupported file format.");
        }
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

    private static bool IsWordDocument(IBrowserFile inputFile)
    {
        var extension = Path.GetExtension(inputFile.Name);
        return extension == ".docx" || extension == ".doc";
    }

    private static bool IsPdfDocument(IBrowserFile inputFile)
    {
        var extension = Path.GetExtension(inputFile.Name);
        return extension == ".pdf";
    }
}
