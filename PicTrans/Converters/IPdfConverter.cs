using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Converters;
public interface IPdfConverter
{
    Task ConvertToPdfAsync(IBrowserFile inputFile, string outputPath);
}