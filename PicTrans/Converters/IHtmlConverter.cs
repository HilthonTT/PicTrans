using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Converters;
public interface IHtmlConverter
{
    Task<byte[]> ConvertToHtmlAsync(IBrowserFile inputFile);
}