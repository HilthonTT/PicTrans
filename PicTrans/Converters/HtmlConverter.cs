using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Text;
using System.Web;
using Xceed.Words.NET;

namespace PicTrans.Converters;
public class HtmlConverter : IHtmlConverter
{
    private const long MaxFileSize = 1024 * 1024 * 500; // represents 500MB
    private readonly IFileChecker _fileChecker;

    public HtmlConverter(IFileChecker fileChecker)
    {
        _fileChecker = fileChecker;
    }

    public async Task<byte[]> ConvertToHtmlAsync(IBrowserFile inputFile)
    {
        string extension = _fileChecker.GetDocumentExtension(inputFile);
        return extension switch
        {
            ".html" or ".htm" => ConvertHtmlToHtml(),
            ".txt" => await ConvertTxtToHtmlAsync(inputFile),
            ".docx" or ".doc" => await ConvertWordToAsyncAsync(inputFile),
            ".pdf" => await ConvertPdfToHtmlAsync(inputFile),
            _ => throw new NotImplementedException("Unsupported file format."),
        };
    }

    private static byte[] ConvertHtmlToHtml()
    {
        return default;
    }

    private static async Task<byte[]> ConvertWordToAsyncAsync(IBrowserFile inputFile)
    {
        using var inputStream = new MemoryStream();
        await inputFile.OpenReadStream(MaxFileSize).CopyToAsync(inputStream);
        inputStream.Position = 0;

        using var doc = DocX.Load(inputStream);
        string plainTextContent = doc.Text;
        string htmlContent = $"<p>{HttpUtility.HtmlEncode(plainTextContent)}</p>";

        return Encoding.UTF8.GetBytes(htmlContent);
    }

    private static async Task<byte[]> ConvertPdfToHtmlAsync(IBrowserFile inputFile)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        using var memoryStream = new MemoryStream();
        await streamReader.BaseStream.CopyToAsync(memoryStream);

        var htmlContent = new StringBuilder();
        memoryStream.Position = 0;
        var pdfReader = new PdfReader(memoryStream);
        var pdfDocument = new PdfDocument(pdfReader);

        for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
        {
            var currentPage = pdfDocument.GetPage(page);
            var strategy = new LocationTextExtractionStrategy();
            string text = PdfTextExtractor.GetTextFromPage(currentPage, strategy);
            htmlContent.Append("<div>").Append(text).Append("</div>");
        }

        return Encoding.UTF8.GetBytes(htmlContent.ToString());
    }

    private static async Task<byte[]> ConvertTxtToHtmlAsync(IBrowserFile inputFile)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        string textContent = await streamReader.ReadToEndAsync();
        string htmlContent = $"<pre>{WebUtility.HtmlEncode(textContent)}</pre>";

        return Encoding.UTF8.GetBytes(htmlContent);
    }
}
