using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Components.Forms;
using Xceed.Words.NET;
using System.Text;
using HtmlAgilityPack;

namespace PicTrans.Converters;
public class TxtConverter : ITxtConverter
{
    private const long MaxFileSize = 1024 * 1024 * 500; //represents 500MB
    private readonly IFileChecker _fileChecker;

    public TxtConverter(IFileChecker fileChecker)
    {
        _fileChecker = fileChecker;
    }

    public async Task<byte[]> ConvertToTxtAsync(IBrowserFile inputFile)
    {
        string extension = _fileChecker.GetDocumentExtension(inputFile);
        return extension switch
        {
            ".txt" => default,
            ".docx" or ".doc" => await ConvertWordToTxtAsync(inputFile),
            ".html" or ".htm" => await ConvertHtmlToTxtAsync(inputFile),
            ".pdf" => await ConvertPdfToTxtAsync(inputFile),
            _ => throw new NotImplementedException("Unsupported file format."),
        };
    }

    private static async Task<byte[]> ConvertWordToTxtAsync(IBrowserFile inputFile)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        using var memoryStream = new MemoryStream();
        await streamReader.BaseStream.CopyToAsync(memoryStream);

        using var doc = DocX.Load(memoryStream);
        string textContent = doc.Text;

        return Encoding.UTF8.GetBytes(textContent);
    }

    private static async Task<byte[]> ConvertHtmlToTxtAsync(IBrowserFile inputFile)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        string htmlContent = await streamReader.ReadToEndAsync();

        string textContent = ConvertHtmlToPlainText(htmlContent);

        return Encoding.UTF8.GetBytes(textContent);
    }

    private static async Task<byte[]> ConvertPdfToTxtAsync(IBrowserFile inputFile)
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

    private static string ConvertHtmlToPlainText(string htmlContent)
    {
        HtmlDocument htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        StringWriter stringWriter = new StringWriter();
        ConvertToPlainText(htmlDocument.DocumentNode, stringWriter);

        return stringWriter.ToString();
    }

    private static void ConvertToPlainText(HtmlNode node, TextWriter textWriter)
    {
        if (node is null)
        {
            return;
        }

        if (node is HtmlTextNode textNode)
        {
            string text = textNode.Text;
            textWriter.Write(text);
        }
        else if (node is HtmlCommentNode)
        {
            // Ignore HTML comments.
            return;
        }
        else
        {
            foreach (HtmlNode childNode in node.ChildNodes)
            {
                ConvertToPlainText(childNode, textWriter);
            }

            if (IsLineBreakTag(node.Name))
            {
                textWriter.WriteLine();
            }
            else if (IsHeadingTag(node.Name))
            {
                textWriter.WriteLine();
                textWriter.WriteLine(node.InnerText);
                textWriter.WriteLine();
            }
            else if (node.Name == "li")
            {
                textWriter.Write("• ");
                textWriter.WriteLine(node.InnerText);
            }
        }
    }

    private static bool IsLineBreakTag(string tagName)
    {
        return tagName == "p" || tagName == "br";
    }

    private static bool IsHeadingTag(string tagName)
    {
        return tagName.StartsWith("h", StringComparison.OrdinalIgnoreCase) && tagName.Length == 2 && char.IsDigit(tagName[1]);
    }
}
