using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using Xceed.Words.NET;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout;
using iText.Kernel.Geom;
using HtmlConverterHelper = iText.Html2pdf.HtmlConverter;

namespace PicTrans.Converters;
public class PdfConverter : IPdfConverter
{
    private const long MaxFileSize = 1024 * 1024 * 500; // represents 500MB
    private readonly IFileChecker _fileChecker;

    public PdfConverter(IFileChecker fileChecker)
    {
        _fileChecker = fileChecker;
    }

    public async Task ConvertToPdfAsync(IBrowserFile inputFile, string outputPath)
    {
        string extension = _fileChecker.GetDocumentExtension(inputFile);
        switch (extension)
        {
            case ".pdf":
                break;
            case ".docx" or ".doc":
                await ConvertWordToPdfAsync(inputFile, outputPath);
                break;
            case ".html" or ".htm":
                await ConvertHtmlToPdfAsync(inputFile, outputPath);
                break;
            case ".txt":
                await ConvertTxtToPdfAsync(inputFile, outputPath);
                break;
            default:
                throw new NotImplementedException("Unsupported file format.");
        }
    }

    private static async Task ConvertWordToPdfAsync(IBrowserFile inputFile, string outputPath)
    {
        using var pdfWriter = new PdfWriter(outputPath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        var page = pdfDocument.AddNewPage();

        using var inputStream = new MemoryStream();
        await inputFile.OpenReadStream(MaxFileSize).CopyToAsync(inputStream);
        inputStream.Position = 0;

        using var wordDocument = DocX.Load(inputStream);
        string wordText = wordDocument.Text;

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var text = new Paragraph(wordText).SetFont(font).SetFontSize(12f);
        var document = new Document(pdfDocument, PageSize.A4);
        document.Add(text);
        document.Close();
    }

    private static async Task ConvertHtmlToPdfAsync(IBrowserFile inputFile, string outputPath)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        string fileContents = await streamReader.ReadToEndAsync();

        using var pdfWriter = new PdfWriter(outputPath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        var page = pdfDocument.AddNewPage();

        using var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));
        var converterProperties = new ConverterProperties();
        HtmlConverterHelper.ConvertToPdf(htmlStream, pdfDocument, converterProperties);
    }

    private static async Task ConvertTxtToPdfAsync(IBrowserFile inputFile, string outputPath)
    {
        using var streamReader = new StreamReader(inputFile.OpenReadStream(MaxFileSize));
        string fileContents = await streamReader.ReadToEndAsync();

        using var pdfWriter = new PdfWriter(outputPath);
        using var pdfDocument = new PdfDocument(pdfWriter);
        var page = pdfDocument.AddNewPage();

        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var text = new Paragraph(fileContents).SetFont(font).SetFontSize(12f);
        var document = new Document(pdfDocument, PageSize.A4);
        document.Add(text);
        document.Close();
    }
}
