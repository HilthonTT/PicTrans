using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using Xceed.Words.NET;

namespace PicTrans.Converters;
public class WordConverter : IWordConverter
{
    private const long MaxFileSize = 1024 * 1024 * 500; //represents 500MB;
    private readonly IFileChecker _fileChecker;

    public WordConverter(IFileChecker fileChecker)
    {
        _fileChecker = fileChecker;
    }

    public async Task<byte[]> ConvertToWordAsync(IBrowserFile inputFile)
    {
        string extension = _fileChecker.GetDocumentExtension(inputFile);
        return extension switch
        {
            ".docx" or ".doc" => default,
            ".pdf" => await ConvertPdfToWordAsync(inputFile),
            ".txt" or ".html" or ".htm" => await ConvertOthersToWordAsync(inputFile),
            _ => throw new NotImplementedException("Unsupported file format.")
        };
    }

    private static async Task<byte[]> ConvertOthersToWordAsync(IBrowserFile inputFile)
    {
        byte[] fileContent = new byte[inputFile.Size];
        await inputFile.OpenReadStream(MaxFileSize).ReadAsync(fileContent);

        byte[] cleanedContent = RemoveInvalidCharacters(fileContent);

        var document = DocX.Create(new MemoryStream());

        string text = Encoding.UTF8.GetString(cleanedContent);
        document.InsertParagraph(text);

        var memoryStream = new MemoryStream();
        document.SaveAs(memoryStream);
        document.Dispose();

        return memoryStream.ToArray();
    }

    private static async Task<byte[]> ConvertPdfToWordAsync(IBrowserFile inputFile)
    {
        using var memoryStream = new MemoryStream();
        await inputFile.OpenReadStream(MaxFileSize).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var pdfReader = new PdfReader(memoryStream);
        var pdfDocument = new PdfDocument(pdfReader);

        using var document = DocX.Create(memoryStream);
        var paragraph = document.InsertParagraph();

        for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
        {
            var page = pdfDocument.GetPage(pageNumber);
            var pdfContent = PdfTextExtractor.GetTextFromPage(page);
            paragraph.Append(pdfContent);
        }

        document.Save();
        document.Dispose();

        return memoryStream.ToArray();
    }

    private static byte[] RemoveInvalidCharacters(byte[] content)
    {
        byte[] cleanedContent = content.Where(b => b >= 32 && b <= 126).ToArray();
        return cleanedContent;
    }
}
