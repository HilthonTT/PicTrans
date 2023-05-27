using System.Net;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using Xceed.Words.NET;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Syncfusion.Pdf.Graphics;
using PointF = Syncfusion.Drawing.PointF;
using System.Web;
using HtmlAgilityPack;

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

        if (convertedFileContent is not null)
        {
            string filePath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
            await File.WriteAllBytesAsync(filePath, convertedFileContent);
        }
    }

    private static async Task<byte[]> ConvertToPdfAsync(IBrowserFile inputFile)
    {
        if (IsPdfDocument(inputFile))
        {
            return default;
        }

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
        else if (IsHtmlDocument(inputFile))
        {
            return default;
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
        if (IsWordDocument(inputFile))
        {
            return default;
        }

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
        if (IsTextDocument(inputFile))
        {
            return default;
        }

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
        else if (IsHtmlDocument(inputFile))
        {
            using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
            string htmlContent = await streamReader.ReadToEndAsync();

            // Convert HTML to plain text using a suitable library or method.
            string textContent = ConvertHtmlToPlainText(htmlContent);

            return Encoding.UTF8.GetBytes(textContent);
        }
        else
        {
            throw new NotSupportedException("Unsupported file format.");
        }
    }

    private static async Task<byte[]> ConvertToHtmlAsync(IBrowserFile inputFile)
    {
        if (IsHtmlDocument(inputFile))
        {
            return default;
        }

        if (IsWordDocument(inputFile))
        {
            using var inputStream = new MemoryStream();
            await inputFile.OpenReadStream(MaxFileSize).CopyToAsync(inputStream);
            inputStream.Position = 0;

            using var doc = DocX.Load(inputStream);
            string plainTextContent = doc.Text;
            string htmlContent = $"<p>{HttpUtility.HtmlEncode(plainTextContent)}</p>";

            return Encoding.UTF8.GetBytes(htmlContent);
        }
        else if (IsPdfDocument(inputFile))
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
        else if (IsTextDocument(inputFile))
        {
            using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
            string textContent = await streamReader.ReadToEndAsync();
            string htmlContent = $"<pre>{WebUtility.HtmlEncode(textContent)}</pre>";

            return Encoding.UTF8.GetBytes(htmlContent);
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
            ".html" => await ConvertToHtmlAsync(file),
            _ => throw new NotImplementedException($"Unsupported file type: {selectedExtension}"),
        };
    }

    private static bool IsTextDocument(IBrowserFile inputFile)
    {
        string extension = Path.GetExtension(inputFile.Name);
        return extension == ".txt";
    }

    private static bool IsWordDocument(IBrowserFile inputFile)
    {
        string extension = Path.GetExtension(inputFile.Name);
        return extension == ".docx" || extension == ".doc";
    }

    private static bool IsPdfDocument(IBrowserFile inputFile)
    {
        string extension = Path.GetExtension(inputFile.Name);
        return extension == ".pdf";
    }

    private static bool IsHtmlDocument(IBrowserFile inputFile)
    {
        string extension = Path.GetExtension(inputFile.Name);
        return extension == ".html";
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
        if (node == null)
            return;

        if (node is HtmlTextNode)
        {
            string text = ((HtmlTextNode)node).Text;
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

            if (node.Name == "p" || node.Name == "br")
            {
                // Add line breaks for <p> and <br> tags.
                textWriter.WriteLine();
            }
            else if (node.Name == "h1" || node.Name == "h2" || node.Name == "h3" || node.Name == "h4" || node.Name == "h5" || node.Name == "h6")
            {
                // Add line breaks and formatting for heading tags.
                textWriter.WriteLine();
                textWriter.WriteLine(node.InnerText);
                textWriter.WriteLine();
            }
            else if (node.Name == "li")
            {
                // Add bullets for list items.
                textWriter.Write("• ");
                textWriter.WriteLine(node.InnerText);
            }
        }
    }
}