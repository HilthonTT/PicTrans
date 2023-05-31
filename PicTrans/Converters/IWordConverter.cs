using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Converters;
public interface IWordConverter
{
    Task<byte[]> ConvertToWordAsync(IBrowserFile inputFile);
}