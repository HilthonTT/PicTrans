using System.Net;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using Xceed.Words.NET;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout;
using iText.Kernel.Geom;
using System.Web;
using HtmlAgilityPack;
using Path = System.IO.Path;

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

        convertedFileContent = await ConvertAsync(file, selectedPath, selectedExtension);

        if (convertedFileContent is not null)
        {
            string filePath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
            await File.WriteAllBytesAsync(filePath, convertedFileContent);
        }
    }

    private async Task<byte[]> ConvertToPdfAsync(IBrowserFile inputFile, string outputPath)
    {
        if (IsPdfDocument(inputFile))
        {
            return default;
        }

        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        string fileContents = await streamReader.ReadToEndAsync();

        using var pdfWriter = new PdfWriter(outputPath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        var page = pdfDocument.AddNewPage();

        if (IsWordDocument(inputFile))
        {
            using var inputStream = new MemoryStream();
            await inputFile.OpenReadStream(MaxFileSize).CopyToAsync(inputStream);
            inputStream.Position = 0;

            using (var wordDocument = DocX.Load(inputStream))
            {
                string wordText = wordDocument.Text;

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var text = new Paragraph(wordText).SetFont(font).SetFontSize(12f);
                var document = new Document(pdfDocument, PageSize.A4);
                document.Add(text);
                document.Close();
            }
        }
        else if (IsHtmlDocument(inputFile))
        {
            using var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));
            var converterProperties = new ConverterProperties();
            HtmlConverter.ConvertToPdf(htmlStream, pdfDocument, converterProperties);
        }
        else
        {
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var text = new Paragraph(fileContents).SetFont(font).SetFontSize(12f);
            var document = new Document(pdfDocument, PageSize.A4);
            document.Add(text);
            document.Close();
        }

        return default;
    }

    private async Task<byte[]> ConvertToWordAsync(IBrowserFile inputFile)
    {
        if (IsWordDocument(inputFile))
        {
            return default;
        }

        if (IsPdfDocument(inputFile))
        {
            return await ConvertPdfToWordAsync(inputFile);
        }

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

    private async Task<byte[]> ConvertToTxtAsync(IBrowserFile inputFile)
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

            string textContent = ConvertHtmlToPlainText(htmlContent);

            return Encoding.UTF8.GetBytes(textContent);
        }
        else
        {
            throw new NotSupportedException("Unsupported file format.");
        }
    }

    private async Task<byte[]> ConvertToHtmlAsync(IBrowserFile inputFile)
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

    private async Task<byte[]> ConvertAsync(
        IBrowserFile file,
        string selectedPath,
        string selectedExtension)
    {
        string outputPath = _pathService.GetFilePath(file, selectedPath, selectedExtension);
        return selectedExtension switch
        {
            ".pdf" => await ConvertToPdfAsync(file, outputPath),
            ".docx" => await ConvertToWordAsync(file),
            ".txt" => await ConvertToTxtAsync(file),
            ".html" => await ConvertToHtmlAsync(file),
            _ => throw new NotImplementedException($"Unsupported file type: {selectedExtension}"),
        };
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
        return extension == ".html" || extension == ".htm";
    }
}